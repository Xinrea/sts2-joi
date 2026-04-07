using BaseLib.Abstracts;
using BaseLib.Utils;
using Joi.JoiCode.Character;
using Joi.JoiCode.Minions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace Joi.JoiCode.Cards;

[Pool(typeof(JoiCardPool))]
public class Orange : JoiCard
{
    private const string ZhouXinSummonKey = "zhou-xin-core";

    public Orange() : base(0, CardType.Skill, CardRarity.Token, TargetType.Self) { }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Retain, CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("Summon", 2),
        new DynamicVar("Heal", 4)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var definition = ZhouXin.GetSummonDefinition();
        var existing = SummonActions.FindExistingSummon(Owner.Creature, definition);

        if (existing != null)
        {
            // 增加轴芯的最大生命值
            var newMaxHp = existing.MaxHp + DynamicVars["Summon"].IntValue;
            existing.SetMaxHpInternal(newMaxHp);
            // 恢复生命值
            existing.HealInternal(DynamicVars["Heal"].IntValue);
        }
        else
        {
            // 召唤新的轴芯
            ZhouXin.RandomizeName();
            var zhouXin = await SummonActions.SummonPet(definition, Owner);
            zhouXin.SetMaxHpInternal(DynamicVars["Summon"].IntValue);
            zhouXin.HealInternal(DynamicVars["Summon"].IntValue);

            // 播放召唤特效
            VfxCmd.PlayOnCreature(zhouXin, VfxCmd.healPath);
        }

        await CommonActions.Draw(this, 1, choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Summon"].UpgradeValueBy(2);
        DynamicVars["Heal"].UpgradeValueBy(2);
    }
}
