using BaseLib.Hooks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Powers;

namespace Joi.JoiCode.Powers;

public class OmniscientPower : JoiPower, IOnPowerAmountChanged
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public Task OnPowerAmountChanged(PowerChangeContext context)
    {
        if (context.Target != Owner || context.Delta <= 0 || context.Power is not BlackHolePower)
        {
            return Task.CompletedTask;
        }

        return PowerCmd.Apply<WhiteHolePower>(Owner, Amount, Owner, null, true);
    }
}
