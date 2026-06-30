using Joi.JoiCode.Cards;
using Joi.JoiCode.Minions;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace Joi.JoiCode.Powers;

public class OrangeTreePower : JoiPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (side != Owner.Side) return;

        var combatState = Owner.CombatState;
        if (combatState == null) return;

        for (int i = 0; i < Amount; i++)
        {
            var existing = ZhouXin.FindExisting(combatState, Owner.Player!);
            if (existing != null)
            {
                existing.Creature.SetMaxHpInternal(existing.Creature.MaxHp + 1);
                existing.Creature.HealInternal(1);
            }
            else
            {
                var creature = await ZhouXin.SummonAsPet(Owner.Player!);
                creature.SetMaxHpInternal(1);
                creature.HealInternal(1);
                VfxCmd.PlayOnCreature(creature, VfxCmd.healPath);
            }

            var card = combatState.CreateCard<Orange>(Owner.Player!);
            await CardPileCmd.AddGeneratedCardToCombat(card, PileType.Hand, Owner.Player);
        }
    }
}
