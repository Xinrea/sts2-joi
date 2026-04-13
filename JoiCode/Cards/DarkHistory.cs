using BaseLib.Utils;
using Joi.JoiCode.Character;
using Joi.JoiCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Joi.JoiCode.Cards;

[Pool(typeof(JoiCardPool))]
public class DarkHistory : JoiCard
{
    public DarkHistory() : base(1, CardType.Skill, CardRarity.Common, TargetType.AllEnemies) { }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<VulnerablePower>(1),
        new DynamicVar("BlackHole", 1)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var enemies = CombatState?.Enemies.ToList() ?? [];
        foreach (var enemy in enemies)
        {
            await PowerCmd.Apply<VulnerablePower>(enemy, DynamicVars["VulnerablePower"].BaseValue, Owner.Creature, this);
        }
        await CommonActions.ApplySelf<Joi.JoiCode.Powers.BlackHolePower>(this, DynamicVars["BlackHole"].BaseValue);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["VulnerablePower"].UpgradeValueBy(1);
    }
}
