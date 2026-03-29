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
public class CosmicRip : JoiCard
{
    public CosmicRip() : base(3, CardType.Attack, CardRarity.Rare, TargetType.AllEnemies) { }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(30, ValueProp.Move),
        new DynamicVar("BlackHole", 10)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.Damage(choiceContext, CombatState?.Enemies.ToList() ?? [], DynamicVars.Damage.BaseValue, ValueProp.Move, Owner.Creature, this);
        await CommonActions.ApplySelf<BlackHolePower>(this, DynamicVars["BlackHole"].BaseValue);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(10);
    }
}
