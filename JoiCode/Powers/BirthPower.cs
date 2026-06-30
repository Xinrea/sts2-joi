using Joi.JoiCode.Minions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Powers;

namespace Joi.JoiCode.Powers;

public class BirthPower : JoiPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    // TODO: Re-implement IOnPowerRemoved hook via BaseLib v3.3.2 HookType subscription system
    // to summon ZhouXin when BlackHolePower is removed with amount >= 5.
    // Original logic:
    //   On BlackHole removed: summon ZhouXin with HP = removed_amount * BirthPower.Amount
}
