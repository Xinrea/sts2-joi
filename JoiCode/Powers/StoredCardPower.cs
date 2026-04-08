using BaseLib.Abstracts;
using BaseLib.Utils;
using Godot;
using Joi.JoiCode.Minions;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
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

    public override async Task BeforeSideTurnStart(PlayerChoiceContext choiceContext, CombatSide side, CombatState combatState)
    {
        if (side != CombatSide.Player) return;
        if (_storedCard == null || _played) return;

        _played = true;
        var card = _storedCard;
        _storedCard = null;

        ClearStoredCardVisual();

        if (Owner.IsAlive)
        {
            MainFile.Logger.Info($"[StoredCardPower] Auto-playing stored card {card.Id}");
            await CardPileCmd.Add(card, PileType.Hand);
            await CardCmd.AutoPlay(choiceContext, card, null);
        }
        else
        {
            await CardPileCmd.Add(card, PileType.Discard);
        }

        await PowerCmd.Remove(this);
    }
}
