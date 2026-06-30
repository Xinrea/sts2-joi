using BaseLib.Utils;
using Joi.JoiCode.Character;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace Joi.JoiCode.Cards;

[Pool(typeof(JoiCardPool))]
public class OrangePile : JoiCard
{
    public OrangePile() : base(1, CardType.Skill, CardRarity.Common, TargetType.Self) { }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("Heal", 5)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromCard<Orange>(IsUpgraded)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // TODO: Reimplement summon/heal logic without SummonActions

        // 通过 CombatState 创建橘子卡
        var combatState = Owner.Creature?.CombatState;
        if (combatState == null) return;
        var orange1 = combatState.CreateCard<Orange>(Owner);
        var orange2 = combatState.CreateCard<Orange>(Owner);

        if (IsUpgraded)
        {
            orange1.UpgradeInternal();
            orange2.UpgradeInternal();
        }

        // 添加到手牌
        await CardPileCmd.AddGeneratedCardsToCombat(
            [orange1, orange2], PileType.Hand, Owner, CardPilePosition.Random);
    }

    protected override void OnUpgrade()
    {
        // Upgrade changes the generated cards to Orange+
    }
}
