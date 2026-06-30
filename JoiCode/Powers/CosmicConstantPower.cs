using Joi.JoiCode.Powers;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace Joi.JoiCode.Powers;

public class CosmicConstantPower : JoiPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.None;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("BlackHole", 2),
        new DynamicVar("WhiteHole", 2)
    ];

    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (side != CombatSide.Player || Owner == null)
            return;

        var bh = (int)(DynamicVars?["BlackHole"]?.BaseValue ?? 1);
        var wh = (int)(DynamicVars?["WhiteHole"]?.BaseValue ?? 1);

        await PowerCmd.Apply<BlackHolePower>(choiceContext, [Owner], bh, Owner, null, true);
        await PowerCmd.Apply<WhiteHolePower>(choiceContext, [Owner], wh, Owner, null, true);
    }
}
