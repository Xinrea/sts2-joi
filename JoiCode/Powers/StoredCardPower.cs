using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace Joi.JoiCode.Powers;

/// <summary>
/// 挂在轴芯身上的 Power，存储一张卡牌。
/// 玩家回合开始时自动打出，然后移除自身。
/// </summary>
public class StoredCardPower : JoiPower
{
    private CardModel? _storedCard;
    private bool _played;

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public CardModel? StoredCard => _storedCard;

    public bool HasStoredCard => _storedCard != null;

    public void SetStoredCard(CardModel card)
    {
        _storedCard = card;
        _played = false;
    }

    /// <summary>
    /// 清除轴芯头顶展示的 NCard 节点
    /// </summary>
    private void ClearStoredCardVisual()
    {
        var creatureNode = NCombatRoom.Instance?.GetCreatureNode(Owner);
        var storedCardPos = creatureNode?.GetSpecialNode<Marker2D>("%StoredCardPos");
        if (storedCardPos == null) return;

        foreach (var child in storedCardPos.GetChildren())
        {
            if (child is NCard nCard)
            {
                nCard.QueueFree();
            }
        }
    }

    public override async Task BeforeDeath(Creature target)
    {
        if (target != Owner) return;
        if (_storedCard == null) return;

        var card = _storedCard;
        _storedCard = null;

        ClearStoredCardVisual();

        MainFile.Logger.Info($"[StoredCardPower] ZhouXin dying, returning stored card {card.Id} to hand");
        await CardPileCmd.Add(card, PileType.Hand);
    }

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player != Owner.PetOwner) return;
        if (_storedCard == null || _played) return;

        _played = true;
        var card = _storedCard;
        _storedCard = null;

        ClearStoredCardVisual();

        if (Owner.IsAlive)
        {
            // 检查玩家是否有 AxisCoreHandsomePower（打出两次）
            bool hasAxisCoreHandsome = player.Creature.GetPower<AxisCoreHandsomePower>() != null;

            // 默认打出1次，轴芯好帅打出2次
            int playCount = hasAxisCoreHandsome ? 2 : 1;

            MainFile.Logger.Info($"[StoredCardPower] Auto-playing stored card {card.Id} x{playCount}");

            var cardsToPlay = new List<CardModel> { card };
            if (hasAxisCoreHandsome)
            {
                cardsToPlay.Add(card.CreateDupe());
            }

            foreach (var cardToPlay in cardsToPlay)
            {
                await CardCmd.AutoPlay(choiceContext, cardToPlay, null);
            }
        }
        else
        {
            // 轴芯死亡，卡牌返回弃牌堆
            await CardPileCmd.Add(card, PileType.Discard);
        }

        await PowerCmd.Remove(this);
    }
}
