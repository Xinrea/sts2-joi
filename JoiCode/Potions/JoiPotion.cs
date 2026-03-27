using BaseLib.Abstracts;
using BaseLib.Utils;
using Joi.JoiCode.Character;

namespace Joi.JoiCode.Potions;

[Pool(typeof(JoiPotionPool))]
public abstract class JoiPotion : CustomPotionModel;