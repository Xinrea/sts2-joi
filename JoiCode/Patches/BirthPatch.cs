using HarmonyLib;
using Joi.JoiCode.Minions;
using Joi.JoiCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Models;

namespace Joi.JoiCode.Patches;

[HarmonyPatch(typeof(PowerCmd), nameof(PowerCmd.Remove), typeof(PowerModel))]
public static class BirthPatch
{
    [HarmonyPostfix]
    static void Postfix(ref Task __result, PowerModel power)
    {
        __result = HandleBirth(__result, power);
    }

    static async Task HandleBirth(Task originalTask, PowerModel power)
    {
        if (power is BlackHolePower blackHole && blackHole.Amount >= 10)
        {
            var birth = blackHole.Owner.GetPower<BirthPower>();
            if (birth != null)
            {
                var existingCore = blackHole.Owner.CombatState.Allies.FirstOrDefault(c => c.Monster is AxisCore);
                if (existingCore != null)
                {
                    var hpIncrease = blackHole.Amount + (birth.Amount > 1 ? birth.Amount - 1 : 0);
                    existingCore.SetMaxHpInternal(existingCore.MaxHp + hpIncrease);
                    existingCore.HealInternal(hpIncrease);
                }
                else
                {
                    var core = (AxisCore)ModelDb.Monster<AxisCore>().MutableClone();
                    var creature = await CreatureCmd.Add(core, blackHole.Owner.CombatState, blackHole.Owner.Side);
                    creature.SetMaxHpInternal(blackHole.Amount);
                    creature.HealInternal(blackHole.Amount);
                }
            }
        }

        await originalTask;
    }
}
