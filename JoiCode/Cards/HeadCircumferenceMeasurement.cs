using BaseLib.Utils;
using Joi.JoiCode.Character;
using Joi.JoiCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace Joi.JoiCode.Cards;

[Pool(typeof(JoiCardPool))]
public class HeadCircumferenceMeasurement : JoiCard
{
    public HeadCircumferenceMeasurement() : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self) { }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("TargetHandSize", 6)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var targetHandSize = (int)DynamicVars["TargetHandSize"].BaseValue;
        var handPile = CardPile.Get(PileType.Hand, Owner);
        var currentHandSize = handPile?.Cards.Count ?? 0;
        var drawCount = Math.Max(0, targetHandSize - currentHandSize);

        if (drawCount > 0)
        {
            await CardPileCmd.Draw(choiceContext, drawCount, Owner);
            await CommonActions.ApplySelf<BlackHolePower>(this, drawCount);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["TargetHandSize"].UpgradeValueBy(2);
    }
}
