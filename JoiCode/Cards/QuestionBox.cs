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
public class QuestionBox : JoiCard
{
    public QuestionBox() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self) { }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new CardsVar(1)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var handPile = CardPile.Get(PileType.Hand, Owner);
        var eligibleCount = handPile?.Cards.Count(card => !card.EnergyCost.CostsX) ?? 0;
        if (eligibleCount == 0)
        {
            return;
        }

        var maxSelect = Math.Min(DynamicVars.Cards.IntValue, eligibleCount);
        var prefs = new CardSelectorPrefs(new LocString("cards", "JOI-QUESTION_BOX.selectionPrompt"), 1, maxSelect)
        {
            Cancelable = true
        };

        var selectedCards = await CardSelectCmd.FromHand(
            choiceContext,
            Owner,
            prefs,
            card => !card.EnergyCost.CostsX,
            this);

        foreach (var selectedCard in selectedCards)
        {
            selectedCard.EnergyCost.SetThisTurn(0, reduceOnly: false);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Cards.UpgradeValueBy(1);
    }
}
