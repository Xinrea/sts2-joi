using BaseLib.Abstracts;
using BaseLib.Utils.NodeFactories;
using Godot;
using Joi.JoiCode.Powers;
using Joi.JoiCode.Services;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.ValueProps;

namespace Joi.JoiCode.Minions;

public class ZhouXin : CustomMonsterModel
{
    public const string DefaultUniqueKey = "zhou-xin-core";

    private static string? _customName;

    public override int MinInitialHp => 5;
    public override int MaxInitialHp => 5;

    public override bool ShouldReceiveCombatHooks => true;

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

    /// <summary>
    /// Find an existing ZhouXin pet for the player.
    /// Checks both Allies and Enemies (creature may be on either side).
    /// </summary>
    public static ZhouXin? FindExisting(ICombatState? combatState, Player player)
    {
        if (combatState == null) return null;
        return combatState.Allies.Concat(combatState.Enemies)
            .Where(c => !c.IsDead && c.PetOwner == player)
            .Select(c => c.Monster)
            .OfType<ZhouXin>()
            .FirstOrDefault();
    }

    /// <summary>
    /// Summon ZhouXin into combat as a real player pet.
    /// </summary>
    public static async Task<Creature> SummonAsPet(Player player)
    {
        RandomizeName();
        var combatState = player.Creature.CombatState;
        if (combatState == null)
            throw new InvalidOperationException("Player has no combat state");
        if (player.PlayerCombatState == null)
            throw new InvalidOperationException("Player has no player combat state");

        var creature = await CreatureCmd.Add(ModelDb.Monster<ZhouXin>().ToMutable(), combatState, player.Creature.Side);
        player.PlayerCombatState.AddPetInternal(creature);

        // Apply StoragePower to player if not already present
        if (player.Creature.GetPower<StoragePower>() == null)
        {
            await PowerCmd.Apply<StoragePower>(new BlockingPlayerChoiceContext(), [player.Creature], 1, player.Creature, null, true);
        }

        // Position next to the player
        var room = NCombatRoom.Instance;
        if (room != null)
        {
            var petNode = room.GetCreatureNode(creature);
            var ownerNode = room.GetCreatureNode(player.Creature);
            if (petNode != null && ownerNode != null)
            {
                petNode.Position = new Vector2(
                    ownerNode.Position.X + ownerNode.Visuals.Bounds.Size.X * 0.5f + 150f,
                    ownerNode.Position.Y
                );
            }
        }

        return creature;
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
        MoveState idle = new("idle", _ => Task.CompletedTask, Array.Empty<AbstractIntent>())
        {
            FollowUpStateId = "idle"
        };
        idle.FollowUpState = idle;
        return new MonsterMoveStateMachine([idle], idle);
    }

    public override Creature ModifyUnblockedDamageTarget(Creature target, decimal amount, ValueProp props, Creature? dealer)
    {
        // Bodyguard: redirect damage targeting the pet owner to ZhouXin
        if (target == Creature.PetOwner?.Creature
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
