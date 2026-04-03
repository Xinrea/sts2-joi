using BaseLib.Hooks;
using BaseLib.Utils;
using Joi.JoiCode.Minions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Powers;

namespace Joi.JoiCode.Powers;

public class BirthPower : JoiPower, IOnPowerRemoved
{
    private const string ZhouXinSummonKey = "zhou-xin-core";

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public async Task OnPowerRemoved(PowerChangeContext context)
    {
        if (context.Target != Owner || context.PreviousAmount < 5 || Owner.CombatState == null || context.Power is not BlackHolePower)
        {
            return;
        }

        int summonHp = (int)context.PreviousAmount;
        int hpIncrease = summonHp + (Amount > 1 ? (int)(Amount - 1) : 0);
        bool reusedExisting = false;

        ZhouXin.RandomizeName();
        var creature = await SummonActions.SummonAlly(
            SummonDefinition.For<ZhouXin>(
                uniqueKey: ZhouXinSummonKey,
                policy: SummonPolicy.BuffExisting,
                handleExisting: existing =>
                {
                    reusedExisting = true;
                    existing.SetMaxHpInternal(existing.MaxHp + hpIncrease);
                    existing.HealInternal(hpIncrease);
                    return Task.CompletedTask;
                }),
            Owner);

        if (reusedExisting)
        {
            return;
        }

        creature.SetMaxHpInternal(summonHp);
        creature.HealInternal(summonHp);

        // 播放召唤特效
        VfxCmd.PlayOnCreature(creature, VfxCmd.healPath);

        if (Owner.Player != null)
        {
            await SummonActions.EnsurePetRegistered(creature, Owner.Player);
        }
    }
}
