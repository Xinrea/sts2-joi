using BaseLib.Utils;
using Joi.JoiCode.Character;
using Joi.JoiCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace Joi.JoiCode.Cards;

[Pool(typeof(JoiCardPool))]
public class PhotonJet : JoiCard
{
    public PhotonJet() : base(1, CardType.Skill, CardRarity.Common, TargetType.Self) { }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("Triggers", 1)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var whiteHolePower = Owner.Creature.GetPower<WhiteHolePower>();
        if (whiteHolePower != null && whiteHolePower.Amount > 0)
        {
            var triggers = DynamicVars["Triggers"].IntValue;
            for (int i = 0; i < triggers; i++)
            {
                if (whiteHolePower.Amount > 0)
                {
                    await CreatureCmd.Damage(choiceContext, Owner.Creature.CombatState?.Enemies.ToList() ?? [], whiteHolePower.Amount, ValueProp.Unpowered, Owner.Creature);
                    await PowerCmd.Decrement(whiteHolePower);
                }
            }
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Triggers"].UpgradeValueBy(1);
    }
}
