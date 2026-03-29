using BaseLib.Utils;
using Joi.JoiCode.Character;
using Joi.JoiCode.Minions;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace Joi.JoiCode.Cards;

[Pool(typeof(JoiCardPool))]
public class Mua : JoiCard
{
    private const string ZhouXinSummonKey = "zhou-xin-core";

    public Mua() : base(1, CardType.Skill, CardRarity.Common, TargetType.Self) { }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("Heal", 5)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var definition = SummonDefinition.For<ZhouXin>(uniqueKey: ZhouXinSummonKey);
        var existing = SummonActions.FindExistingSummon(Owner.Creature, definition);
        if (existing != null)
        {
            existing.HealInternal(DynamicVars["Heal"].IntValue);
            return;
        }

        var creature = await SummonActions.SummonPet(definition, Owner);
        creature.SetMaxHpInternal(5);
        creature.HealInternal(5);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Heal"].UpgradeValueBy(3);
    }
}
