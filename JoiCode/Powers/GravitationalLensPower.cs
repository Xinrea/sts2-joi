using BaseLib.Hooks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;

namespace Joi.JoiCode.Powers;

public class GravitationalLensPower : JoiPower, IOnPowerAmountChanged
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public async Task OnPowerAmountChanged(PowerChangeContext context)
    {
        if (context.Target != Owner || context.Delta == 0 || context.Power is not WhiteHolePower)
        {
            return;
        }

        var enemies = Owner.CombatState?.Enemies.ToList();
        if (enemies == null || enemies.Count == 0)
        {
            return;
        }

        var randomTarget = enemies[Random.Shared.Next(enemies.Count)];
        await CreatureCmd.Damage(new BlockingPlayerChoiceContext(), [randomTarget], Amount, ValueProp.Unpowered, Owner, context.SourceCard);
    }
}
