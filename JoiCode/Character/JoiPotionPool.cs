using BaseLib.Abstracts;
using Joi.JoiCode.Extensions;
using Godot;

namespace Joi.JoiCode.Character;

public class JoiPotionPool : CustomPotionPoolModel
{
    public override Color LabOutlineColor => Joi.Color;
    

    public override string BigEnergyIconPath => "charui/big_energy.png".ImagePath();
    public override string TextEnergyIconPath => "charui/text_energy.png".ImagePath();
}