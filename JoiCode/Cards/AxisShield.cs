using BaseLib.Utils;
using Joi.JoiCode.Character;
using Joi.JoiCode.Minions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace Joi.JoiCode.Cards;

[Pool(typeof(JoiCardPool))]
public class AxisShield : JoiCard
{
    public AxisShield() : base(1, CardType.Skill, CardRarity.Common, TargetType.Self) { }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("Heal", 4),
        new BlockVar(4, ValueProp.Move)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var combatState = Owner.Creature.CombatState;
        if (combatState == null) return;

        var existing = ZhouXin.FindExisting(combatState, Owner);

        if (existing != null)
        {
            var newMaxHp = existing.Creature.MaxHp + (int)DynamicVars["Heal"].BaseValue;
            existing.Creature.SetMaxHpInternal(newMaxHp);
            existing.Creature.HealInternal((int)DynamicVars["Heal"].BaseValue);
        }
        else
        {
            var creature = await ZhouXin.SummonAsPet(Owner);
            creature.SetMaxHpInternal((int)DynamicVars["Heal"].BaseValue);
            creature.HealInternal((int)DynamicVars["Heal"].BaseValue);
            VfxCmd.PlayOnCreature(creature, VfxCmd.healPath);
        }

        Owner.Creature.GainBlockInternal((int)DynamicVars["Block"].BaseValue);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Heal"].UpgradeValueBy(2);
        DynamicVars.Block.UpgradeValueBy(2);
    }
}
