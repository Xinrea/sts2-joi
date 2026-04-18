using BaseLib.Utils;
using Joi.JoiCode.Minions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;

namespace Joi.JoiCode.Powers;

public class WhiteHolePower : JoiPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
    {
        if (side == CombatSide.Player && Amount > 0)
        {
            await CreatureCmd.Damage(choiceContext, Owner.CombatState?.Enemies.ToList() ?? [], Amount, ValueProp.Unpowered, Owner);

            var definition = ZhouXin.GetSummonDefinition();
            var existing = SummonActions.FindExistingSummon(Owner, definition);

            if (existing != null)
            {
                var newMaxHp = existing.MaxHp + 1;
                existing.SetMaxHpInternal(newMaxHp);
                existing.HealInternal(1);
            }
            else if (Owner.Player != null)
            {
                ZhouXin.RandomizeName();
                var zhouXin = await SummonActions.SummonPet(definition, Owner.Player);
                zhouXin.SetMaxHpInternal(1);
                zhouXin.HealInternal(1);
                VfxCmd.PlayOnCreature(zhouXin, VfxCmd.healPath);
            }

            await PowerCmd.Decrement(this);
        }
    }
}
