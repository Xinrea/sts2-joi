using BaseLib.Utils;
using Joi.JoiCode.Character;
using Joi.JoiCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace Joi.JoiCode.Cards;

[Pool(typeof(JoiCardPool))]
public class TwoPoleReversal : JoiCard
{
    public TwoPoleReversal() : base(2, CardType.Skill, CardRarity.Uncommon, TargetType.Self) { }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("BlackHole", 2),
        new DynamicVar("WhiteHole", 2)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CommonActions.ApplySelf<BlackHolePower>(this, DynamicVars["BlackHole"].BaseValue);
        await CommonActions.ApplySelf<WhiteHolePower>(this, DynamicVars["WhiteHole"].BaseValue);

        var blackHole = Owner.Creature.GetPower<BlackHolePower>();
        var whiteHole = Owner.Creature.GetPower<WhiteHolePower>();

        if (blackHole != null && whiteHole != null)
        {
            var temp = blackHole.Amount;
            blackHole.Amount = whiteHole.Amount;
            whiteHole.Amount = temp;
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["BlackHole"].UpgradeValueBy(1);
        DynamicVars["WhiteHole"].UpgradeValueBy(1);
        RemoveKeyword(CardKeyword.Exhaust);
    }
}
