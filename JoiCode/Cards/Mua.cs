using BaseLib.Abstracts;
using BaseLib.Utils;
using Joi.JoiCode.Character;
using Joi.JoiCode.Minions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace Joi.JoiCode.Cards;

[Pool(typeof(JoiCardPool))]
public class Mua : JoiCard
{
    private const string ZhouXinSummonKey = "zhou-xin-core";

    public Mua() : base(1, CardType.Skill, CardRarity.Common, TargetType.Self) { }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("Heal", 8)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var definition = ZhouXin.GetSummonDefinition();
        var existing = SummonActions.FindExistingSummon(Owner.Creature, definition);

        if (existing != null)
        {
            // 轴芯已存在，增加最大生命值
            var currentMaxHp = existing.MaxHp;
            var newMaxHp = currentMaxHp + DynamicVars["Heal"].IntValue;
            existing.SetMaxHpInternal(newMaxHp);
            existing.HealInternal(DynamicVars["Heal"].IntValue);
        }
        else
        {
            // 召唤新的轴芯
            ZhouXin.RandomizeName();
            var zhouXin = await SummonActions.SummonPet(definition, Owner);
            zhouXin.SetMaxHpInternal(8);
            zhouXin.HealInternal(8);

            VfxCmd.PlayOnCreature(zhouXin, VfxCmd.healPath);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Heal"].UpgradeValueBy(4);
    }
}
