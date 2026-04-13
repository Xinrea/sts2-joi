using BaseLib.Utils;
using Joi.JoiCode.Character;
using Joi.JoiCode.Powers;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace Joi.JoiCode.Cards;

[Pool(typeof(JoiCardPool))]
public class GravitationalSingularity : JoiCard
{
    public GravitationalSingularity() : base(1, CardType.Skill, CardRarity.Common, TargetType.Self) { }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("BlackHole", 3),
        new DynamicVar("WhiteHole", 3)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CommonActions.ApplySelf<BlackHolePower>(this, DynamicVars["BlackHole"].BaseValue);
        await CommonActions.ApplySelf<WhiteHolePower>(this, DynamicVars["WhiteHole"].BaseValue);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["BlackHole"].UpgradeValueBy(2);
        DynamicVars["WhiteHole"].UpgradeValueBy(2);
    }
}
