using MegaCrit.Sts2.Core.Entities.Powers;

namespace Joi.JoiCode.Powers;

/// <summary>
/// 轴芯好帅：轴芯会把存储的牌打出两次
/// </summary>
public class AxisCoreHandsomePower : JoiPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;
}
