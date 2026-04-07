using BaseLib.Utils;
using Joi.JoiCode.Character;
using Joi.JoiCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace Joi.JoiCode.Cards;

[Pool(typeof(JoiCardPool))]
public class Reboot : JoiCard
{
    public Reboot() : base(2, CardType.Skill, CardRarity.Uncommon, TargetType.Self) { }

    protected override IEnumerable<DynamicVar> CanonicalVars => [];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var blackHole = Owner.Creature.GetPower<BlackHolePower>();
        var whiteHole = Owner.Creature.GetPower<WhiteHolePower>();
        if (blackHole != null) await PowerCmd.Remove(blackHole);
        if (whiteHole != null) await PowerCmd.Remove(whiteHole);

        var hand = Owner.PlayerCombatState?.Hand;
        if (hand == null) return;

        var cardsToDiscard = hand.Cards.Where(c => c != this).ToList();
        int count = cardsToDiscard.Count;

        foreach (var card in cardsToDiscard)
        {
            await CommonActions.MoveCardToPile(this, card, PileType.Discard, CardPilePosition.Random, false);
        }

        if (count > 0)
        {
            await CardPileCmd.Draw(choiceContext, count, Owner);
        }
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
