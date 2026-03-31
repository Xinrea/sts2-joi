using BaseLib.Utils;
using Joi.JoiCode.Character;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;

namespace Joi.JoiCode.Cards;

[Pool(typeof(JoiCardPool))]
public class OrangePile : JoiCard
{
    public OrangePile() : base(1, CardType.Skill, CardRarity.Common, TargetType.Self) { }

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromCard<Orange>(IsUpgraded)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 创建橘子卡的原型
        var orangePrototype = new Orange();

        // 如果橘子堆升级了，创建升级版的橘子
        if (IsUpgraded)
        {
            orangePrototype.UpgradeInternal();
        }

        // 创建两张橘子卡
        var orange1 = orangePrototype.CreateDupe();
        var orange2 = orangePrototype.CreateDupe();

        // 添加到手牌
        await CardPileCmd.AddGeneratedCardToCombat(orange1, PileType.Hand, true, CardPilePosition.Random);
        await CardPileCmd.AddGeneratedCardToCombat(orange2, PileType.Hand, true, CardPilePosition.Random);
    }

    protected override void OnUpgrade()
    {
        // Upgrade changes the generated cards to Orange+
    }
}
