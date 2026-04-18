using BaseLib.Utils;
using Joi.JoiCode.Character;
using Joi.JoiCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace Joi.JoiCode.Cards;

[Pool(typeof(JoiCardPool))]
public class IdolCharm : JoiCard
{
    public IdolCharm() : base(2, CardType.Skill, CardRarity.Uncommon, TargetType.AnyEnemy) { }

    protected override IEnumerable<DynamicVar> CanonicalVars => [];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var target = cardPlay.Target;
        if (target == null)
        {
            MainFile.Logger.Warn("[IdolCharm] Played without a valid target.");
            return;
        }

        MainFile.Logger.Info($"[IdolCharm] Applying charm to {target.Name}");

        var enemies = CombatState?.Enemies.ToList() ?? [];
        foreach (var enemy in enemies)
        {
            var charm = enemy.GetPower<IdolCharmPower>();
            if (charm != null)
            {
                MainFile.Logger.Info($"[IdolCharm] Removing existing charm from {enemy.Name}");
                await PowerCmd.Remove(charm);
            }
        }

        await PowerCmd.Apply<IdolCharmPower>(target, 1, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
