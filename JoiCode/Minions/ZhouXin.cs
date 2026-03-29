using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.ValueProps;

namespace Joi.JoiCode.Minions;

public class ZhouXin : MonsterModel
{
    public override int MinInitialHp => 1;
    public override int MaxInitialHp => 1;

    public override bool ShouldReceiveCombatHooks => false;

    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        return null!;
    }

    public override Creature ModifyUnblockedDamageTarget(Creature target, decimal amount, ValueProp props, Creature? dealer)
    {
        MainFile.Logger.Info($"[ZhouXin] target={target.Name}, amount={amount}, props={props}, dealer={dealer?.Name}, creatureHp={Creature.CurrentHp}");

        if (target.Side == Creature.Side && !Creature.IsDead && dealer != null && dealer.Side != Creature.Side)
        {
            // Only intercept if creature can survive the damage
            if (Creature.CurrentHp > 0)
            {
                MainFile.Logger.Info($"[ZhouXin] Intercepting damage to {target.Name}, redirecting to ZhouXin");
                return Creature;
            }
        }
        return target;
    }
}
