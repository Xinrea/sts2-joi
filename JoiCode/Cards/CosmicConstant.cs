using BaseLib.Utils;
using Joi.JoiCode.Character;
using Joi.JoiCode.Powers;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace Joi.JoiCode.Cards;

[Pool(typeof(JoiCardPool))]
public class CosmicConstant : JoiCard
{
    public CosmicConstant() : base(2, CardType.Power, CardRarity.Rare, TargetType.Self) { }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("BlackHole", 2),
        new DynamicVar("WhiteHole", 2)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CommonActions.ApplySelf<CosmicConstantPower>(this, 1);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["BlackHole"].UpgradeValueBy(1);
        DynamicVars["WhiteHole"].UpgradeValueBy(1);
    }
}
