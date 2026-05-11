using BaseLib.Utils;
using Joi.JoiCode.Character;
using Joi.JoiCode.Powers;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace Joi.JoiCode.Cards;

[Pool(typeof(JoiCardPool))]
public class PrimordialCovenant : JoiCard
{
    public PrimordialCovenant() : base(2, CardType.Skill, CardRarity.Ancient, TargetType.Self) { }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new BlockVar(10, ValueProp.Move),
        new DynamicVar("BlackHole", 10),
        new DynamicVar("WhiteHole", 10)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CommonActions.CardBlock(this, cardPlay);
        await CommonActions.ApplySelf<BlackHolePower>(this, DynamicVars["BlackHole"].BaseValue);
        await CommonActions.ApplySelf<WhiteHolePower>(this, DynamicVars["WhiteHole"].BaseValue);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(2);
        DynamicVars["BlackHole"].UpgradeValueBy(2);
        DynamicVars["WhiteHole"].UpgradeValueBy(2);
    }
}
