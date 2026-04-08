using System.Collections.Generic;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace Joi.JoiCode.Powers;

public class TimeDilationPower : JoiPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    private readonly HashSet<CardModel> _discountedCards = [];

    public override async Task AfterCardDrawn(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
    {
        if (card.Owner.Creature == Owner && !fromHandDraw)
        {
            _discountedCards.Add(card);
        }
        await Task.CompletedTask;
    }

    public override bool TryModifyEnergyCostInCombat(CardModel card, decimal originalCost, out decimal modifiedCost)
    {
        modifiedCost = originalCost;
        if (!_discountedCards.Contains(card) || originalCost <= 0m)
            return false;
        modifiedCost = Math.Max(0m, originalCost - 1m);
        return true;
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
    {
        _discountedCards.Remove(cardPlay.Card);
        await PowerCmd.Apply<BlackHolePower>(Owner, Amount, Owner, null);
    }

    public override async Task AfterTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
    {
        if (side == Owner.Side)
        {
            _discountedCards.Clear();
            await PowerCmd.Remove(this);
        }
    }
}
