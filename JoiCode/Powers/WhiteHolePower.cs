using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;

namespace Joi.JoiCode.Powers;

public class WhiteHolePower : JoiPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
    {
        if (side == CombatSide.Player && Amount >= 10)
        {
            await CreatureCmd.Damage(choiceContext, Owner.CombatState.Enemies.ToList(), Amount, ValueProp.Unpowered, Owner, null);
            await PowerCmd.Remove(this);
        }
    }
}
