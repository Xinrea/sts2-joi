using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.RestSite;
using MegaCrit.Sts2.Core.Nodes.Screens.Shops;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Context;
using Godot;
using BaseLib;
using System;
using System.Collections.Generic;

namespace Joi.JoiCode.Patches;

/// <summary>
/// 拦截 NRestSiteCharacter.Create，为轴伊加载自定义静态图片场景
/// </summary>
[HarmonyPatch(typeof(NRestSiteCharacter), nameof(NRestSiteCharacter.Create))]
public static class JoiRestSiteCreatePatch
{
    [HarmonyPrefix]
    static bool Prefix(Player player, int characterIndex, ref NRestSiteCharacter __result)
    {
        if (!player.Character.Id.Entry.Contains("JOI"))
            return true;

        try
        {
            var scenePath = player.Character.RestSiteAnimPath;
            var scene = GD.Load<PackedScene>(scenePath);
            var instance = scene.Instantiate<Node2D>();

            var wrapper = new JoiRestSiteCharacter();
            wrapper.AddChild(instance);

            AccessTools.Field(typeof(NRestSiteCharacter), "_player")?.SetValue(wrapper, player);
            AccessTools.Field(typeof(NRestSiteCharacter), "_characterIndex")?.SetValue(wrapper, characterIndex);

            __result = wrapper;
            return false;
        }
        catch (Exception e)
        {
            MainFile.Logger.Error($"[JOI] Failed to create custom rest site: {e.Message}");
            return true;
        }
    }
}

/// <summary>
/// 拦截 NMerchantRoom.AfterRoomIsLoaded，为轴伊加载自定义静态图片场景
/// </summary>
[HarmonyPatch(typeof(NMerchantRoom), "AfterRoomIsLoaded")]
public static class JoiMerchantRoomPatch
{
    [HarmonyPrefix]
    static bool Prefix(NMerchantRoom __instance)
    {
        var playersField = AccessTools.Field(typeof(NMerchantRoom), "_players");
        var players = playersField?.GetValue(__instance) as List<Player>;
        if (players == null) return true;

        // 检查是否有轴伊玩家
        bool hasJoi = false;
        foreach (var p in players)
        {
            if (p.Character.Id.Entry.Contains("JOI"))
            {
                hasJoi = true;
                break;
            }
        }
        if (!hasJoi) return true;

        // 有轴伊玩家，完全替换 AfterRoomIsLoaded 逻辑
        try
        {
            var playerVisualsField = AccessTools.Field(typeof(NMerchantRoom), "_playerVisuals");
            var characterContainerField = AccessTools.Field(typeof(NMerchantRoom), "_characterContainer");

            var playerVisuals = playerVisualsField?.GetValue(__instance) as List<NMerchantCharacter>;
            var characterContainer = characterContainerField?.GetValue(__instance) as Control;

            if (playerVisuals == null || characterContainer == null)
            {
                MainFile.Logger.Error("[JOI] Failed to get merchant room fields");
                return true;
            }

            // 复制原版逻辑：重新排列玩家列表，本地玩家放第一
            var me = LocalContext.GetMe(players);
            players.Remove(me);
            players.Insert(0, me);

            int num = Mathf.CeilToInt(Mathf.Sqrt(players.Count));
            for (int i = 0; i < num; i++)
            {
                float num2 = -140f * (float)i;
                for (int j = 0; j < num; j++)
                {
                    int idx = i * num + j;
                    if (idx >= players.Count) break;

                    var player = players[idx];
                    NMerchantCharacter character;

                    if (player.Character.Id.Entry.Contains("JOI"))
                    {
                        // 轴伊：使用自定义场景
                        var scenePath = player.Character.MerchantAnimPath;
                        var scene = GD.Load<PackedScene>(scenePath);
                        var sceneInstance = scene.Instantiate<Node2D>();

                        var wrapper = new JoiMerchantCharacter();
                        wrapper.AddChild(sceneInstance);
                        character = wrapper;
                    }
                    else
                    {
                        // 其他角色：使用原版方式
                        character = PreloadManager.Cache.GetScene(player.Character.MerchantAnimPath)
                            .Instantiate<NMerchantCharacter>(PackedScene.GenEditState.Disabled);
                    }

                    characterContainer.AddChild(character);
                    characterContainer.MoveChild(character, 0);
                    character.Position = new Vector2(num2, -50f * (float)i);
                    if (i > 0)
                        character.Modulate = new Color(0.5f, 0.5f, 0.5f);
                    num2 -= 275f;
                    playerVisuals.Add(character);
                }
            }

            return false; // 跳过原方法
        }
        catch (Exception e)
        {
            MainFile.Logger.Error($"[JOI] Failed in merchant room patch: {e.Message}");
            return true;
        }
    }
}

public partial class JoiRestSiteCharacter : NRestSiteCharacter
{
    public override void _Ready() { }
}

public partial class JoiMerchantCharacter : NMerchantCharacter
{
    public override void _Ready() { }
}
