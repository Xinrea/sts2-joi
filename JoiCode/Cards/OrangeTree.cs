using BaseLib.Utils;
using Joi.JoiCode.Character;
using Joi.JoiCode.Powers;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace Joi.JoiCode.Cards;

[Pool(typeof(JoiCardPool))]
public class OrangeTree : JoiCard
{
    public OrangeTree() : base(2, CardType.Power, CardRarity.Common, TargetType.Self) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CommonActions.ApplySelf<OrangeTreePower>(this, 1);
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}

