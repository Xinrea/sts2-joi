using BaseLib.Hooks;
using BaseLib.Utils;
using Joi.JoiCode.Cards;
using Joi.JoiCode.Minions;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace Joi.JoiCode.Powers;

/// <summary>
/// 羁绊：回合开始时，如果有轴芯在场，则获得一张[存储]
/// 当轴芯死亡时自动移除
/// </summary>
public class StoragePower : JoiPower, IOnCreatureDied
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public override async Task BeforeSideTurnStart(PlayerChoiceContext choiceContext, CombatSide side, CombatState combatState)
    {
        if (side != CombatSide.Player) return;

        var definition = ZhouXin.GetSummonDefinition();
        var zhouXin = SummonActions.FindExistingSummon(Owner, definition);

        if (zhouXin != null && zhouXin.IsAlive)
        {
            var card = combatState.CreateCard<Store>(Owner.Player!);
            await CardPileCmd.AddGeneratedCardToCombat(card, PileType.Hand, true);
        }
    }

    public async Task OnCreatureDied(CreatureLifecycleContext context)
    {
        if (context.Kind != CreatureLifecycleKind.Died || context.Monster is not ZhouXin)
            return;

        // 轴芯死亡，移除自己
        MainFile.Logger.Info("[StoragePower] ZhouXin died, removing StoragePower from player");
        await PowerCmd.Remove(this);
    }
}
