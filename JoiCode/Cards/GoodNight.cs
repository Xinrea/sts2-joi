using BaseLib.Utils;
using Joi.JoiCode.Character;
using Joi.JoiCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace Joi.JoiCode.Cards;

[Pool(typeof(JoiCardPool))]
public class GoodNight : JoiCard
{
    public GoodNight() : base(2, CardType.Skill, CardRarity.Uncommon, TargetType.AllEnemies) { }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new HealVar(5),
        new PowerVar<SleepPower>(1)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.Heal(Owner.Creature, DynamicVars.Heal.BaseValue, true);

        var enemies = CombatState?.Enemies.ToList() ?? [];
        foreach (var enemy in enemies)
        {
            await PowerCmd.Apply<SleepPower>(enemy, DynamicVars["SleepPower"].BaseValue, Owner.Creature, this);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Heal.UpgradeValueBy(2);
    }
}
