using BaseLib.Utils.NodeFactories;
using Godot;
using HarmonyLib;
using Joi.JoiCode.Minions;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace Joi.JoiCode.Patches;

[HarmonyPatch(typeof(MonsterModel), nameof(MonsterModel.CreateVisuals))]
public class ZhouXinVisualsPatch
{
    public static bool Prefix(MonsterModel __instance, ref NCreatureVisuals __result)
    {
        if (__instance is ZhouXin)
        {
            var texture = GD.Load<Texture2D>("res://Joi/images/creatures/zhou_xin.png");
            __result = NodeFactory<NCreatureVisuals>.CreateFromResource(texture);
            return false;
        }
        return true;
    }
}
