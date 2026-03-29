using BaseLib.Hooks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;

namespace Joi.JoiCode.Powers;

public class GravityFieldPower : JoiPower, IOnPowerAmountChanged
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public async Task OnPowerAmountChanged(PowerChangeContext context)
    {
        if (context.Target != Owner || context.Delta <= 0 || context.Power is not WhiteHolePower)
        {
            return;
        }

        var enemies = Owner.CombatState?.Enemies.ToList();
        if (enemies == null || enemies.Count == 0)
        {
            return;
        }

        await CreatureCmd.Damage(new BlockingPlayerChoiceContext(), enemies, Amount * 3, ValueProp.Unpowered, Owner, context.SourceCard);
    }
}
