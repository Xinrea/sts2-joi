using BaseLib.Utils;
using Joi.JoiCode.Character;
using Joi.JoiCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace Joi.JoiCode.Cards;

[Pool(typeof(JoiCardPool))]
public class WhiteHoleReflux : JoiCard
{
    public WhiteHoleReflux() : base(1, CardType.Skill, CardRarity.Rare, TargetType.Self) { }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("WhiteHole", 1)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var whiteHole = Owner.Creature.GetPower<WhiteHolePower>();
        var removedStacks = whiteHole?.Amount ?? 0;

        if (whiteHole != null)
        {
            await PowerCmd.Remove(whiteHole);
        }

        if (removedStacks > 0)
        {
            Owner.Creature.HealInternal(removedStacks);
        }

        await CommonActions.ApplySelf<WhiteHolePower>(this, DynamicVars["WhiteHole"].BaseValue);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["WhiteHole"].UpgradeValueBy(1);
    }
}
