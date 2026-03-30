using BaseLib.Hooks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Joi.JoiCode.Powers;

public class StellarRequiemPower : JoiPower, IOnPowerRemoved
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public async Task OnPowerRemoved(PowerChangeContext context)
    {
        if (context.Power is not SleepPower || context.Target.Side == Owner.Side)
        {
            return;
        }

        // Apply White Hole to self
        await PowerCmd.Apply<WhiteHolePower>(Owner, Amount, Owner, null);

        // Apply Vulnerable to awakened enemy
        await PowerCmd.Apply<VulnerablePower>(context.Target, 1, Owner, null);
    }
}
