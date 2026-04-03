using Joi.JoiCode.Cards;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;

namespace Joi.JoiCode.Powers;

public class OrangeExpertPower : JoiPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner != Owner.Player)
        {
            return;
        }

        // Check if the played card is specifically the Orange card (not Orange Expert, Orange Pile, etc.)
        if (cardPlay.Card is not Orange)
        {
            return;
        }

        var enemies = Owner.CombatState?.Enemies.ToList();
        if (enemies == null || enemies.Count == 0)
        {
            return;
        }

        var randomTarget = enemies[Random.Shared.Next(enemies.Count)];
        await CreatureCmd.Damage(context, [randomTarget], Amount, ValueProp.Unpowered, Owner, cardPlay.Card);
    }
}
