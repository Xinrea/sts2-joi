using HarmonyLib;
using Joi.JoiCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;

namespace Joi.JoiCode.Patches;

[HarmonyPatch]
public static class UniversalGravitationPatch
{
    static IEnumerable<System.Reflection.MethodBase> TargetMethods()
    {
        return AccessTools.GetDeclaredMethods(typeof(CreatureCmd))
            .Where(method => method.Name == nameof(CreatureCmd.Damage));
    }

    [HarmonyPostfix]
    static void Postfix(Task __result, object[] __args)
    {
        __result.ContinueWith(async _ =>
        {
            if (__args.Length < 2)
            {
                return;
            }

            Creature? dealer = __args.OfType<Creature?>().Skip(1).FirstOrDefault();
            if (dealer?.CombatState == null)
            {
                return;
            }

            var power = dealer.GetPower<UniversalGravitationPower>();
            if (power == null)
            {
                return;
            }

            // Check targets for IdolCharmPower
            switch (__args[1])
            {
                case Creature target:
                    if (target.GetPower<IdolCharmPower>() != null)
                    {
                        await PowerCmd.Apply<BlackHolePower>(dealer, power.Amount, dealer, null);
                    }
                    break;
                case IEnumerable<Creature> targets:
                    foreach (var target in targets)
                    {
                        if (target.GetPower<IdolCharmPower>() != null)
                        {
                            await PowerCmd.Apply<BlackHolePower>(dealer, power.Amount, dealer, null);
                        }
                    }
                    break;
            }
        });
    }
}
