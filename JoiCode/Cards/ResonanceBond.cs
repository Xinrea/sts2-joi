using BaseLib.Utils;
using Joi.JoiCode.Character;
using Joi.JoiCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace Joi.JoiCode.Cards;

[Pool(typeof(JoiCardPool))]
public class ResonanceBond : JoiCard
{
    public ResonanceBond() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.AnyEnemy) { }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("WhiteHole", 2),
        new DynamicVar("BlackHole", 2)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var target = cardPlay.Target;
        if (target == null)
        {
            return;
        }

        // Remove existing charm from all enemies
        var enemies = CombatState?.Enemies.ToList() ?? [];
        foreach (var enemy in enemies)
        {
            var charm = enemy.GetPower<IdolCharmPower>();
            if (charm != null)
            {
                await PowerCmd.Remove(charm);
            }
        }

        // Apply charm to target
        await PowerCmd.Apply<IdolCharmPower>(target, 1, Owner.Creature, this);

        // Apply White Hole and Black Hole to self
        await CommonActions.ApplySelf<WhiteHolePower>(this, DynamicVars["WhiteHole"].BaseValue);
        await CommonActions.ApplySelf<BlackHolePower>(this, DynamicVars["BlackHole"].BaseValue);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["WhiteHole"].UpgradeValueBy(1);
    }
}
