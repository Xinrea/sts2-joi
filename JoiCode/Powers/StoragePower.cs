using Joi.JoiCode.Cards;
using Joi.JoiCode.Minions;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace Joi.JoiCode.Powers;

/// <summary>
/// Bond: At the start of turn, if ZhouXin is on the field, gain a [Store] card.
/// Automatically removed when ZhouXin dies.
/// </summary>
public class StoragePower : JoiPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public override async Task BeforeSideTurnStart(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IReadOnlyList<Creature> participants,
        ICombatState combatState)
    {
        if (side != CombatSide.Player) return;

        var zhouXin = ZhouXin.FindExisting(combatState, Owner.Player!);
        if (zhouXin != null && zhouXin.Creature.IsAlive)
        {
            var card = combatState.CreateCard<Store>(Owner.Player!);
            await CardPileCmd.AddGeneratedCardToCombat(card, PileType.Hand, Owner.Player);
        }
    }
}
