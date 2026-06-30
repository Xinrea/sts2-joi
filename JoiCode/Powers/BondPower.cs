using Joi.JoiCode.Minions;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace Joi.JoiCode.Powers;

public class BondPower : JoiPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.None;

    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (side != Owner.Side) return;

        var combatState = Owner.CombatState;
        if (combatState == null) return;

        var existing = ZhouXin.FindExisting(combatState, Owner.Player!);
        if (existing != null && existing.Creature.IsAlive && Owner.Player != null)
        {
            await PlayerCmd.GainEnergy(1, Owner.Player);
        }
    }
}
