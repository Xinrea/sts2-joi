using HarmonyLib;
using Joi.JoiCode.Enchantments;
using MegaCrit.Sts2.Core.Models;

namespace Joi.JoiCode.Patches;

[HarmonyPatch(typeof(EnchantmentModel), nameof(EnchantmentModel.IconPath), MethodType.Getter)]
public static class EnchantmentIconPatch
{
    private const string FeedIconPath = "res://Joi/images/enchantments/feed_enchantment.png";

    [HarmonyPostfix]
    public static void Postfix(EnchantmentModel __instance, ref string __result)
    {
        if (__instance is FeedEnchantment)
        {
            __result = FeedIconPath;
        }
    }
}
