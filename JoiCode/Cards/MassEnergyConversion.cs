using BaseLib.Utils;
using Joi.JoiCode.Character;
using Joi.JoiCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace Joi.JoiCode.Cards;

[Pool(typeof(JoiCardPool))]
public class MassEnergyConversion : JoiCard
{
    public MassEnergyConversion() : base(1, CardType.Power, CardRarity.Uncommon, TargetType.Self) { }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("BlackHole", 3),
        new DynamicVar("Multiplier", 2)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var blackHole = Owner.Creature.GetPower<BlackHolePower>();
        var stacks = (int)(blackHole?.Amount ?? 0);
        var maxConvert = (int)DynamicVars["BlackHole"].BaseValue;
        var convert = Math.Min(stacks, maxConvert);
        var multiplier = (int)DynamicVars["Multiplier"].BaseValue;

        if (convert > 0 && blackHole != null)
        {
            var newAmount = blackHole.Amount - convert;
            if (newAmount == 0)
                await PowerCmd.Remove(blackHole);
            else
                blackHole.SetAmount(newAmount, false);

            Owner.Creature.GainBlockInternal(convert * multiplier);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Multiplier"].UpgradeValueBy(1);
    }
}
