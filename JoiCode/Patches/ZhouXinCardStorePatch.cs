using BaseLib.Utils;
using Godot;
using HarmonyLib;
using Joi.JoiCode.Minions;
using Joi.JoiCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace Joi.JoiCode.Patches;

public static class ZhouXinCardStorePatch
{
    public static bool IsLivingZhouXin(Creature? creature)
    {
        return creature != null
            && creature.IsAlive
            && creature.Monster is ZhouXin;
    }

    /// <summary>
    /// 轴芯是否还能接受卡牌（活着且没有已存储的卡牌）
    /// </summary>
    public static bool CanAcceptCard(Creature? zhouXin)
    {
        if (!IsLivingZhouXin(zhouXin)) return false;
        var power = zhouXin!.GetPower<StoredCardPower>();
        return power == null || !power.HasStoredCard;
    }

    // ========== Patch 1: IsValidTarget ==========
    [HarmonyPatch(typeof(CardModel), nameof(CardModel.IsValidTarget))]
    public static class IsValidTargetPatch
    {
        [HarmonyPostfix]
        static void Postfix(CardModel __instance, Creature? target, ref bool __result)
        {
            if (__result || !CanAcceptCard(target))
                return;
            if (__instance.Owner?.Creature?.Side != target!.Side)
                return;
            __result = true;
        }
    }

    // ========== Patch 2: AllowedToTargetCreature ==========
    [HarmonyPatch(typeof(NTargetManager), "AllowedToTargetCreature")]
    public static class AllowedToTargetCreaturePatch
    {
        [HarmonyPostfix]
        static void Postfix(Creature creature, ref bool __result)
        {
            if (!__result && CanAcceptCard(creature))
            {
                __result = true;
            }
        }
    }

    // ========== Patch 3: ExecuteAction ==========
    [HarmonyPatch(typeof(PlayCardAction), "ExecuteAction")]
    public static class ExecuteActionPatch
    {
        [HarmonyPrefix]
        static bool Prefix(PlayCardAction __instance, ref Task __result)
        {
            var target = __instance.Target;
            if (!CanAcceptCard(target))
                return true;

            __result = StoreCardToZhouXin(__instance);
            return false;
        }

        private static async Task StoreCardToZhouXin(PlayCardAction action)
        {
            var card = action.NetCombatCard.ToCardModel();

            NCardPlayQueue.Instance?.UpdateCardBeforeExecution(action);

            var target = await action.Player.Creature.CombatState.GetCreatureAsync(action.TargetId, 10.0);

            if (!CanAcceptCard(target))
                return;

            var pile = card.Pile;
            if (pile == null || pile.Type != PileType.Hand)
            {
                NCardPlayQueue.Instance?.RemoveCardFromQueueForCancellation(action);
                return;
            }

            MainFile.Logger.Info($"[ZhouXinStore] Storing card {card.Id} to Axle Core");

            // 获取或创建 StoredCardPower
            var power = target!.GetPower<StoredCardPower>();
            if (power == null)
            {
                await PowerCmd.Apply<StoredCardPower>(target, 1, card.Owner.Creature, null);
                power = target.GetPower<StoredCardPower>();
            }

            power?.SetStoredCard(card);

            // 从手牌移到 Play 堆（处理 UI 动画）
            await CardPileCmd.AddDuringManualCardPlay(card);

            // 销毁 AddDuringManualCardPlay 创建的 Play 堆 NCard（它有 tween 动画会残留）
            var playCard = NCard.FindOnTable(card);
            playCard?.QueueFree();

            // 移到 Exhaust 堆隐藏
            await CardPileCmd.Add(card, PileType.Exhaust, CardPilePosition.Bottom, null, true);

            // 在轴芯头顶创建新的 NCard 展示
            var creatureNode = NCombatRoom.Instance?.GetCreatureNode(target);
            var storedCardPos = creatureNode?.GetSpecialNode<Marker2D>("%StoredCardPos");
            if (storedCardPos != null)
            {
                var displayCard = NCard.Create(card);
                if (displayCard != null)
                {
                    storedCardPos.AddChild(displayCard);
                    displayCard.Position = -displayCard.Size * 0.5f;
                    displayCard.UpdateVisuals(PileType.Hand, CardPreviewMode.Normal);
                }
            }

            VfxCmd.PlayOnCreature(target, VfxCmd.healPath);
        }
    }
}
