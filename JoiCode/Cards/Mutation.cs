using BaseLib.Utils;
using Joi.JoiCode.Character;
using Joi.JoiCode.Minions;
using Joi.JoiCode.Powers;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace Joi.JoiCode.Cards;

[Pool(typeof(JoiCardPool))]
public class Mutation : JoiCard
{
    public Mutation() : base(0, CardType.Skill, CardRarity.Common, TargetType.Self) { }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<ZhouXinThornsPower>(3),
        new BlockVar(3, ValueProp.Move)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var combatState = Owner.Creature.CombatState;
        if (combatState == null) return;

        var existing = ZhouXin.FindExisting(combatState, Owner);

        // Only applies thorns if ZhouXin is in play
        if (existing != null && existing.Creature.IsAlive)
        {
            await CommonActions.Apply<ZhouXinThornsPower>(Owner.Creature, this, DynamicVars["ZhouXinThornsPower"].BaseValue);
        }

        Owner.Creature.GainBlockInternal((int)DynamicVars["Block"].BaseValue);
    }

    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Retain);
    }
}
