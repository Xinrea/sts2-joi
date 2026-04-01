using BaseLib.Utils;
using Joi.JoiCode.Minions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace Joi.JoiCode.Enchantments;

/// <summary>
/// 喂食附魔：费用减1，当卡牌被抽到时，如果轴芯在场，自动打出
/// </summary>
public class FeedEnchantment : EnchantmentModel
{
    public override bool HasExtraCardText => true;

    protected override void OnEnchant()
    {
        if (!Card.EnergyCost.CostsX)
        {
            Card.EnergyCost.AddThisCombat(-1, false);
        }
    }

    public override async Task AfterCardDrawn(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
    {
        if (card != Card)
        {
            return;
        }

        if (card.Pile?.Type != PileType.Hand)
        {
            return;
        }

        // 检查轴芯是否在场
        var definition = SummonDefinition.For<ZhouXin>(uniqueKey: "zhou-xin-core");
        var zhouXin = SummonActions.FindExistingSummon(card.Owner.Creature, definition);

        if (zhouXin == null || !zhouXin.IsAlive)
        {
            return;
        }

        MainFile.Logger.Info($"[FeedEnchantment] Auto-playing fed card {card.Id}");
        await CardCmd.AutoPlay(choiceContext, card, null);
    }
}
