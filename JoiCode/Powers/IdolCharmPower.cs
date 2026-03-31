using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;

namespace Joi.JoiCode.Powers;

public class IdolCharmPower : JoiPower
{
    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.None;

    public override async Task AfterTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
    {
        if (side != Owner.Side)
        {
            return;
        }

        MainFile.Logger.Info($"[IdolCharm] Expiring on {Owner.Name} at end of {side} turn");
        await PowerCmd.Remove(this);
    }

    public override Creature ModifyUnblockedDamageTarget(Creature target, decimal amount, ValueProp props, Creature? dealer)
    {
        if (Owner.IsDead
            || Owner.CurrentHp <= 0
            || target == Owner
            || dealer == null
            || target.Side != Owner.Side
            || dealer.Side == Owner.Side)
        {
            return target;
        }

        return Owner;
    }
}
