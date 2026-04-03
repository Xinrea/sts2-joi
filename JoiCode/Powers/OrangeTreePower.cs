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
    public override PowerStackType StackType => PowerStackType.None;

    public override async Task AfterSideTurnStart(CombatSide side, CombatState combatState)
    {
        if (side != Owner.Side)
        {
            return;
        }

        var definition = SummonDefinition.For<ZhouXin>(uniqueKey: ZhouXinSummonKey);
        var existing = SummonActions.FindExistingSummon(Owner, definition);

        if (existing != null)
        {
            var newMaxHp = existing.MaxHp + 1;
            existing.SetMaxHpInternal(newMaxHp);
            existing.HealInternal(1);
        }
        else
        {
            ZhouXin.RandomizeName();
            var zhouXin = await SummonActions.SummonPet(definition, Owner.Player!);
            zhouXin.SetMaxHpInternal(1);
            zhouXin.HealInternal(1);
        }

        var card = combatState.CreateCard<Orange>(Owner.Player!);
        await CardPileCmd.AddGeneratedCardToCombat(card, PileType.Hand, true);
    }
}
