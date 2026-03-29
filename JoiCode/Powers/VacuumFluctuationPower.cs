using BaseLib.Hooks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace Joi.JoiCode.Powers;

public class VacuumFluctuationPower : JoiPower, IOnPowerAmountChanged
{
    private int remainingTriggersThisTurn;

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public async Task OnPowerAmountChanged(PowerChangeContext context)
    {
        if (context.Target == Owner && context.Delta > 0 && context.Power is VacuumFluctuationPower)
        {
            remainingTriggersThisTurn += (int)context.Delta;
            return;
        }

        if (context.Target != Owner
            || context.Delta <= 0
            || remainingTriggersThisTurn <= 0
            || context.Power is not BlackHolePower and not WhiteHolePower
            || Owner.Player == null)
        {
            return;
        }

        await PlayerCmd.GainEnergy(1, Owner.Player);
        remainingTriggersThisTurn--;
    }

    public override Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player == Owner.Player)
        {
            remainingTriggersThisTurn = (int)Amount;
        }

        return Task.CompletedTask;
    }
}
