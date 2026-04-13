using BaseLib.Utils;
using Joi.JoiCode.Character;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace Joi.JoiCode.Cards;

[Pool(typeof(JoiCardPool))]
public class OrangePeel : JoiCard
{
    public OrangePeel() : base(1, CardType.Skill, CardRarity.Common, TargetType.Self) { }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("Multiplier", 3)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromCard<Orange>(false)];

    protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var exhaustPile = CardPile.Get(PileType.Exhaust, Owner);
        var handPile = CardPile.Get(PileType.Hand, Owner);
        var orangeCount = (exhaustPile?.Cards.Count(c => c is Orange) ?? 0)
                        + (handPile?.Cards.Count(c => c is Orange) ?? 0);

        var multiplier = (int)DynamicVars["Multiplier"].BaseValue;
        var block = orangeCount * multiplier;

        if (block > 0)
        {
            Owner.Creature.GainBlockInternal(block);
        }

        return Task.CompletedTask;
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Multiplier"].UpgradeValueBy(1);
    }
}
