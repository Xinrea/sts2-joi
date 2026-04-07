using HarmonyLib;
using Joi.JoiCode.Relics;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;

namespace Joi.JoiCode.Patches;

[HarmonyPatch(typeof(TouchOfOrobas), nameof(TouchOfOrobas.GetUpgradedStarterRelic))]
public static class TouchOfOrobasPatch
{
    [HarmonyPostfix]
    static void Postfix(RelicModel starterRelic, ref RelicModel __result)
    {
        if (starterRelic is CosmicOriginRelic)
        {
            __result = ModelDb.Relic<CosmicApexRelic>();
        }
    }
}
