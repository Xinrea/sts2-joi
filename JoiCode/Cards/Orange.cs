using BaseLib.Utils;
using Joi.JoiCode.Character;
using Joi.JoiCode.Minions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace Joi.JoiCode.Cards;

[Pool(typeof(JoiCardPool))]
public class Orange : JoiCard
{
    public Orange() : base(0, CardType.Skill, CardRarity.Token, TargetType.Self) { }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("Summon", 2),
        new DynamicVar("Heal", 4)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Retain, CardKeyword.Exhaust];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var combatState = Owner.Creature.CombatState;
        if (combatState == null) return;

        var existing = ZhouXin.FindExisting(combatState, Owner);

        if (existing != null)
        {
            var newMaxHp = existing.Creature.MaxHp + (int)DynamicVars["Summon"].BaseValue;
            existing.Creature.SetMaxHpInternal(newMaxHp);
            existing.Creature.HealInternal((int)DynamicVars["Heal"].BaseValue);
        }
        else
        {
            var creature = await ZhouXin.SummonAsPet(Owner);
            creature.SetMaxHpInternal((int)DynamicVars["Summon"].BaseValue);
            creature.HealInternal((int)DynamicVars["Summon"].BaseValue);
            VfxCmd.PlayOnCreature(creature, VfxCmd.healPath);
        }

        await CardPileCmd.Draw(choiceContext, 1, Owner);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Summon"].UpgradeValueBy(2);
        DynamicVars["Heal"].UpgradeValueBy(2);
    }
}
