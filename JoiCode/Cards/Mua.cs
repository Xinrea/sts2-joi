using BaseLib.Utils;
using Joi.JoiCode.Character;
using Joi.JoiCode.Minions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace Joi.JoiCode.Cards;

[Pool(typeof(JoiCardPool))]
public class Mua : JoiCard
{
    public Mua() : base(1, CardType.Skill, CardRarity.Common, TargetType.Self) { }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("Heal", 8)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var combatState = Owner.Creature.CombatState;
        if (combatState == null) return;

        var existing = ZhouXin.FindExisting(combatState, Owner);

        if (existing != null)
        {
            var newMaxHp = existing.Creature.MaxHp + (int)DynamicVars["Heal"].BaseValue;
            existing.Creature.SetMaxHpInternal(newMaxHp);
            existing.Creature.HealInternal((int)DynamicVars["Heal"].BaseValue);
        }
        else
        {
            var creature = await ZhouXin.SummonAsPet(Owner);
            creature.SetMaxHpInternal((int)DynamicVars["Heal"].BaseValue);
            creature.HealInternal((int)DynamicVars["Heal"].BaseValue);
            VfxCmd.PlayOnCreature(creature, VfxCmd.healPath);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Heal"].UpgradeValueBy(4);
    }
}
