using BaseLib.Abstracts;
using BaseLib.Utils;
using Joi.JoiCode.Character;
using Joi.JoiCode.Enchantments;
using Joi.JoiCode.Minions;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace Joi.JoiCode.Cards;

[Pool(typeof(JoiCardPool))]
public class Mua : JoiCard
{
    private const string ZhouXinSummonKey = "zhou-xin-core";

    public Mua() : base(1, CardType.Skill, CardRarity.Common, TargetType.Self) { }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("Heal", 5)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var definition = ZhouXin.GetSummonDefinition();
        var existing = SummonActions.FindExistingSummon(Owner.Creature, definition);

        Creature zhouXin;

        if (existing != null)
        {
            // 轴芯已存在，增加最大生命值
            var currentMaxHp = existing.MaxHp;
            var newMaxHp = currentMaxHp + DynamicVars["Heal"].IntValue;
            existing.SetMaxHpInternal(newMaxHp);
            existing.HealInternal(DynamicVars["Heal"].IntValue);
            zhouXin = existing;
        }
        else
        {
            // 召唤新的轴芯
            ZhouXin.RandomizeName();
            zhouXin = await SummonActions.SummonPet(definition, Owner);
            zhouXin.SetMaxHpInternal(5);
            zhouXin.HealInternal(5);

            // 播放召唤特效
            VfxCmd.PlayOnCreature(zhouXin, VfxCmd.healPath);
        }

        // 无论是否已有轴芯，都执行喂食机制
        await FeedCardToZhouXin(choiceContext, zhouXin);
    }

    private async Task FeedCardToZhouXin(PlayerChoiceContext choiceContext, Creature zhouXin)
    {
        // 过滤出可以被喂食附魔的手牌
        var feedEnchantment = ModelDb.Enchantment<FeedEnchantment>();
        var hand = Owner.PlayerCombatState?.Hand;
        if (hand == null) return;

        var eligibleCards = hand.Cards
            .Where(c => c != this && feedEnchantment.CanEnchant(c))
            .ToList();

        if (eligibleCards.Count == 0)
        {
            return;
        }

        // 选择一张手牌
        var selectionPrefs = new CardSelectorPrefs(
            new LocString("cards", "JOI-MUA.feed_prompt"), 1);
        var selected = (await CardSelectCmd.FromSimpleGrid(
            choiceContext, eligibleCards, Owner, selectionPrefs)).FirstOrDefault();

        if (selected == null)
        {
            return;
        }

        // 给卡牌添加喂食附魔
        CardCmd.Enchant<FeedEnchantment>(selected, 1);

        MainFile.Logger.Info($"[MUA] Added feed enchantment to card {selected.Id}");
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Heal"].UpgradeValueBy(3);
    }
}
