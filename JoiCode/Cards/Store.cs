using BaseLib.Utils;
using Godot;
using Joi.JoiCode.Character;
using Joi.JoiCode.Minions;
using Joi.JoiCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace Joi.JoiCode.Cards;

[Pool(typeof(JoiCardPool))]
public class Store : JoiCard
{
    public Store() : base(0, CardType.Skill, CardRarity.Token, TargetType.Self) { }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var definition = ZhouXin.GetSummonDefinition();
        var zhouXin = SummonActions.FindExistingSummon(Owner.Creature, definition);

        // 检查轴芯是否存在且可以接受卡牌
        if (zhouXin == null || !zhouXin.IsAlive)
        {
            MainFile.Logger.Info("[Store] No living ZhouXin found");
            return;
        }

        var storedCardPower = zhouXin.GetPower<StoredCardPower>();
        if (storedCardPower != null && storedCardPower.HasStoredCard)
        {
            MainFile.Logger.Info("[Store] ZhouXin already has a stored card");
            return;
        }

        // 选择一张手牌
        var card = await CommonActions.SelectSingleCard(this, new LocString("cards", "JOI-STORE.selectionPrompt"), choiceContext, PileType.Hand);
        if (card == null)
        {
            MainFile.Logger.Info("[Store] No card selected");
            return;
        }

        MainFile.Logger.Info($"[Store] Storing card {card.Id} to ZhouXin");

        // 获取或创建 StoredCardPower
        if (storedCardPower == null)
        {
            await PowerCmd.Apply<StoredCardPower>(zhouXin, 1, Owner.Creature, null);
            storedCardPower = zhouXin.GetPower<StoredCardPower>();
        }

        storedCardPower?.SetStoredCard(card);

        // 从手牌中移除（数据层和 UI 层）
        var nCard = NCard.FindOnTable(card);
        if (nCard != null)
        {
            NCombatRoom.Instance?.Ui.Hand.Remove(card);
            nCard.QueueFree();
        }
        card.Pile?.RemoveInternal(card, silent: true);

        // 在轴芯头顶创建 NCard 展示
        var creatureNode = NCombatRoom.Instance?.GetCreatureNode(zhouXin);
        var storedCardPos = creatureNode?.GetSpecialNode<Marker2D>("%StoredCardPos");
        if (storedCardPos != null)
        {
            var displayCard = NCard.Create(card);
            if (displayCard != null)
            {
                storedCardPos.AddChild(displayCard);
                displayCard.Position = -displayCard.Size * 0.5f;
                displayCard.UpdateVisuals(PileType.Hand, CardPreviewMode.Normal);
            }
        }

        VfxCmd.PlayOnCreature(zhouXin, VfxCmd.healPath);
    }

    protected override void OnUpgrade()
    {
        // No upgrade effect
    }
}
