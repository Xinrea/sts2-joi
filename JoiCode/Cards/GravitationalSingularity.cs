using BaseLib.Utils;
using Joi.JoiCode.Character;
using Joi.JoiCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Joi.JoiCode.Cards;

[Pool(typeof(JoiCardPool))]
public class GravitationalSingularity : JoiCard
{
    public GravitationalSingularity() : base(1, CardType.Skill, CardRarity.Common, TargetType.Self) { }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("WhiteHole", 2)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var blackHole = Owner.Creature.GetPower<BlackHolePower>();
        if (blackHole != null && blackHole.Amount > 0)
        {
            await CreatureCmd.Damage(choiceContext, [Owner.Creature], blackHole.Amount, ValueProp.Unpowered, Owner.Creature, null);
            await PowerCmd.Remove(blackHole);
        }

        await CommonActions.ApplySelf<WhiteHolePower>(this, DynamicVars["WhiteHole"].BaseValue);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["WhiteHole"].UpgradeValueBy(1);
    }
}
