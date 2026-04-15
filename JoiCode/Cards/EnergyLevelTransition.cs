using BaseLib.Utils;
using Joi.JoiCode.Character;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace Joi.JoiCode.Cards;

[Pool(typeof(JoiCardPool))]
public class EnergyLevelTransition : JoiCard
{
    public EnergyLevelTransition() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self) { }

    protected override IEnumerable<DynamicVar> CanonicalVars => [];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var playerCombatState = Owner.Creature.Player?.PlayerCombatState;
        if (playerCombatState == null) return;

        var maxEnergy = playerCombatState.MaxEnergy;
        var currentEnergy = playerCombatState.Energy;

        // 随机设置能量为 0 到 maxEnergy 之间的值
        var randomEnergy = Random.Shared.Next(0, maxEnergy + 1);
        var energyDiff = randomEnergy - currentEnergy;

        if (energyDiff > 0)
        {
            await PlayerCmd.GainEnergy(energyDiff, Owner);
        }
        else if (energyDiff < 0)
        {
            await PlayerCmd.LoseEnergy(-energyDiff, Owner);
        }
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
