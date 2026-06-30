using BaseLib.Utils;
using Joi.JoiCode.Character;
using Joi.JoiCode.Minions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace Joi.JoiCode.Cards;

[Pool(typeof(JoiCardPool))]
public class TenfoldManifest : JoiCard
{
    public TenfoldManifest() : base(0, CardType.Skill, CardRarity.Ancient, TargetType.Self) { }

    /// <summary>提高 <see cref="MegaCrit.Sts2.Core.Models.CardModel.MaxUpgradeLevel"/>，使休息处可反复锤炼（与引擎 <c>IsUpgradable</c> 逻辑一致）。</summary>
    public override int MaxUpgradeLevel => 999;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("Heal", 10)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Innate, CardKeyword.Exhaust];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var combatState = Owner.Creature.CombatState;
        if (combatState == null) return;

        var hp = (int)DynamicVars["Heal"].BaseValue;
        var existing = ZhouXin.FindExisting(combatState, Owner);

        if (existing != null)
        {
            var newMaxHp = existing.Creature.MaxHp + hp;
            existing.Creature.SetMaxHpInternal(newMaxHp);
            existing.Creature.HealInternal(hp);
        }
        else
        {
            var creature = await ZhouXin.SummonAsPet(Owner);
            creature.SetMaxHpInternal(hp);
            creature.HealInternal(hp);
            VfxCmd.PlayOnCreature(creature, VfxCmd.healPath);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Heal"].UpgradeValueBy(5);
    }
}
