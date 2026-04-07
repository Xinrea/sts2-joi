using BaseLib.Abstracts;
using BaseLib.Utils;
using BaseLib.Utils.NodeFactories;
using Godot;
using Joi.JoiCode.Services;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.ValueProps;

namespace Joi.JoiCode.Minions;

public class ZhouXin : CustomMonsterModel
{
    private const string DefaultUniqueKey = "zhou-xin-core";

    private static string? _customName;

    public override string? DefaultMoveState => "idle";

    public override int MinInitialHp => 1;
    public override int MaxInitialHp => 1;

    public override bool ShouldReceiveCombatHooks => false;

    public override LocString Title
    {
        get
        {
            var title = L10NMonsterLookup(Id.Entry + ".name");
            var name = _customName ?? L10NMonsterLookup(Id.Entry + ".default_name").GetFormattedText();
            title.Add("guard-name", name);
            return title;
        }
    }

    public static void RandomizeName()
    {
        _customName = BiliGuardService.GetRandomGuardName();
    }

    public static SummonDefinition GetSummonDefinition(string uniqueKey = DefaultUniqueKey)
    {
        return SummonDefinition.For<ZhouXin>(
            uniqueKey: uniqueKey,
            visuals: new CreatureVisualSpec
            {
                ImagePath = "res://Joi/images/creatures/zhou_xin.png",
                BoundsOffset = new Vector2(-250, 0)
            }
        );
    }

    public override NCreatureVisuals CreateCustomVisuals()
    {
        var visuals = NodeFactory<NCreatureVisuals>.CreateFromScene("res://scenes/creature_visuals/zhou_xin.tscn");
        var sprite = visuals.GetNode<Sprite2D>("%Visuals");
        sprite.Texture = GD.Load<Texture2D>("res://Joi/images/creatures/zhou_xin.png");
        return visuals;
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
            && dealer.Side != target.Side)
        {
            return Creature;
        }

        return target;
    }
}
