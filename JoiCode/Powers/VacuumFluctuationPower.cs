using BaseLib.Hooks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace Joi.JoiCode.Powers;

public class VacuumFluctuationPower : JoiPower, IOnPowerAmountChanged
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public async Task OnPowerAmountChanged(PowerChangeContext context)
    {
        if (context.Target != Owner
            || context.Delta <= 0
            || Amount <= 0
            || context.Power is not BlackHolePower and not WhiteHolePower
            || Owner.Player == null)
        {
            return;
        }

        await PlayerCmd.GainEnergy(1, Owner.Player);

        if (Amount <= 1)
        {
            await PowerCmd.Remove(this);
        }
        else
        {
            SetAmount(Amount - 1, false);
        }
    }

    public override async Task AfterTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
    {
        if (side == CombatSide.Player)
        {
            await PowerCmd.Remove(this);
        }
    }
}
