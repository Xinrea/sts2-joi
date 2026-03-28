using HarmonyLib;
using Joi.JoiCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;

namespace Joi.JoiCode.Patches;

[HarmonyPatch]
public static class OmniscientPatch
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(PowerCmd), nameof(PowerCmd.Apply), [typeof(PowerModel), typeof(Creature), typeof(decimal), typeof(Creature), typeof(CardModel), typeof(bool)])]
    static void ApplyPostfix(ref Task __result, PowerModel power, Creature target, decimal amount)
    {
        __result = TriggerOmniscientOnApply(__result, power, target, amount);
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(PowerCmd), nameof(PowerCmd.ModifyAmount), [typeof(PowerModel), typeof(decimal), typeof(Creature), typeof(CardModel), typeof(bool)])]
    static void ModifyAmountPostfix(ref Task<int> __result, PowerModel power, decimal offset)
    {
        __result = TriggerOmniscientOnModify(__result, power, offset);
    }

    static async Task TriggerOmniscientOnApply(Task originalTask, PowerModel power, Creature target, decimal amount)
    {
        await originalTask;

        if (power is BlackHolePower && amount > 0)
        {
            var omniscient = target.GetPower<OmniscientPower>();
            if (omniscient != null)
            {
                await PowerCmd.Apply<WhiteHolePower>(target, omniscient.Amount, target, null, true);
            }
        }
    }

    static async Task<int> TriggerOmniscientOnModify(Task<int> originalTask, PowerModel power, decimal offset)
    {
        var result = await originalTask;

        if (power is BlackHolePower && offset > 0)
        {
            var omniscient = power.Owner.GetPower<OmniscientPower>();
            if (omniscient != null)
            {
                await PowerCmd.Apply<WhiteHolePower>(power.Owner, omniscient.Amount, power.Owner, null, true);
            }
        }

        return result;
    }
}
