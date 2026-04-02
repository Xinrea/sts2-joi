using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using BaseLib;
using System.Collections.Generic;
using System.Linq;

namespace Joi.JoiCode.Patches;

[HarmonyPatch(typeof(CharacterModel), "AssetPaths", MethodType.Getter)]
public static class AssetPathsDebugPatch
{
    [HarmonyPostfix]
    static void Postfix(CharacterModel __instance, ref IEnumerable<string> __result)
    {
        if (__instance.Id.Entry == "JOI")
        {
            var paths = __result.ToList();
            MainFile.Logger.Info($"[JOI DEBUG] AssetPaths called for Joi character");
            MainFile.Logger.Info($"[JOI DEBUG] Total asset paths: {paths.Count}");

            foreach (var path in paths)
            {
                MainFile.Logger.Info($"[JOI DEBUG] Asset path: {path}");
            }
        }
    }
}
