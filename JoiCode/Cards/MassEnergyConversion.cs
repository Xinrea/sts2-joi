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
public class MassEnergyConversion : JoiCard
{
    public MassEnergyConversion() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self) { }

    protected override IEnumerable<DynamicVar> CanonicalVars => [];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var handPile = CardPile.Get(PileType.Hand, Owner);
        if (handPile == null || handPile.Cards.Count == 0) return;

        // 过滤掉X费卡（EnergyCost为-1的卡）
        var validCards = handPile.Cards.Where(c => !c.EnergyCost.CostsX).ToList();
        if (validCards.Count == 0) return;

        var prefs = new CardSelectorPrefs(new LocString("cards", "JOI-MASS_ENERGY_CONVERSION.selectionPrompt"), 1);
        var selectedCards = await CardSelectCmd.FromSimpleGrid(choiceContext, validCards, Owner, prefs);
        var card = selectedCards.FirstOrDefault();

        if (card != null)
        {
            var energyCost = card.EnergyCost.GetWithModifiers(CostModifiers.None);

            await CardCmd.Exhaust(choiceContext, card);

            if (energyCost > 0)
            {
                await PlayerCmd.GainEnergy(energyCost, Owner);
            }
        }
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
