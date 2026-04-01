using BaseLib.Utils;
using Joi.JoiCode.Character;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;

namespace Joi.JoiCode.Cards;

[Pool(typeof(JoiCardPool))]
public class OnlyOrange : JoiCard
{
    public OnlyOrange() : base(2, CardType.Skill, CardRarity.Uncommon, TargetType.Self) { }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromCard<Orange>(IsUpgraded)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var hand = Owner.PlayerCombatState?.Hand;
        if (hand == null) return;

        var cardsToExhaust = hand.Cards.Where(c => c != this).ToList();
        var count = cardsToExhaust.Count;

        // 消耗所有其他手牌
        foreach (var card in cardsToExhaust)
        {
            await CardCmd.Exhaust(choiceContext, card, false, false);
        }

        if (count == 0)
        {
            return;
        }

        // 生成橘子卡
        var combatState = Owner.Creature?.CombatState;
        if (combatState == null) return;

        var oranges = new List<CardModel>();

        for (int i = 0; i < count; i++)
        {
            var orange = combatState.CreateCard<Orange>(Owner);
            if (IsUpgraded)
            {
                orange.UpgradeInternal();
            }
            oranges.Add(orange);
        }

        await CardPileCmd.AddGeneratedCardsToCombat(oranges, PileType.Hand, true, CardPilePosition.Random);
    }

    protected override void OnUpgrade()
    {
        // Upgrade changes the generated cards to Orange+
    }
}
