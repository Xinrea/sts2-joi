using BaseLib.Utils;
using Joi.JoiCode.Character;
using Joi.JoiCode.Powers;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace Joi.JoiCode.Cards;

[Pool(typeof(JoiCardPool))]
public class HeavyHammer : JoiCard
{
    public HeavyHammer() : base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy) { }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(6, ValueProp.Move),
        new PowerVar<WeakPower>(1),
        new PowerVar<VulnerablePower>(1),
        new DynamicVar("BlackHole", 2)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);

        await CommonActions.CardAttack(this, cardPlay).Execute(choiceContext);
        await CommonActions.Apply<WeakPower>(cardPlay.Target, this, DynamicVars.Weak.BaseValue);
        await CommonActions.Apply<VulnerablePower>(cardPlay.Target, this, DynamicVars.Vulnerable.BaseValue);
        await CommonActions.ApplySelf<Joi.JoiCode.Powers.BlackHolePower>(this, DynamicVars["BlackHole"].BaseValue);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3);
        DynamicVars.Weak.UpgradeValueBy(1);
        DynamicVars.Vulnerable.UpgradeValueBy(1);
        DynamicVars["BlackHole"].UpgradeValueBy(2);
    }
}
