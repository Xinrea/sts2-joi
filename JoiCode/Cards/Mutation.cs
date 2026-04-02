using BaseLib.Utils;
using Joi.JoiCode.Character;
using Joi.JoiCode.Minions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Joi.JoiCode.Cards;

[Pool(typeof(JoiCardPool))]
public class Mutation : JoiCard
{
    private const string ZhouXinSummonKey = "zhou-xin-core";

    public Mutation() : base(0, CardType.Skill, CardRarity.Common, TargetType.Self) { }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<ThornsPower>(3)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var definition = SummonDefinition.For<ZhouXin>(uniqueKey: ZhouXinSummonKey);
        var existing = SummonActions.FindExistingSummon(Owner.Creature, definition);

        if (existing != null && existing.IsAlive)
        {
            await PowerCmd.Apply<ThornsPower>(existing, DynamicVars["ThornsPower"].BaseValue, Owner.Creature, this);
        }
    }

    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Retain);
    }
}
