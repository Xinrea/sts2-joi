using BaseLib.Utils;
using Joi.JoiCode.Character;
using Joi.JoiCode.Powers;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace Joi.JoiCode.Cards;

[Pool(typeof(JoiCardPool))]
public class Bond : JoiCard
{
    public Bond() : base(2, CardType.Power, CardRarity.Uncommon, TargetType.Self) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CommonActions.ApplySelf<BondPower>(this, 1);
    }

    protected override void OnUpgrade()
    {
        // reduce cost by 1
        EnergyCost.UpgradeBy(-1);
    }
}
