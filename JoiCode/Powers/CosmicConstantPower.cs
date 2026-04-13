using BaseLib.Hooks;
using Joi.JoiCode.Character;
using Joi.JoiCode.Powers;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Powers;

namespace Joi.JoiCode.Powers;

public class CosmicConstantPower : JoiPower, IAfterTurnStart
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.None;

    public async Task AfterTurnStart(TurnContext context)
    {
        if (context.Side != CombatSide.Player || Owner == null)
            return;

        var bh = (int)(DynamicVars?["BlackHole"]?.BaseValue ?? 1);
        var wh = (int)(DynamicVars?["WhiteHole"]?.BaseValue ?? 1);

        await PowerCmd.Apply<BlackHolePower>(Owner, bh, Owner, null, true);
        await PowerCmd.Apply<WhiteHolePower>(Owner, wh, Owner, null, true);
    }
}
