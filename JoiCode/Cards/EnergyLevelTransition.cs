using BaseLib.Utils;
using Joi.JoiCode.Character;
using Joi.JoiCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace Joi.JoiCode.Cards;

[Pool(typeof(JoiCardPool))]
public class EnergyLevelTransition : JoiCard
{
    public EnergyLevelTransition() : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self) { }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("Energy", 1),
        new DynamicVar("BonusEnergy", 2)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var handPile = CardPile.Get(PileType.Hand, Owner);
        bool hasHoleCard = handPile?.Cards.Any(c =>
            c.Id.Entry.Contains("BlackHole", System.StringComparison.OrdinalIgnoreCase) ||
            c.Id.Entry.Contains("WhiteHole", System.StringComparison.OrdinalIgnoreCase) ||
            c.Id.Entry.Contains("Singularity", System.StringComparison.OrdinalIgnoreCase) ||
            c.Id.Entry.Contains("Photon", System.StringComparison.OrdinalIgnoreCase) ||
            c.Id.Entry.Contains("Gravitational", System.StringComparison.OrdinalIgnoreCase) ||
            c.Id.Entry.Contains("Pulsar", System.StringComparison.OrdinalIgnoreCase) ||
            c.Id.Entry.Contains("Resonance", System.StringComparison.OrdinalIgnoreCase) ||
            c.Id.Entry.Contains("Starlight", System.StringComparison.OrdinalIgnoreCase) ||
            c.Id.Entry.Contains("Axis", System.StringComparison.OrdinalIgnoreCase)
        ) ?? false;

        var energy = hasHoleCard ? (int)DynamicVars["BonusEnergy"].BaseValue : (int)DynamicVars["Energy"].BaseValue;
        await PlayerCmd.GainEnergy(energy, Owner);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Energy"].UpgradeValueBy(2);
        DynamicVars["BonusEnergy"].UpgradeValueBy(1);
    }
}
