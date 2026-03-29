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
public class DarkMatterImpact : JoiCard
{
    public DarkMatterImpact() : base(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy) { }

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(8, ValueProp.Move)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var blackHole = Owner.Creature.GetPower<BlackHolePower>();

        if (blackHole != null && blackHole.Amount >= 5)
        {
            var totalDamage = DynamicVars.Damage.BaseValue + blackHole.Amount;
            await CreatureCmd.Damage(choiceContext, [cardPlay.Target], totalDamage, ValueProp.Move, Owner.Creature, this);
        }
        else
        {
            await CommonActions.CardAttack(this, cardPlay).Execute(choiceContext);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3);
    }
}
