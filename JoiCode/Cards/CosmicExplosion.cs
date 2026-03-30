using BaseLib.Utils;
using Joi.JoiCode.Character;
using Joi.JoiCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace Joi.JoiCode.Cards;

[Pool(typeof(JoiCardPool))]
public class CosmicExplosion : JoiCard
{
    public CosmicExplosion() : base(-1, CardType.Attack, CardRarity.Rare, TargetType.AllEnemies) { }

    protected override bool HasEnergyCostX => true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new StringVar("ExtraTimes", IsUpgraded ? "+1" : "")
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var blackHolePower = Owner.Creature.GetPower<BlackHolePower>();
        if (blackHolePower == null) return;

        var damage = blackHolePower.Amount;
        var times = ResolveEnergyXValue();
        if (IsUpgraded)
        {
            times++;
        }

        for (int i = 0; i < times; i++)
        {
            await CreatureCmd.Damage(choiceContext, CombatState?.Enemies.ToList() ?? [], damage, ValueProp.Move, Owner.Creature, this);
        }

        if (times > 0)
        {
            await PowerCmd.Remove(blackHolePower);
        }
    }

    protected override void OnUpgrade()
    {
        // Upgrade is handled in OnPlay and CanonicalVars
    }
}
