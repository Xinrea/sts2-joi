using BaseLib.Utils;
using Joi.JoiCode.Character;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace Joi.JoiCode.Cards;

[Pool(typeof(JoiCardPool))]
public class QuantumSuperposition : JoiCard
{
    public QuantumSuperposition() : base(2, CardType.Skill, CardRarity.Uncommon, TargetType.Self) { }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new CardsVar(2)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var handPile = CardPile.Get(PileType.Hand, Owner);
        var eligibleCount = handPile?.Cards.Count(card => !card.EnergyCost.CostsX && card.EnergyCost.GetResolved() <= 1) ?? 0;
        if (eligibleCount == 0)
        {
            return;
        }

        var maxSelect = Math.Min(DynamicVars.Cards.IntValue, eligibleCount);
        var prefs = new CardSelectorPrefs(new LocString("cards", "JOI-QUANTUM_SUPERPOSITION.selectionPrompt"), 1, maxSelect)
        {
            Cancelable = true
        };

        var selectedCards = await CardSelectCmd.FromHand(
            choiceContext,
            Owner,
            prefs,
            card => !card.EnergyCost.CostsX && card.EnergyCost.GetResolved() <= 1,
            this);

        foreach (var selectedCard in selectedCards)
        {
            var copy = selectedCard.CreateDupe();
            await CardPileCmd.AddGeneratedCardToCombat(copy, PileType.Hand, true, CardPilePosition.Random);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Cards.UpgradeValueBy(1);
    }
}
