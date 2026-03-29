using BaseLib.Utils;
using Joi.JoiCode.Character;
using Joi.JoiCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace Joi.JoiCode.Cards;

[Pool(typeof(JoiCardPool))]
public class QuantumEntanglement : JoiCard
{
    public QuantumEntanglement() : base(2, CardType.Skill, CardRarity.Uncommon, TargetType.Self) { }

    protected override IEnumerable<DynamicVar> CanonicalVars => [];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var blackHole = Owner.Creature.GetPower<BlackHolePower>();
        if (blackHole != null)
        {
            var transfer = blackHole.Amount / 2;
            if (transfer > 0)
            {
                blackHole.SetAmount(blackHole.Amount - transfer, false);
                await CommonActions.ApplySelf<WhiteHolePower>(this, transfer);
            }
        }
    }
}
