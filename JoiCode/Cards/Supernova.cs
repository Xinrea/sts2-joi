using BaseLib.Utils;
using Joi.JoiCode.Character;
using Joi.JoiCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace Joi.JoiCode.Cards;

[Pool(typeof(JoiCardPool))]
public class Supernova : JoiCard
{
    public Supernova() : base(3, CardType.Attack, CardRarity.Rare, TargetType.AllEnemies) { }

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(20, ValueProp.Move)];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var blackHole = Owner.Creature.GetPower<BlackHolePower>();
        var whiteHole = Owner.Creature.GetPower<WhiteHolePower>();

        var totalStacks = (blackHole?.Amount ?? 0) + (whiteHole?.Amount ?? 0);
        var bonusDamage = totalStacks * 2;
        var totalDamage = DynamicVars.Damage.BaseValue + bonusDamage;

        await CreatureCmd.Damage(choiceContext, CombatState?.Enemies.ToList() ?? [], totalDamage, ValueProp.Move, Owner.Creature, this);

        if (blackHole != null) await PowerCmd.Remove(blackHole);
        if (whiteHole != null) await PowerCmd.Remove(whiteHole);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(6);
    }
}
