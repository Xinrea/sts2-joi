using BaseLib.Utils;
using Joi.JoiCode.Character;
using Joi.JoiCode.Minions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace Joi.JoiCode.Cards;

[Pool(typeof(JoiCardPool))]
public class SmellsGood : JoiCard
{
    private const string ZhouXinSummonKey = "zhou-xin-core";

    public SmellsGood() : base(1, CardType.Skill, CardRarity.Rare, TargetType.Self) { }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new HealVar(3),
        new DynamicVar("BonusHeal", 5)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.Heal(Owner.Creature, DynamicVars.Heal.BaseValue, true);

        var definition = ZhouXin.GetSummonDefinition();
        var existing = SummonActions.FindExistingSummon(Owner.Creature, definition);

        if (existing != null && existing.IsAlive)
        {
            await CreatureCmd.Kill(existing, true);
            await CreatureCmd.Heal(Owner.Creature, DynamicVars["BonusHeal"].BaseValue, true);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Heal.UpgradeValueBy(1);
    }
}
