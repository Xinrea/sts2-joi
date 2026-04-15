using BaseLib.Utils;
using Joi.JoiCode.Minions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Joi.JoiCode.Powers;

/// <summary>
/// 挂在 Joi 身上的荆棘，当轴芯死亡时移除
/// </summary>
public class ZhouXinThornsPower : JoiPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    // 复用原版荆棘图标
    public override string CustomPackedIconPath => ImageHelper.GetImagePath("atlases/power_atlas.sprites/thorns_power.tres");
    public override string CustomBigIconPath => ImageHelper.GetImagePath("powers/thorns_power.png");

    public override async Task BeforeDamageReceived(PlayerChoiceContext choiceContext, Creature target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (target == Owner && dealer != null && props.HasFlag(ValueProp.Move) && !props.HasFlag(ValueProp.Unpowered))
        {
            Flash();
            await CreatureCmd.Damage(choiceContext, dealer, Amount, ValueProp.Unpowered | ValueProp.SkipHurtAnim, Owner, null);
        }
    }

    public override async Task AfterDeath(PlayerChoiceContext choiceContext, Creature creature, bool wasRemovalPrevented, float deathAnimLength)
    {
        // 轴芯死亡时移除此荆棘
        if (creature.IsPet && creature.Monster?.GetType() == typeof(ZhouXin))
        {
            await PowerCmd.Remove(this);
        }
    }
}
