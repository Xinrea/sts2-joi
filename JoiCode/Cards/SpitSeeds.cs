using BaseLib.Utils;
using Joi.JoiCode.Character;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace Joi.JoiCode.Cards;

[Pool(typeof(JoiCardPool))]
public class SpitSeeds : JoiCard
{
    public SpitSeeds() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.AnyEnemy) { }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("Multiplier", 3)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromCard<Orange>(false)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var exhaustPile = CardPile.Get(PileType.Exhaust, Owner);
        var handPile = CardPile.Get(PileType.Hand, Owner);
        var orangeCount = (exhaustPile?.Cards.Count(c => c is Orange) ?? 0)
                        + (handPile?.Cards.Count(c => c is Orange) ?? 0);

        var multiplier = (int)DynamicVars["Multiplier"].BaseValue;
        var damage = orangeCount * multiplier;

        if (damage > 0 && cardPlay.Target != null)
        {
            await CreatureCmd.Damage(choiceContext, [cardPlay.Target], damage, ValueProp.Move, Owner.Creature, this);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Multiplier"].UpgradeValueBy(2);
    }
}
