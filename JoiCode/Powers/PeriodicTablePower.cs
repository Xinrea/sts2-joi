using BaseLib.Utils;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;

namespace Joi.JoiCode.Powers;

public class PeriodicTablePower : JoiPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task BeforeSideTurnStart(PlayerChoiceContext choiceContext, CombatSide side, CombatState combatState)
    {
        if (side != CombatSide.Player) return;

        var drawPile = CardPile.Get(PileType.Draw, Owner.Player!);
        if (drawPile == null || !drawPile.Cards.Any())
        {
            await PowerCmd.Remove(this);
            return;
        }

        var count = (int)Amount;
        for (int i = 0; i < count; i++)
        {
            if (!drawPile.Cards.Any()) break;

            var prefs = new CardSelectorPrefs(new LocString("cards", "JOI-PERIODIC_TABLE.selectionPrompt"), 1);
            var selected = (await CardSelectCmd.FromSimpleGrid(choiceContext, drawPile.Cards, Owner.Player!, prefs)).FirstOrDefault();
            if (selected != null)
            {
                await CardPileCmd.Add(selected, PileType.Hand);
            }
        }

        await PowerCmd.Remove(this);
    }
}
