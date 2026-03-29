using HarmonyLib;
using Joi.JoiCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;

namespace Joi.JoiCode.Patches;

[HarmonyPatch]
public static class IdolCharmRedirectPatch
{
    static IEnumerable<System.Reflection.MethodBase> TargetMethods()
    {
        return AccessTools.GetDeclaredMethods(typeof(CreatureCmd))
            .Where(method => method.Name == nameof(CreatureCmd.Damage));
    }

    [HarmonyPrefix]
    static void Prefix(System.Reflection.MethodBase __originalMethod, object[] __args)
    {
        if (__args.Length < 2)
        {
            return;
        }

        Creature? dealer = __args.OfType<Creature?>().Skip(1).FirstOrDefault();
        switch (__args[1])
        {
            case Creature target:
            {
                var redirectedTarget = TryGetRedirectedTarget(target, dealer);
                if (redirectedTarget == null || redirectedTarget == target)
                {
                    return;
                }

                MainFile.Logger.Info($"[IdolCharm] Redirect single target via {__originalMethod.Name}: {target.Name} -> {redirectedTarget.Name}, dealer={dealer?.Name}");
                __args[1] = redirectedTarget;
                break;
            }
            case IEnumerable<Creature> targets:
            {
                var originalTargets = targets.ToList();
                if (originalTargets.Count == 0)
                {
                    return;
                }

                bool changed = false;
                var redirectedTargets = new List<Creature>(originalTargets.Count);
                foreach (var currentTarget in originalTargets)
                {
                    var redirectedTarget = TryGetRedirectedTarget(currentTarget, dealer);
                    redirectedTargets.Add(redirectedTarget ?? currentTarget);
                    changed |= redirectedTarget != null && redirectedTarget != currentTarget;
                }

                if (!changed)
                {
                    return;
                }

                MainFile.Logger.Info($"[IdolCharm] Redirect multi target via {__originalMethod.Name}: [{string.Join(", ", originalTargets.Select(t => t.Name))}] -> [{string.Join(", ", redirectedTargets.Select(t => t.Name))}], dealer={dealer?.Name}");
                __args[1] = redirectedTargets;
                break;
            }
        }
    }

    private static Creature? TryGetRedirectedTarget(Creature target, Creature? dealer)
    {
        if (target.CombatState == null)
        {
            return null;
        }

        var charmedEnemy = target.CombatState.Enemies
            .FirstOrDefault(enemy => !enemy.IsDead
                                     && enemy.CurrentHp > 0
                                     && enemy.GetPower<IdolCharmPower>() != null);

        if (charmedEnemy == null || charmedEnemy == target)
        {
            return null;
        }

        MainFile.Logger.Info($"[IdolCharm] Candidate redirect: original={target.Name}, redirected={charmedEnemy.Name}, dealer={dealer?.Name}");
        return charmedEnemy;
    }
}
