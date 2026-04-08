using BaseLib.Utils;
using Joi.JoiCode.Cards;
using Joi.JoiCode.Minions;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;

namespace Joi.JoiCode.Powers;

public class OrangeTreePower : JoiPower
{
    private const string ZhouXinSummonKey = "zhou-xin-core";

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterSideTurnStart(CombatSide side, CombatState combatState)
    {
        if (side != Owner.Side)
        {
            return;
        }

        var stacks = (int)Amount;

        var definition = ZhouXin.GetSummonDefinition();
        var existing = SummonActions.FindExistingSummon(Owner, definition);

        if (existing != null)
        {
            var newMaxHp = existing.MaxHp + stacks*3;
            existing.SetMaxHpInternal(newMaxHp);
            existing.HealInternal(stacks*3);
        }
        else
        {
            ZhouXin.RandomizeName();
            var zhouXin = await SummonActions.SummonPet(definition, Owner.Player!);
            zhouXin.SetMaxHpInternal(stacks*3);
            zhouXin.HealInternal(stacks*3);

            // 播放召唤特效
            VfxCmd.PlayOnCreature(zhouXin, VfxCmd.healPath);
        }

        for (int i = 0; i < stacks; i++)
        {
            var card = combatState.CreateCard<Orange>(Owner.Player!);
            await CardPileCmd.AddGeneratedCardToCombat(card, PileType.Hand, true);
        }
    }
}
