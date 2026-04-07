using BaseLib.Utils;
using Joi.JoiCode.Character;
using Joi.JoiCode.Minions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace Joi.JoiCode.Cards;

[Pool(typeof(JoiCardPool))]
public class SorryBuddy : JoiCard
{
    private const string ZhouXinSummonKey = "zhou-xin-core";

    public SorryBuddy() : base(0, CardType.Skill, CardRarity.Common, TargetType.Self) { }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new CardsVar(1),
        new DynamicVar("BonusCards", 1),
        new EnergyVar(1)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.IntValue, Owner);

        var definition = ZhouXin.GetSummonDefinition();
        var existing = SummonActions.FindExistingSummon(Owner.Creature, definition);

        if (existing != null && existing.IsAlive)
        {
            await CreatureCmd.Kill(existing, true);
            await CardPileCmd.Draw(choiceContext, DynamicVars["BonusCards"].IntValue, Owner);
            await PlayerCmd.GainEnergy(DynamicVars.Energy.BaseValue, Owner);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Energy.UpgradeValueBy(1);
    }
}
