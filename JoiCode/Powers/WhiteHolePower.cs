using Joi.JoiCode.Minions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;

namespace Joi.JoiCode.Powers;

public class WhiteHolePower : JoiPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (side == CombatSide.Player && Amount > 0)
        {
            await CreatureCmd.Damage(choiceContext, Owner.CombatState?.Enemies.ToList() ?? [], Amount, ValueProp.Unpowered, Owner);

            var combatState = Owner.CombatState;
            if (combatState != null)
            {
                var existing = ZhouXin.FindExisting(combatState, Owner.Player!);
                if (existing != null)
                {
                    existing.Creature.SetMaxHpInternal(existing.Creature.MaxHp + 1);
                    existing.Creature.HealInternal(1);
                }
                else if (Owner.Player != null)
                {
                    var creature = await ZhouXin.SummonAsPet(Owner.Player);
                    creature.SetMaxHpInternal(1);
                    creature.HealInternal(1);
                    VfxCmd.PlayOnCreature(creature, VfxCmd.healPath);
                }
            }

            await PowerCmd.Decrement(this);
        }
    }
}
