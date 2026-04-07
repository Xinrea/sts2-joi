using BaseLib.Utils;
using Joi.JoiCode.Character;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace Joi.JoiCode.Cards;

[Pool(typeof(JoiCardPool))]
public class Seniority : JoiCard
{
    public Seniority() : base(1, CardType.Power, CardRarity.Common, TargetType.Self) { }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<StrengthPower>(1)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<StrengthPower>(Owner.Creature, DynamicVars["StrengthPower"].BaseValue, Owner.Creature, this);

        var enemies = CombatState?.Enemies.ToList() ?? [];
        if (enemies != null && enemies.Count > 0)
        {
            var target = enemies[Random.Shared.Next(enemies.Count)];
            await PowerCmd.Apply<StrengthPower>(target, -1, Owner.Creature, this);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["StrengthPower"].UpgradeValueBy(1);
    }
}
