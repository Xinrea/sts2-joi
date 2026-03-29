using BaseLib.Utils;
using Joi.JoiCode.Character;
using Joi.JoiCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace Joi.JoiCode.Cards;

[Pool(typeof(JoiCardPool))]
public class PureWhiteForm : JoiCard
{
    public PureWhiteForm() : base(2, CardType.Skill, CardRarity.Uncommon, TargetType.Self) { }

    protected override IEnumerable<DynamicVar> CanonicalVars => [];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var blackHole = Owner.Creature.GetPower<BlackHolePower>();
        var convertedStacks = blackHole?.Amount ?? 0;
        var whiteHoleToApply = convertedStacks + (IsUpgraded ? 3 : 0);

        if (blackHole != null)
        {
            await PowerCmd.Remove(blackHole);
        }

        if (whiteHoleToApply > 0)
        {
            await CommonActions.ApplySelf<WhiteHolePower>(this, whiteHoleToApply);
        }
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
