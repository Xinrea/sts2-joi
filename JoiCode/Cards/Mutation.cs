using BaseLib.Utils;
using Joi.JoiCode.Character;
using Joi.JoiCode.Minions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using Joi.JoiCode.Powers;

namespace Joi.JoiCode.Cards;

[Pool(typeof(JoiCardPool))]
public class Mutation : JoiCard
{
    private const string ZhouXinSummonKey = "zhou-xin-core";

    public Mutation() : base(0, CardType.Skill, CardRarity.Common, TargetType.Self) { }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<ZhouXinThornsPower>(3),
        new BlockVar(3, ValueProp.Move)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var definition = ZhouXin.GetSummonDefinition();
        var existing = SummonActions.FindExistingSummon(Owner.Creature, definition);

        // 只有轴芯在场时才能打出，给 Joi 本人加荆棘
        if (existing != null && existing.IsAlive)
        {
            await PowerCmd.Apply<ZhouXinThornsPower>(Owner.Creature, DynamicVars["ZhouXinThornsPower"].BaseValue, Owner.Creature, this);
        }

        Owner.Creature.GainBlockInternal((int)DynamicVars["Block"].BaseValue);
    }

    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Retain);
    }
}
