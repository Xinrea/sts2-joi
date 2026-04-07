using System.Linq;
using Joi.JoiCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Joi.JoiCode.Relics;

public class CosmicApexRelic : JoiRelic
{
    public override RelicRarity Rarity => RelicRarity.Starter;

    public override decimal ModifyHpLostBeforeOsty(Creature target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        // Don't apply relic effect to summoned creatures (pets)
        if (target.IsPet)
        {
            return amount;
        }

        if (target == Owner.Creature && dealer != null && dealer.Side != target.Side && amount > 0)
        {
            // Check if damage will be intercepted by a pet
            var pets = Owner.Creature.Pets;
            var alivePet = pets.FirstOrDefault(p => !p.IsDead);
            var hasPets = alivePet != null;

            if (hasPets && !props.HasFlag(ValueProp.Unblockable))
            {
                var petHp = alivePet!.CurrentHp;

                // If pet will fully absorb damage, skip relic effect
                if (petHp >= amount)
                {
                    return amount;
                }

                // If there's overflow damage: pet absorbs petHp, player takes reduced overflow
                var overflow = amount - petHp;
                var reducedOverflow = overflow * 0.5m;
                var blackHoleStacks = (int)Math.Ceiling(overflow - reducedOverflow);
                var processed = petHp + reducedOverflow;

                if (blackHoleStacks > 0)
                {
                    _ = PowerCmd.Apply<BlackHolePower>(target, blackHoleStacks, target, null);
                    _ = PowerCmd.Apply<WhiteHolePower>(target, blackHoleStacks, target, null);
                }
                return processed;
            }

            // No pet, apply normal relic effect
            var reduced = amount * 0.5m;
            var blackHoleStacks2 = (int)Math.Ceiling(amount - reduced);
            if (blackHoleStacks2 > 0)
            {
                _ = PowerCmd.Apply<BlackHolePower>(target, blackHoleStacks2, target, null);
                _ = PowerCmd.Apply<WhiteHolePower>(target, blackHoleStacks2, target, null);
            }
            return reduced;
        }
        return amount;
    }
}
