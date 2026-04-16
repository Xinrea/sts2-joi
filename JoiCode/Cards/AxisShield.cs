using BaseLib.Utils;
using Joi.JoiCode.Character;
using Joi.JoiCode.Minions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace Joi.JoiCode.Cards;

[Pool(typeof(JoiCardPool))]
public class AxisShield : JoiCard
{
    public AxisShield() : base(1, CardType.Skill, CardRarity.Common, TargetType.Self) { }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("Heal", 4),
        new BlockVar(4, ValueProp.Move)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var definition = ZhouXin.GetSummonDefinition();
        var existing = SummonActions.FindExistingSummon(Owner.Creature, definition);

        if (existing != null)
        {
            var newMaxHp = existing.MaxHp + DynamicVars["Heal"].IntValue;
            existing.SetMaxHpInternal(newMaxHp);
            existing.HealInternal(DynamicVars["Heal"].IntValue);
        }
        else
        {
            ZhouXin.RandomizeName();
            var zhouXin = await SummonActions.SummonPet(definition, Owner);
            zhouXin.SetMaxHpInternal(DynamicVars["Heal"].IntValue);
            zhouXin.HealInternal(DynamicVars["Heal"].IntValue);
            VfxCmd.PlayOnCreature(zhouXin, VfxCmd.healPath);
        }

        Owner.Creature.GainBlockInternal((int)DynamicVars["Block"].BaseValue);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Heal"].UpgradeValueBy(2);
        DynamicVars.Block.UpgradeValueBy(2);
    }
}
