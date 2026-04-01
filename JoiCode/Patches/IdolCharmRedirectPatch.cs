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

        // 找到 dealer 参数的索引（类型为 Creature 且不是 target）
        int dealerIndex = -1;
        int targetIndex = -1;
        var parameters = __originalMethod.GetParameters();
        for (int i = 0; i < parameters.Length; i++)
        {
            var name = parameters[i].Name;
            if (name == "target" || name == "targets") targetIndex = i;
            if (name == "dealer") dealerIndex = i;
        }

        switch (__args[targetIndex])
        {
            case Creature target:
            {
                var result = TryRedirect(target);
                if (result == null) return;

                MainFile.Logger.Info($"[IdolCharm] Redirect: {target.Name} -> {result.Charmed.Name}, dealer -> {result.Player.Name}");
                __args[targetIndex] = result.Charmed;
                if (dealerIndex >= 0) __args[dealerIndex] = result.Player;
                break;
            }
            case IEnumerable<Creature> targets:
            {
                var originalTargets = targets.ToList();
                if (originalTargets.Count == 0) return;

                RedirectResult? firstResult = null;
                bool changed = false;
                var redirectedTargets = new List<Creature>(originalTargets.Count);
                foreach (var currentTarget in originalTargets)
                {
                    var result = TryRedirect(currentTarget);
                    if (result != null)
                    {
                        redirectedTargets.Add(result.Charmed);
                        firstResult ??= result;
                        changed = true;
                    }
                    else
                    {
                        redirectedTargets.Add(currentTarget);
                    }
                }

                if (!changed) return;

                MainFile.Logger.Info($"[IdolCharm] Redirect multi: [{string.Join(", ", originalTargets.Select(t => t.Name))}] -> [{string.Join(", ", redirectedTargets.Select(t => t.Name))}]");
                __args[targetIndex] = redirectedTargets;
                if (dealerIndex >= 0 && firstResult != null) __args[dealerIndex] = firstResult.Player;
                break;
            }
        }
    }

    private sealed record RedirectResult(Creature Charmed, Creature Player);

    private static RedirectResult? TryRedirect(Creature target)
    {
        if (target.CombatState == null) return null;

        var charmedEnemy = target.CombatState.Enemies
            .FirstOrDefault(enemy => !enemy.IsDead
                                     && enemy.CurrentHp > 0
                                     && enemy.GetPower<IdolCharmPower>() != null);

        if (charmedEnemy == null || charmedEnemy == target) return null;

        // 获取玩家角色作为新的 dealer
        var player = target.CombatState.PlayerCreatures
            .FirstOrDefault(c => !c.IsDead && !c.IsPet);

        if (player == null) return null;

        return new RedirectResult(charmedEnemy, player);
    }
}
