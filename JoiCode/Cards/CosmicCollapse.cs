using BaseLib.Utils;
using Joi.JoiCode.Character;
using Joi.JoiCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace Joi.JoiCode.Cards;

[Pool(typeof(JoiCardPool))]
public class CosmicCollapse : JoiCard
{
    public CosmicCollapse() : base(2, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy) { }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("Bonus", 5)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var blackHole = Owner.Creature.GetPower<BlackHolePower>();
        var stacks = (int)(blackHole?.Amount ?? 0);
        var bonus = (int)DynamicVars["Bonus"].BaseValue;

        if (stacks + bonus > 0 && cardPlay.Target != null)
        {
            await CreatureCmd.Damage(choiceContext, [cardPlay.Target], stacks + bonus, ValueProp.Move, Owner.Creature, this);
        }

        if (blackHole != null && stacks >= 10)
        {
            await PowerCmd.Remove(blackHole);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Bonus"].UpgradeValueBy(5);
    }
}
