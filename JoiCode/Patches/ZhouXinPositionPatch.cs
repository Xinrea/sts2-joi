using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils;
using HarmonyLib;
using Joi.JoiCode.Minions;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace Joi.JoiCode.Patches;

[HarmonyPatch(typeof(SummonActions), nameof(SummonActions.RegisterPetForPlayer))]
public static class ZhouXinPositionPatch
{
    [HarmonyPostfix]
    static void Postfix(Creature creature, Player player)
    {
        if (creature.Monster is not ZhouXin) return;

        var room = NCombatRoom.Instance;
        if (room == null) return;

        var petNode = room.GetCreatureNode(creature);
        var ownerNode = room.GetCreatureNode(player.Creature);

        MainFile.Logger.Info($"[ZhouXinPositionPatch] RegisterPetForPlayer postfix: petNode={petNode != null}, ownerNode={ownerNode != null}");

        if (petNode == null || ownerNode == null) return;

        var newPos = new Godot.Vector2(
            ownerNode.Position.X + ownerNode.Visuals.Bounds.Size.X * 0.5f + 150f,
            ownerNode.Position.Y
        );
        MainFile.Logger.Info($"[ZhouXinPositionPatch] Moving ZhouXin to {newPos}, owner.Position={ownerNode.Position}");
        petNode.Position = newPos;
    }
}
