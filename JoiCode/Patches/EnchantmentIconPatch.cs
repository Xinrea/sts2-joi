using HarmonyLib;
using MegaCrit.Sts2.Core.Models;

namespace Joi.JoiCode.Patches;

[HarmonyPatch(typeof(EnchantmentModel), nameof(EnchantmentModel.IconPath), MethodType.Getter)]
public static class EnchantmentIconPatch
{
    [HarmonyPostfix]
    public static void Postfix(EnchantmentModel __instance, ref string __result)
    {
        // 如果是 FeedEnchantment 且图标路径是 missing_enchantment，替换为我们的图标
        if (__instance.GetType().Name == "FeedEnchantment" &&
            __result.Contains("missing_enchantment"))
        {
            __result = "res://Joi/images/enchantments/feed_enchantment.png";
            MainFile.Logger.Info($"[EnchantmentIconPatch] Redirected FeedEnchantment icon to {__result}");
        }
    }
}
