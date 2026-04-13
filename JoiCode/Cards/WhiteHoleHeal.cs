using BaseLib.Utils;
using Joi.JoiCode.Character;
using Joi.JoiCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace Joi.JoiCode.Cards;

[Pool(typeof(JoiCardPool))]
public class WhiteHoleHeal : JoiCard
{
    public WhiteHoleHeal() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self) { }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("WhiteHole", 5)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var whiteHole = Owner.Creature.GetPower<WhiteHolePower>();
        var stacks = (int)(whiteHole?.Amount ?? 0);
        var maxRemove = (int)DynamicVars["WhiteHole"].BaseValue;
        var removed = Math.Min(stacks, maxRemove);

        if (removed > 0 && whiteHole != null)
        {
            var newAmount = whiteHole.Amount - removed;
            if (newAmount == 0)
                await PowerCmd.Remove(whiteHole);
            else
                whiteHole.SetAmount(newAmount, false);

            // Heal
            Owner.Creature.HealInternal(removed);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["WhiteHole"].UpgradeValueBy(2);
    }
}
