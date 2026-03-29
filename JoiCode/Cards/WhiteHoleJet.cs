using BaseLib.Utils;
using Joi.JoiCode.Character;
using Joi.JoiCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace Joi.JoiCode.Cards;

[Pool(typeof(JoiCardPool))]
public class WhiteHoleJet : JoiCard
{
    public WhiteHoleJet() : base(0, CardType.Skill, CardRarity.Common, TargetType.Self) { }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("WhiteHole", 2),
        new CardsVar(1),
        new EnergyVar(1)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var whiteHole = Owner.Creature.GetPower<WhiteHolePower>();
        var stacksToRemove = DynamicVars["WhiteHole"].IntValue;
        if (whiteHole == null || whiteHole.Amount < stacksToRemove)
        {
            return;
        }

        var newAmount = (int)whiteHole.Amount - stacksToRemove;
        if (newAmount == 0)
        {
            await PowerCmd.Remove(whiteHole);
        }
        else
        {
            whiteHole.SetAmount(newAmount, false);
        }

        await CommonActions.Draw(this, choiceContext);
        await PlayerCmd.GainEnergy(DynamicVars.Energy.BaseValue, Owner);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Cards.UpgradeValueBy(1);
    }
}
