using BaseLib.Utils;
using Joi.JoiCode.Character;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace Joi.JoiCode.Cards;

[Pool(typeof(JoiCardPool))]
public class BurningPact : JoiCard
{
    public BurningPact() : base(1, CardType.Skill, CardRarity.Common, TargetType.Self) { }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("HPCost", 2),
        new BlockVar(8, ValueProp.Move)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var hpCost = IsUpgraded ? 1 : 2;
        // Lose HP by dealing damage to self
        await CreatureCmd.Damage(choiceContext, [Owner.Creature], hpCost, ValueProp.Unpowered, Owner.Creature, this);
        // Gain Block
        Owner.Creature.GainBlockInternal(DynamicVars.Block.BaseValue);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["HPCost"].UpgradeValueBy(-1);
        DynamicVars.Block.UpgradeValueBy(4);
    }
}
