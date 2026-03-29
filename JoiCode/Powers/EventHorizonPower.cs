using BaseLib.Hooks;
using MegaCrit.Sts2.Core.Entities.Powers;

namespace Joi.JoiCode.Powers;

public class EventHorizonPower : JoiPower, IOnPowerAmountChanged
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public Task OnPowerAmountChanged(PowerChangeContext context)
    {
        if (context.Target != Owner || context.Delta <= 0 || context.Power is not BlackHolePower)
        {
            return Task.CompletedTask;
        }

        Owner.GainBlockInternal((int)context.Delta * Amount);
        return Task.CompletedTask;
    }
}
