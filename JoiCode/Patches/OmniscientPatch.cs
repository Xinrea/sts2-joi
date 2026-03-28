using HarmonyLib;
using Joi.JoiCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;

namespace Joi.JoiCode.Patches;

[HarmonyPatch(typeof(PowerCmd), nameof(PowerCmd.Apply), [typeof(PowerModel), typeof(Creature), typeof(decimal), typeof(Creature), typeof(CardModel), typeof(bool)])]
public static class OmniscientPatch
{
    [HarmonyPostfix]
    static void Postfix(ref Task __result, PowerModel power, Creature target, decimal amount)
    {
        __result = ApplyOmniscient(__result, power, target, amount);
    }

    static async Task ApplyOmniscient(Task originalTask, PowerModel power, Creature target, decimal amount)
    {
        await originalTask;

        if (power is BlackHolePower)
        {
            var omniscient = target.GetPower<OmniscientPower>();
            if (omniscient != null)
            {
                await PowerCmd.Apply<WhiteHolePower>(target, amount * omniscient.Amount, target, null, true);
            }
        }
    }
}
