using HarmonyLib;
using Joi.JoiCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Joi.JoiCode.Patches;

[HarmonyPatch]
public static class GravityFieldPatch
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(PowerCmd), nameof(PowerCmd.Apply), [typeof(PowerModel), typeof(Creature), typeof(decimal), typeof(Creature), typeof(CardModel), typeof(bool)])]
    static void ApplyPostfix(ref Task __result, PowerModel power, Creature target, decimal amount)
    {
        __result = TriggerGravityField(__result, power, target, amount);
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(PowerCmd), nameof(PowerCmd.ModifyAmount), [typeof(PowerModel), typeof(decimal), typeof(Creature), typeof(CardModel), typeof(bool)])]
    static void ModifyAmountPostfix(ref Task<int> __result, PowerModel power, decimal offset)
    {
        __result = TriggerGravityFieldOnModify(__result, power, offset);
    }

    static async Task TriggerGravityField(Task originalTask, PowerModel power, Creature target, decimal amount)
    {
        await originalTask;

        if (power is WhiteHolePower && amount > 0)
        {
            var gravityField = target.GetPower<GravityFieldPower>();
            if (gravityField != null)
            {
                var enemies = target.CombatState?.Enemies.ToList();
                if (enemies != null && enemies.Count > 0)
                {
                    var randomTarget = enemies[Random.Shared.Next(enemies.Count)];
                    await CreatureCmd.Damage(default!, [randomTarget], gravityField.Amount * 3, ValueProp.Unpowered, target);
                }
            }
        }
    }

    static async Task<int> TriggerGravityFieldOnModify(Task<int> originalTask, PowerModel power, decimal offset)
    {
        var result = await originalTask;

        if (power is WhiteHolePower && offset > 0)
        {
            var gravityField = power.Owner.GetPower<GravityFieldPower>();
            if (gravityField != null)
            {
                var enemies = power.Owner.CombatState?.Enemies.ToList();
                if (enemies != null && enemies.Count > 0)
                {
                    var randomTarget = enemies[Random.Shared.Next(enemies.Count)];
                    await CreatureCmd.Damage(default!, [randomTarget], gravityField.Amount * 3, ValueProp.Unpowered, power.Owner);
                }
            }
        }

        return result;
    }
}
