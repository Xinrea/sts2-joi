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
public class GravityTether : JoiCard
{
    public GravityTether() : base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy) { }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(8, ValueProp.Move),
        new DynamicVar("BonusDamage", 5)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var enemies = CombatState?.Enemies.ToList() ?? [];
        bool hasCharmedEnemy = enemies.Any(e => e.GetPower<IdolCharmPower>() != null);

        if (hasCharmedEnemy)
        {
            var totalDamage = DynamicVars.Damage.BaseValue + DynamicVars["BonusDamage"].BaseValue;
            await CreatureCmd.Damage(choiceContext, cardPlay.Target != null ? [cardPlay.Target] : [], totalDamage, ValueProp.Move, Owner.Creature, this);
        }
        else
        {
            await CommonActions.CardAttack(this, cardPlay).Execute(choiceContext);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(2);
        DynamicVars["BonusDamage"].UpgradeValueBy(2);
    }
}
