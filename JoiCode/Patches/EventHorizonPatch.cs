using HarmonyLib;
using Joi.JoiCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Joi.JoiCode.Patches;

[HarmonyPatch]
public static class EventHorizonPatch
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(PowerCmd), nameof(PowerCmd.Apply), [typeof(PowerModel), typeof(Creature), typeof(decimal), typeof(Creature), typeof(CardModel), typeof(bool)])]
    static void ApplyPostfix(ref Task __result, PowerModel power, Creature target, decimal amount)
    {
        __result = TriggerEventHorizon(__result, power, target, amount);
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(PowerCmd), nameof(PowerCmd.ModifyAmount), [typeof(PowerModel), typeof(decimal), typeof(Creature), typeof(CardModel), typeof(bool)])]
    static void ModifyAmountPostfix(ref Task<int> __result, PowerModel power, decimal offset)
    {
        __result = TriggerEventHorizonOnModify(__result, power, offset);
    }

    static async Task TriggerEventHorizon(Task originalTask, PowerModel power, Creature target, decimal amount)
    {
        await originalTask;

        if (power is BlackHolePower && amount > 0)
        {
            var eventHorizon = target.GetPower<EventHorizonPower>();
            if (eventHorizon != null)
            {
                target.GainBlockInternal((int)amount * eventHorizon.Amount);
            }
        }
    }

    static async Task<int> TriggerEventHorizonOnModify(Task<int> originalTask, PowerModel power, decimal offset)
    {
        var result = await originalTask;

        if (power is BlackHolePower && offset > 0)
        {
            var eventHorizon = power.Owner.GetPower<EventHorizonPower>();
            if (eventHorizon != null)
            {
                power.Owner.GainBlockInternal((int)offset * eventHorizon.Amount);
            }
        }

        return result;
    }
}
