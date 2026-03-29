using BaseLib.Abstracts;
using BaseLib.Utils.NodeFactories;
using Godot;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.ValueProps;

namespace Joi.JoiCode.Minions;

public class ZhouXin : CustomMonsterModel
{
    public override string? DefaultMoveState => "idle";

    public override int MinInitialHp => 1;
    public override int MaxInitialHp => 1;

    public override bool ShouldReceiveCombatHooks => false;

    public override NCreatureVisuals CreateCustomVisuals()
    {
        Texture2D texture = GD.Load<Texture2D>("res://Joi/images/creatures/zhou_xin.png");
        return NodeFactory<NCreatureVisuals>.CreateFromResource(texture);
    }

    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        MoveState idle = new("idle", _ => Task.CompletedTask, Array.Empty<AbstractIntent>());
        return new MonsterMoveStateMachine([idle], idle);
    }

    public override Creature ModifyUnblockedDamageTarget(Creature target, decimal amount, ValueProp props, Creature? dealer)
    {
        if (target.Side == Creature.Side
            && !Creature.IsDead
            && Creature.CurrentHp > 0
            && dealer != null
            && dealer.Side != Creature.Side)
        {
            return Creature;
        }

        return target;
    }
}
