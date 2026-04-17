using BaseLib.Utils;
using Joi.JoiCode.Character;
using Joi.JoiCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace Joi.JoiCode.Cards;

[Pool(typeof(JoiCardPool))]
public class Compress : JoiCard
{
    public Compress() : base(1, CardType.Skill, CardRarity.Common, TargetType.Self) { }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("BlackHole", 3),
        new CardsVar(1)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var power = Owner.Creature.GetPower<BlackHolePower>();
        if (power != null)
        {
            var newAmount = Math.Max(0, power.Amount - DynamicVars["BlackHole"].IntValue);
            if (newAmount == 0)
                await PowerCmd.Remove(power);
            else
                power.SetAmount(newAmount, true);
        }

        await CommonActions.Draw(this, choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["BlackHole"].UpgradeValueBy(3);
    }
}
