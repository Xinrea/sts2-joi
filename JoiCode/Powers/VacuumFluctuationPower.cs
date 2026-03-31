using BaseLib.Hooks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace Joi.JoiCode.Powers;

public class VacuumFluctuationPower : JoiPower, IOnPowerAmountChanged
{
    private bool hasTriggeredThisTurn;

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.None;

    public async Task OnPowerAmountChanged(PowerChangeContext context)
    {
        if (context.Target != Owner
            || context.Delta <= 0
            || hasTriggeredThisTurn
            || context.Power is not BlackHolePower and not WhiteHolePower
            || Owner.Player == null)
        {
            return;
        }

        await PlayerCmd.GainEnergy(1, Owner.Player);
        hasTriggeredThisTurn = true;
    }

    public override Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player == Owner.Player)
        {
            hasTriggeredThisTurn = false;
        }

        return Task.CompletedTask;
    }
}
