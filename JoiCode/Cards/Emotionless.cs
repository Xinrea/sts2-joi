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
public class Emotionless : JoiCard
{
    public Emotionless() : base(1, CardType.Skill, CardRarity.Common, TargetType.AllEnemies) { }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("BlackHole", 5)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var blackHolePower = Owner.Creature.GetPower<BlackHolePower>();
        var blackHoleStacks = blackHolePower?.Amount ?? 0;
        var damage = blackHoleStacks + (IsUpgraded ? 5 : 0);

        await CreatureCmd.Damage(choiceContext, CombatState?.Enemies.ToList() ?? [], damage, ValueProp.Move, Owner.Creature, this);
        await CommonActions.ApplySelf<BlackHolePower>(this, DynamicVars["BlackHole"].IntValue);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["BlackHole"].UpgradeValueBy(2);
    }
}
