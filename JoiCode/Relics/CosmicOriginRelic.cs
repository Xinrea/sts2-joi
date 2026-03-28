using Joi.JoiCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Joi.JoiCode.Relics;

public class CosmicOriginRelic : JoiRelic
{
    public override RelicRarity Rarity => RelicRarity.Starter;

    public override decimal ModifyHpLostBeforeOsty(Creature target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (target == Owner.Creature && dealer != Owner.Creature && amount > 0)
        {
            var reduced = amount * 0.5m;
            var blackHoleStacks = (int)Math.Ceiling(amount - reduced);
            if (blackHoleStacks > 0)
            {
                _ = PowerCmd.Apply<BlackHolePower>(target, blackHoleStacks, target, null);
            }
            return reduced;
        }
        return amount;
    }
}
