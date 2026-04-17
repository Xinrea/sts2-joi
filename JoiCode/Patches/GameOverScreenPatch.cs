using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Screens.GameOverScreen;
using MegaCrit.Sts2.Core.Nodes.Combat;
using System.Collections.Generic;
using System.Linq;
using Godot;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.RestSite;
using MegaCrit.Sts2.Core.Nodes.Screens.Shops;
using MegaCrit.Sts2.Core.Rooms;

namespace Joi.JoiCode.Patches;

[HarmonyPatch(typeof(NGameOverScreen), "MoveCreaturesToDifferentLayerAndDisableUi")]
public static class GameOverScreenPatch
{
    [HarmonyPrefix]
    static bool Prefix(NGameOverScreen __instance)
    {
        // 获取私有字段
        var runStateField = AccessTools.Field(typeof(NGameOverScreen), "_runState");
        var creatureContainerField = AccessTools.Field(typeof(NGameOverScreen), "_creatureContainer");

        var runState = (RunState?)runStateField.GetValue(__instance);
        var creatureContainer = (Control?)creatureContainerField.GetValue(__instance);

        if (runState == null || creatureContainer == null)
        {
            return true; // 如果获取字段失败，执行原始方法
        }

        List<NCreatureVisuals> list = new List<NCreatureVisuals>();

        if (NCombatRoom.Instance != null)
        {
            if (NCombatRoom.Instance.Mode == CombatRoomMode.ActiveCombat)
            {
                NCombatRoom.Instance.Ui.AnimOut();
            }
            var list2 = NCombatRoom.Instance.CreatureNodes.ToList();
            list = list2.Select((NCreature c) => c.Visuals).ToList();
        }
        else if (NMerchantRoom.Instance != null)
        {
            foreach (NMerchantCharacter playerVisual in NMerchantRoom.Instance.PlayerVisuals)
            {
                playerVisual.PlayAnimation("die");
                playerVisual.Reparent(creatureContainer);
            }
        }
        else if (NRestSiteRoom.Instance != null)
        {
            list = new List<NCreatureVisuals>();
            foreach (var player in runState.Players)
            {
                NCreatureVisuals? nCreatureVisuals = player.Creature.CreateVisuals();
                if (nCreatureVisuals == null) continue;

                list.Add(nCreatureVisuals);
                creatureContainer.AddChild(nCreatureVisuals);

                // 添加空值检查 - 只有 Spine 动画才调用
                if (nCreatureVisuals.SpineBody != null)
                {
                    nCreatureVisuals.SpineBody.GetAnimationState().SetAnimation("die", loop: false);
                }

                NRestSiteCharacter? characterForPlayer = NRestSiteRoom.Instance.GetCharacterForPlayer(player);
                if (characterForPlayer != null)
                {
                    nCreatureVisuals.GlobalPosition = characterForPlayer.GlobalPosition;
                    nCreatureVisuals.Scale = characterForPlayer.Scale;
                    characterForPlayer.Visible = false;
                    Vector2 vector = new Vector2(100f, 100f);
                    nCreatureVisuals.Position += vector * new Vector2(Mathf.Sign(nCreatureVisuals.Scale.X), Mathf.Sign(nCreatureVisuals.Scale.Y));
                }
            }
        }
        else
        {
            list = new List<NCreatureVisuals>();
            foreach (var player2 in runState.Players)
            {
                NCreatureVisuals? nCreatureVisuals2 = player2.Creature.CreateVisuals();
                if (nCreatureVisuals2 == null) continue;

                list.Add(nCreatureVisuals2);
                creatureContainer.AddChild(nCreatureVisuals2);

                // 添加空值检查 - 只有 Spine 动画才调用
                if (nCreatureVisuals2.SpineBody != null)
                {
                    nCreatureVisuals2.SpineBody.GetAnimationState().SetAnimation("die", loop: false);
                }
            }
        }

        // 跳过原始方法
        return false;
    }
}
