using BaseLib.Utils;
using Joi.JoiCode.Character;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace Joi.JoiCode.Cards;

[Pool(typeof(JoiCardPool))]
public class GravitySlingshot : JoiCard
{
    public GravitySlingshot() : base(0, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy) { }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar("Damage1", 6, ValueProp.Move),
        new DamageVar("Damage2", 12, ValueProp.Move)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var handPile = CardPile.Get(PileType.Hand, Owner);
        var cardCount = handPile?.Cards.Count ?? 0;
        var damage = cardCount <= 1 ? DynamicVars["Damage2"].BaseValue : DynamicVars["Damage1"].BaseValue;

        if (cardPlay.Target != null)
        {
            await CreatureCmd.Damage(choiceContext, [cardPlay.Target], (int)damage, ValueProp.Move, Owner.Creature, this);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Damage1"].UpgradeValueBy(4);
        DynamicVars["Damage2"].UpgradeValueBy(4);
    }
}
