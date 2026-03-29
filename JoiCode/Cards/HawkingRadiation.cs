using BaseLib.Utils;
using Joi.JoiCode.Character;
using Joi.JoiCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace Joi.JoiCode.Cards;

[Pool(typeof(JoiCardPool))]
public class HawkingRadiation : JoiCard
{
    public HawkingRadiation() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self) { }

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("BlockPerStack", 3)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var blackHole = Owner.Creature.GetPower<BlackHolePower>();
        if (blackHole != null)
        {
            var stacksToRemove = Math.Min(5, blackHole.Amount);

            for (int i = 0; i < stacksToRemove; i++)
            {
                Owner.Creature.GainBlockInternal(DynamicVars["BlockPerStack"].BaseValue);
            }

            blackHole.SetAmount(blackHole.Amount - stacksToRemove, false);
            await Task.CompletedTask;
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["BlockPerStack"].UpgradeValueBy(1);
    }
}
