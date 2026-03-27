`BaseLib`是统一添加新内容行为的基础mod，类似于塔1的`basemod`和`stslib`。

https://github.com/Alchyr/BaseLib-StS2

> 由于目前`BaseLib`尚处于开发阶段，如果只打patch不添加新内容可以不使用。

## 下载

* 前往 https://github.com/Alchyr/BaseLib-StS2/releases 下载`dll`，`pck`和`json`三个文件，把他们放在`mods`文件夹里。记住你下载的版本。

* 在`csproj`文件中相应位置引用`BaseLib.dll`，如下，两种方式都可。

```xml
  <ItemGroup>
    <Reference Include="sts2">
      <HintPath>$(Sts2DataDir)/sts2.dll</HintPath>
      <Private>false</Private>
    </Reference>

    <Reference Include="0Harmony">
      <HintPath>$(Sts2DataDir)/0Harmony.dll</HintPath>
      <Private>false</Private>
    </Reference>

    <!-- 本地引用，注意路径是否正确 -->
    <Reference Include="BaseLib">
      <HintPath>$(Sts2Dir)/mods/BaseLib/BaseLib.dll</HintPath>
      <Private>false</Private>
    </Reference>
    <!-- NuGet获取，注意版本是否一致，不一致手动更改Version -->
    <!-- <PackageReference Include="Alchyr.Sts2.BaseLib" Version="*" /> -->
  </ItemGroup>
```

* 不要忘了在你`{modid}.json`中填写`dependencies`。

```json
  "dependencies": ["BaseLib"],
```

## 添加新卡牌

### 代码

创建一个新的`Cards`文件夹方便管理，并创建新的`cs`文件，例如`TestCard.cs`。

```csharp
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.ValueProps;

namespace Test.Scripts;

// 加入哪个卡池
[Pool(typeof(ColorlessCardPool))]
public class TestCard : CustomCardModel
{
    // 基础耗能
    private const int energyCost = 1;
    // 卡牌类型
    private const CardType type = CardType.Attack;
    // 卡牌稀有度
    private const CardRarity rarity = CardRarity.Common;
    // 目标类型（AnyEnemy表示任意敌人）
    private const TargetType targetType = TargetType.AnyEnemy;
    // 是否在卡牌图鉴中显示
    private const bool shouldShowInCardLibrary = true;

    // 卡牌的基础属性（例如这里是12点伤害）
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(12, ValueProp.Move)];

    public TestCard() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    // 打出时的效果逻辑
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue) // 造成伤害，数值来源于卡牌的基础伤害属性
            .FromCard(this) // 伤害来源于这张卡牌
            .Targeting(cardPlay.Target) // 伤害目标是玩家选择的目标
            .Execute(choiceContext);
    }

    // 升级后的效果逻辑
    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(4); // 升级后增加4点伤害
    }
}
```

* `CanonicalVars`翻译是“规范值”，指卡牌的基础数值。添加一个`DamageVar`意为指定卡牌的基础伤害是多少，例如这里是`12`。

* `ValueProp`表示数值的属性，例如`ValueProp.Move`表示是通过卡牌造成的伤害/格挡，`ValueProp.Unpowered`表示不受修正影响（如力量等），`ValueProp.Unblockable`表示伤害不可被格挡，`ValueProp.SkipHurtAnim`表示跳过受伤动画。这是一个bitflag类型的枚举，你可以进行组合，例如`ValueProp.Unblockable | ValueProp.Unpowered`，不可被格挡也不受修正影响。

* 尖塔2使用了`async`和`await`来控制效果逻辑顺序执行，比如选择一张牌时就一直`await`不让后续代码执行，和尖塔1的`action`类似的生态位。此处的`OnPlay`中写了一个造成单体伤害的指令。

* 想做什么样的卡牌，看原版代码哪张有类似的效果，参考即可。

* 添加一个`Pool`的attribute，并指定要添加的颜色卡池，然后会自动注册。
* 继承`CustomCardModel`而不是`CardModel`。
* <b>注意</b>：通过`baselib`添加卡牌，其id会变成`{命名空间第一段大写}-{原卡牌id}`，例如`namespace Test.Scripts;`取`TEST`，原始卡牌id为`TEST-CARD`，是`TestCard`的大写snake-case，最后变成`TEST-TEST_CARD`。

### 卡图

可以通过在卡牌类中添加一个表达式属性来添加卡牌，这样的话可以任意指定位置：`public override string PortraitPath => $"res://test/images/cards/{Id.Entry.ToLowerInvariant()}.png";`，
如下，那么路径就是`test/images/cards/test-test_card.png`（是你类名的`snake_case`命名风格加前缀，例如`TestCard`即为`test-test_card`）。当然按你的喜好组织资源路径也可。

卡图任意尺寸都可，且不需要裁剪，官方使用的尺寸是普通卡1000x760，先古卡606x852。

```csharp
public class TestCard : TestCardModel
{
    private const int energyCost = 1;
    private const CardType type = CardType.Attack;
    private const CardRarity rarity = CardRarity.Common;
    private const TargetType targetType = TargetType.AnyEnemy;
    private const bool shouldShowInCardLibrary = true;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(12, ValueProp.Move)];

    // 添加这一行，指定卡牌立绘路径
    public override string PortraitPath => $"res://test/images/cards/{Id.Entry.ToLowerInvariant()}.png";

    public TestCard() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }
}
```

![示例卡图](../../images/image10.png)

你也可以通过新增一个`abstract`类，避免每张卡都写一遍卡图路径，并且方便管理一些自定义功能。

```csharp
public abstract class TestCardModel : CardModel
{
    public override string PortraitPath => $"res://test/images/cards/{Id.Entry.ToLowerInvariant()}.png";

    public TestCardModel(int energyCost, CardType type, CardRarity rarity, TargetType targetType, bool shouldShowInCardLibrary) : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }
}

public class TestCard : TestCardModel {}
```

### 文本

此外还需要本地化文件。创建一个`{modId}/localization/{Language}/cards.json`。
* `modId`即为你`{modId}.json`中填写的。<b>不是你的根目录，而是一个新文件夹。</b>
* `Language`可以写`zhs`表示简体中文。填写`{CardId}.title`（卡牌名）和`{CardId}.description`（卡牌描述）：

```json
{
    "TEST-TEST_CARD.title": "测试卡牌",
    "TEST-TEST_CARD.description": "造成{Damage:diff()}点伤害。"
}
```

编译打包`dll`和`pck`后打开游戏。如果你在对应池子中看到卡牌说明成功了。如果没有任何卡牌（或者一张在左上角的卡牌）说明出问题了。

按`~`打开控制台输入`card TEST-TEST_CARD`获得这张卡。

![示例卡牌](../../images/image11.png)

如果报错，回头看看。最终项目结构参考：

```
Test (你的项目文件夹)
├── Scripts (你的脚本文件夹，随意)
│   ├── TestCard.cs
│   └── Entry.cs
└── Test (不要忘了这一层文件夹)
    ├── images
    │   └── cards
    │       └── test-test_card.png
    └── localization
        └── zhs
            └── cards.json
```

## 自定义模组配置

* 要使用此功能，需要先放一张图片到`{modId}\mod_image.png`作为mod图标，尺寸任意，否则会由于报错不显示配置。
* 添加一个继承`SimpleModConfig`（或者是`ModConfig`如果你想要更复杂的设置）的类，在其中添加`public static bool`变量。支持`bool`，`double`，`enum`，`int`，`string`。
* 在初始化函数调用`ModConfigRegistry.Register`。字符串写你的`modId`。

```csharp
[ModInitializer("Init")]
public class Entry
{
    public static void Init()
    {
        ModConfigRegistry.Register("test", new ModConfig());
    }
}

public class ModConfig : SimpleModConfig
{
    public static bool Test1 { get; set; } = true;
    public static bool Test2 { get; set; } = false;
    public static bool Test3 { get; set; } = true;
}
```

![示例配置](../../images/image12.png)

更多请参考`baselib`的`BaseLibConfig`类。

## 添加新遗物

和添加卡牌类似。先新建一个类。

```csharp
// 加入哪个遗物池，此处为通用
[Pool(typeof(SharedRelicPool))]
public class TestRelic : CustomRelicModel
{
    // 稀有度
    public override RelicRarity Rarity => RelicRarity.Common;

    // 遗物的数值。替换本地化中的{Cards}。
    protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(1)];

    // 小图标
    public override string PackedIconPath => $"res://test/images/relics/{Id.Entry.ToLowerInvariant()}.png";
    // 轮廓图标
    protected override string PackedIconOutlinePath => $"res://test/images/relics/{Id.Entry.ToLowerInvariant()}.png";
    // 大图标
    protected override string BigIconPath => $"res://test/images/relics/{Id.Entry.ToLowerInvariant()}.png";

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        // 这里的DynamicVars.Cards.IntValue为上面设置的CardsVar的数值。
        await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.IntValue, player);
    }
}
```

然后放一张图片`test/images/relics/test_relic.png`。路径不一定是`test`，组织风格自定义，参考上面卡图部分。这里偷懒三张图片用了一样的，可以自己修改。

![示例遗物](../../images/image13.png)

然后写一个本地化文件，`{modId}/localization/{Language}/relics.json`。

```json
{
  "TEST-TEST_RELIC.title": "测试遗物",
  "TEST-TEST_RELIC.description": "每回合开始时，抽[blue]{Cards}[/blue]张牌。",
  "TEST-TEST_RELIC.flavor": "觉得很眼熟？"
}
```

## 添加新卡牌关键词

这里的关键词指的是`消耗`，`虚无`一类的卡牌属性，塔2并不需要你在卡牌描述里写这些，只需在`CanonicalKeywords`添加即可。

* 新建一个类：

```csharp
public class MyKeywords
{
    // 自定义枚举的名字。最终会变成{前缀}-{枚举值大写}的形式，例如TEST-UNIQUE
    [CustomEnum("UNIQUE")]
    // 放在原版卡牌描述的位置，这里是卡牌描述的前面
    [KeywordProperties(AutoKeywordPosition.Before)]
    public static CardKeyword Unique;
}
```

* 添加一个本地化文件，`{modId}/localization/{Language}/card_keywords.json`。

```json
{
    "TEST-UNIQUE.description": "卡组中只能有一张同名牌。",
    "TEST-UNIQUE.title": "唯一"
}
```

* 然后在你的卡牌类里添加这一行，或者添加keyword：

```csharp
    public override IEnumerable<CardKeyword> CanonicalKeywords => [MyKeywords.Unique];
```

![alt text](../../images/image23.png)

## 添加动态变量

动态变量是指`伤害`，`格挡`，`抽牌数`，`获得能量数`等这种动态数值。虽然可以通过`new DynamicPower("xxx", 1)`这种形式添加，但是写一个新的类比较规范也便于扩展功能。

通过`baselib`的`WithTooltip`可以添加tooltip。

先创建新的类：
```csharp
using BaseLib.Extensions;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace Test.Scripts;

public class TestDynamicVar : DynamicVar
{
    // 在描述中用作占位符的键，推荐添加前缀避免撞车
    public const string Key = "Test-Leech";
    // 本地化键，这里设置为大写的Key，也就是"TEST-LEECH"
    public static readonly string LocKey = Key.ToUpperInvariant();

    public TestDynamicVar(decimal baseValue) : base(Key, baseValue)
    {
        this.WithTooltip(LocKey);
    }
}
```

然后添加一个新的本地化文件`{modId}/localization/{Language}/static_hover_tips.json`。

```json
{
    "TEST-LEECH.description": "吸取等量生命。",
    "TEST-LEECH.title": "汲取"
}
```

如果要使用这个变量，在`CanonicalVars`中添加你新建的变量即可。

```csharp
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(12, ValueProp.Move),
        new TestDynamicVar(3)
    ];
```

然后修改卡牌的描述以使用：

```json
{
    "TEST-TEST_CARD.title": "测试卡牌",
    "TEST-TEST_CARD.description": "[gold]汲取[/gold]{Test-Leech:diff()}。\n造成{Damage:diff()}点伤害。"
}
```

`:diff()`表示这个值一旦和基础值不同，就会变红色或绿色（例如升级时增加数值，预览变成绿色）。


![alt text](../../images/image26.png)

当然如果你只是个简单的数值，这样就行：

```csharp
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(12, ValueProp.Move),
        new DynamicVar("Test-Leech", 1m).WithTooltip("TEST-LEECH")
    ];
```


## 添加局内保存

在卡牌、遗物、附魔、Modifier（每日挑战效果）的`Model`的属性中中添加带`SavedProperty`的属性即可保存。

```csharp
[Pool(typeof(SharedRelicPool))]
public class TestRelic : CustomRelicModel
{
    // 这个属性会被保存。建议添加前缀id防止撞车。
    // 设置不同的SerializationCondition来控制属性的保存条件，例如这里使用默认值AlwaysSave表示无论属性值是什么都保存。
    [SavedProperty]
    public int Test_GameTurns { get; set; } = 0;

    // 添加新的动态变量
    protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(1), new DynamicVar("GameTurns", Test_GameTurns)];

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        // 每回合开始时，修改Test_GameTurns的值，并改变卡牌描述中{GameTurns}的值为Test_GameTurns的值
        Test_GameTurns++;
        DynamicVars["GameTurns"].BaseValue = Test_GameTurns;
        await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.IntValue, player);
    }
}
```

```json
{
  "TEST-TEST_RELIC.title": "测试遗物",
  "TEST-TEST_RELIC.description": "每回合开始时，抽[blue]{Cards}[/blue]张牌。\n已经历过[blue]{GameTurns}[/blue]回合了。",
  "TEST-TEST_RELIC.flavor": "觉得很眼熟？"
}
```

## 添加新能力

新建类：

```csharp
public class TestPower : CustomPowerModel
{
    // 类型，Buff或Debuff
    public override PowerType Type => PowerType.Buff;
    // 叠加类型，Counter表示可叠加，Single表示不可叠加
    public override PowerStackType StackType => PowerStackType.Counter;

    // 自定义图标路径，自己指定，或者创建一个基类来统一指定图标路径
    public override string? CustomPackedIconPath => "res://test/powers/test_power.png";
    public override string? CustomBigIconPath => "res://test/powers/test_power.png";

    // 抽牌后给予玩家力量
    public override async Task AfterCardDrawn(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
    {
        await PowerCmd.Apply<StrengthPower>(Owner, Amount, Owner, null);
    }
}
```

添加json，`{modId}/localization/{Language}/powers.json`。
```json
{
    "TEST-TEST_POWER.description": "每次抽牌时，获得一点[gold]力量[/gold]。",
    "TEST-TEST_POWER.smartDescription": "每次抽牌时，获得[blue]{Amount}[/blue]点[gold]力量[/gold]。", // smartDescription可以使用{Amount}来显示当前的数值
    "TEST-TEST_POWER.title": "邪火"
}
```

然后使用`PowerCmd.Apply<TestPower>(...)`给予即可。或者使用控制台`power TEST-TEST_POWER 1 0`。

![alt text](../../images/image25.png)

## 添加新角色

添加新人物过于麻烦了，于是单开一章。

## 依赖BaseLib

参考上一章添加对`baselib`的依赖。这可以省去你很多功夫。

## 创建池子

需要创建人物独有的卡牌、药水、遗物池各一个。

`TestCardPool.cs`:
```csharp
public class TestCardPool : CustomCardPoolModel
{
    // 卡池的ID。必须唯一防撞车。
    public override string Title => "test";

    // 描述中使用的能量图标。大小为24x24。
    public override string? TextEnergyIconPath => "res://test/images/energy_test.png";
    // tooltip和卡牌左上角的能量图标。大小为74x74。
    public override string? BigEnergyIconPath => "res://test/images/energy_test_big.png";

    // 卡池的主题色。
    public override Color DeckEntryCardColor => new(0.5f, 0.5f, 1f);

    // 卡池是否是无色。例如事件、状态等卡池就是无色的。
    public override bool IsColorless => false;
}
```

`TestRelicPool.cs`:

```csharp
public class TestRelicPool : CustomRelicPoolModel
{
    // 描述中使用的能量图标。大小为24x24。
    public override string? TextEnergyIconPath => "res://test/images/energy_test.png";
    // tooltip和卡牌左上角的能量图标。大小为74x74。
    public override string? BigEnergyIconPath => "res://test/images/energy_test_big.png";
}
```

`TestPotionPool.cs`:

```csharp
public class TestPotionPool : CustomPotionPoolModel
{
    // 描述中使用的能量图标。大小为24x24。
    public override string? TextEnergyIconPath => "res://test/images/energy_test.png";
    // tooltip和卡牌左上角的能量图标。大小为74x74。
    public override string? BigEnergyIconPath => "res://test/images/energy_test_big.png";
}
```

## 创建人物

人物需要极其大量的资源，推荐新建类继承`PlaceholderCharacterModel`而不是`CustomCharacterModel`。你没有的资源直接注释掉以使用原版。教程提供的资源在最下方。

```csharp
public class TestCharacter : PlaceholderCharacterModel
{
    // 角色名称颜色
    public override Color NameColor => new(0.5f, 0.5f, 1f);
    // 能量图标轮廓颜色
    public override Color EnergyLabelOutlineColor => new(0.1f, 0.1f, 1f);

    // 人物性别（男女中立）
    public override CharacterGender Gender => CharacterGender.Masculine;

    // 初始血量
    public override int StartingHp => 80;

    // 人物模型tscn路径。要自定义见下。
    public override string CustomVisualPath => "res://test/scenes/test_character.tscn";
    // 卡牌拖尾场景。
    // public override string CustomTrailPath => "res://scenes/vfx/card_trail_ironclad.tscn";
    // 人物头像路径。
    public override string CustomIconTexturePath => "res://icon.svg";
    // 人物头像2号。
    // public override string CustomIconPath => "res://scenes/ui/character_icons/ironclad_icon.tscn";
    // 能量表盘tscn路径。要自定义见下。
    public override string CustomEnergyCounterPath => "res://test/scenes/test_energy_counter.tscn";
    // 篝火休息场景。
    // public override string CustomRestSiteAnimPath => "res://scenes/rest_site/characters/ironclad_rest_site.tscn";
    // 商店人物场景。
    // public override string CustomMerchantAnimPath => "res://scenes/merchant/characters/ironclad_merchant.tscn";
    // 多人模式-手指。
    // public override string CustomArmPointingTexturePath => null;
    // 多人模式剪刀石头布-石头。
    // public override string CustomArmRockTexturePath => null;
    // 多人模式剪刀石头布-布。
    // public override string CustomArmPaperTexturePath => null;
    // 多人模式剪刀石头布-剪刀。
    // public override string CustomArmScissorsTexturePath => null;

    // 人物选择背景。
    public override string CustomCharacterSelectBg => "res://test/scenes/test_bg.tscn";
    // 人物选择图标。
    public override string CustomCharacterSelectIconPath => "res://test/images/char_select_test.png";
    // 人物选择图标-锁定状态。
    public override string CustomCharacterSelectLockedIconPath => "res://test/images/char_select_test_locked.png";
    // 人物选择过渡动画。
    // public override string CustomCharacterSelectTransitionPath => "res://materials/transitions/ironclad_transition_mat.tres";
    // 地图上的角色标记图标、表情轮盘上的角色头像
    // public override string CustomMapMarkerPath => null;
    // 攻击音效
    // public override string CustomAttackSfx => null;
    // 施法音效
    // public override string CustomCastSfx => null;
    // 死亡音效
    // public override string CustomDeathSfx => null;
    // 角色选择音效
    // public override string CharacterSelectSfx => null;
    // 过渡音效。这个不能删。
    public override string CharacterTransitionSfx => "event:/sfx/ui/wipe_ironclad";

    public override CardPoolModel CardPool => ModelDb.CardPool<TestCardPool>();
    public override RelicPoolModel RelicPool => ModelDb.RelicPool<TestRelicPool>();
    public override PotionPoolModel PotionPool => ModelDb.PotionPool<TestPotionPool>();

    // 初始卡组
    public override IEnumerable<CardModel> StartingDeck => [
        ModelDb.Card<TestCard>(),
        ModelDb.Card<TestCard>(),
        ModelDb.Card<TestCard>(),
        ModelDb.Card<TestCard>(),
        ModelDb.Card<TestCard>(),
    ];

    // 初始遗物
    public override IReadOnlyList<RelicModel> StartingRelics => [
        ModelDb.Relic<TestRelic>(),
    ];

    // 攻击建筑师的攻击特效列表
    public override List<string> GetArchitectAttackVfx() => [
        "vfx/vfx_attack_blunt",
        "vfx/vfx_heavy_blunt",
        "vfx/vfx_attack_slash",
        "vfx/vfx_bloody_impact",
        "vfx/vfx_rock_shatter"
    ];
}
```

## 自定义人物背景

`public override string CustomCharacterSelectBg => "res://test/scenes/test_bg.tscn";`

没什么要求，Godot里创建一个新的场景，类型为`Control`，自己搭建场景即可。参考：

![人物背景](../../images/image17.png)

## 自定义人物

`public override string CustomVisualPath => "res://test/scenes/test_character.tscn";`

新建一个`Node2D`类型的场景，如下：

```
TestCharacter (Node2D)
├── Visuals (Node2D) %
├── Bounds (Control) %
├── IntentPos (Marker2D) %
└── CenterPos (Marker2D) %
```

<b>其中`Visuals`，`Bounds`，`IntentPos`，`CenterPos`需要右键勾选`作为唯一名称访问`，出现`%`即可。名字不要改。</b>

~~创建一个`NTestCharacter.cs`，继承`CreatureVisuals`。然后把它挂载到`TestCharacter`节点上。~~现在不需要了。

`Bounds`就是你的人物hitbox的大小，如果你觉得血条太短调整一下它的大小。

* 人物显示在x轴上方。
* 如果想使用3d模型，新建`visuals→subviewportcontainer→subviewport`的层级结构，然后在`subviewport`中添加`camera3d`和任意3d模型，在3d视图中调整视角至2d视图正常显示。最后设置`subviewport`的`transparent`为`true`。

![alt text](../../images/image18.png)

### 人物动画

* 其中`Visuals`可以更改成任意继承了`Node2D`的类型，例如`SpineSprite`，`Sprite2D`或是`AnimatedSprite2D`，或者在它之下新建节点都可。

* 如果要自然支持Spine播放，需要把`Visuals`改成`SpineSprite`，且你的战斗人物模型需要有`idle_loop`（待机循环），`attack`（攻击动作），`cast`（能力卡动作），`hurt`（受伤），`die`（死亡）这些动画名。（如果你没有`SpineSprite`，参考`卡图&皮肤替换`一章先下载`Spine Godot Extension`。）

* 如果你只有一张图，那么把`Visuals`改成`Sprite2D`类型更改图片即可。

* 此外`baselib`支持使用`AnimationPlayer`控制动画，例如你使用`AnimatedSprite2D`或者是3D模型。虽然`AnimationPlayer`放在任意位置都可以，但推荐把根节点之下。动画名和上方设置的一致即可自动播放动画。

* 例如：如果是使用`AnimatedSprite2D`，设置好`临近FPS`（例如0.2秒），然后前往`Visuals`节点点击属性`Frame`右侧的钥匙插入关键帧，重复修改当前帧和插入关键帧即可。参考：https://docs.godotengine.org/en/stable/tutorials/2d/2d_sprite_animation.html#sprite-sheet-with-animationplayer

## 自定义能量表盘

`public override string CustomEnergyCounterPath => "res://test/scenes/test_energy_counter.tscn";`

* 建议从原版或者下面的附赠资源处复制一份tscn快速开始。
创建一个`Control`类型的新场景，设定以下结构：

```
TestEnergyCounter (Control)
├── EnergyVfxBack (Node2D) %
├── Layers (Control) %
│   ├── Layer1 (TextureRect，或任意)
│   └── RotationLayers (Control) %
├── EnergyVfxFront (Node2D) %
└── Label (Label)
```

* 后面标`%`的需要作为唯一名称访问。名字不要改，label也是。
* RotationLayers里放需要旋转的图层。没有也行。

![alt text](../../images/image19.png)

然后创建一个继承`NEnergyCounter`的类，挂载到父节点上。

```csharp
public partial class NTestEnergyCounter : NEnergyCounter
{
}
```

然后创建一个继承`NParticlesContainer`的类，挂载到`EnergyVfxBack`和`EnergyVfxFront`上。

其中的`_particles`无法使用`Export`设置，所以需要反射设置。当然你也可以往里添加`GpuParticles2D`以添加粒子动画效果。具体参考原版。

```csharp
public partial class NTestParticlesContainer : NParticlesContainer
{
    public override void _Ready()
    {
        base._Ready();
        Traverse.Create(this).Field("_particles").SetValue(new Array<GpuParticles2D>());
    }
}
```

然后创建一个继承`MegaLabel`的类，挂载到`Label`上。

```csharp
public partial class TestMegaLabel : MegaLabel
{
}
```

保存一下然后关闭这个场景，然后开始<b>神秘操作</b>。在本地，或者是你的ide里打开这个tscn文件，先修改开头，
* 在`ext_resource`这一组下添加`kreon_bold_shared_font`这一行。
* 在`ext_resource`这一组下添加`FontVariation_kreon_bold_shared_font`，`base_font`，`spacing_glyph`这三行。
* 修改`load_steps`，改为原来的数字+2。（ext数量+sub数量+1）

```tscn
[gd_scene load_steps=7 format=3 uid="uid://cs3a5onikvhi4"]

[ext_resource type="Texture2D" path="res://icon.svg" id="1_85qf2"]
[ext_resource type="Script" path="res://Scripts/NTestEnergyCounter.cs" id="1_tmfxn"]
[ext_resource type="Script" path="res://Scripts/NTestParticlesContainer.cs" id="2_1ytbd"]
[ext_resource type="Script" path="res://Scripts/TestMegaLabel.cs" id="4_6cpd0"]

[ext_resource type="FontVariation" path="res://themes/kreon_bold_shared.tres" id="kreon_bold_shared_font"]

[sub_resource type="FontVariation" id="FontVariation_kreon_bold_shared_font"]
base_font = ExtResource("kreon_bold_shared_font")
spacing_glyph = 2
```

* 然后往下，找到`[node name="Label" type="Label" parent="."]`这一行，添加以下这一行。

```tscn
theme_override_fonts/font = SubResource("FontVariation_kreon_bold_shared_font")
```

* 然后如果你想修改这个能量表盘，打开场景后需要重复以上工作。
* 或者你也可以把反编译的字体资源复制到本地以省去以上工作。

## 本地化文件

创建`{modId}/localization/{Language}/characters.json`，填写以下内容：

```json
{
  // 混沌香气事件中的内心独白
  "TEST-TEST_CHARACTER.aromaPrinciple": "[sine][blue]……等待……[/blue][/sine]",
  // 多人模式：存活时回合结束对白
  "TEST-TEST_CHARACTER.banter.alive.endTurnPing": "……",
  // 多人模式：死亡后回合结束对白
  "TEST-TEST_CHARACTER.banter.dead.endTurnPing": "……",
  // 自定义模式文本
  "TEST-TEST_CHARACTER.cardsModifierDescription": "戈多的卡牌现在会出现在奖励和商店中。",
  // 卡池名字
  "TEST-TEST_CHARACTER.cardsModifierTitle": "戈多卡牌",
  // 角色选择界面描述
  "TEST-TEST_CHARACTER.description": "一个在无尽等待中的存在。\n时间对[gold]戈多[/gold]而言，不过是另一种形式的永恒。",
  // 死亡时事件对话
  "TEST-TEST_CHARACTER.eventDeathPrevention": "我还得继续等下去……",
  // 沉没宝库事件中对金币的独白
  "TEST-TEST_CHARACTER.goldMonologue": "[sine]这些金币……也许能派上用场……[/sine]",
  // 物主形容词，用于动态文本
  "TEST-TEST_CHARACTER.possessiveAdjective": "他的",
  // 宾语代词
  "TEST-TEST_CHARACTER.pronounObject": "他",
  // 所有格代词
  "TEST-TEST_CHARACTER.pronounPossessive": "他的",
  // 主语代词
  "TEST-TEST_CHARACTER.pronounSubject": "他",
  // 角色名（标题用）
  "TEST-TEST_CHARACTER.title": "戈多",
  // 角色名（作宾语用）
  "TEST-TEST_CHARACTER.titleObject": "戈多",
  // 解锁条件文案，{Prerequisite} 会被替换
  "TEST-TEST_CHARACTER.unlockText": "用[pink]{Prerequisite}[/pink]进行一局游戏来解锁这个角色。"
}
```

不要忘记在你的`Init`初始化函数中添加`ScriptManagerBridge.LookupScriptsInAssembly(typeof(Entry).Assembly);`这一行。

* 打开`项目→项目设置`，把`将文本资源转换为二进制`禁用。

![3](../../images/image16.png)

![alt text](../../images/image20.png)

## 附赠资源

![alt text](../../images/image21.png)

![alt text](../../images/image22.png)

![alt text](../../images/energy_test.png)

![alt text](../../images/energy_test_big.png)

`test_bg.tscn`:
```tscn
[gd_scene load_steps=2 format=3 uid="uid://cejqjeipgqe0n"]

[ext_resource type="Texture2D" uid="uid://ddxmxgyyfy8mn" path="res://icon.svg" id="1_c8lhi"]

[node name="TestBg" type="Control"]
layout_mode = 3
anchors_preset = 8
anchor_left = 0.5
anchor_top = 0.5
anchor_right = 0.5
anchor_bottom = 0.5
offset_left = -1790.0
offset_top = -1043.0
offset_right = 1790.0
offset_bottom = 1043.0
grow_horizontal = 2
grow_vertical = 2
metadata/_edit_lock_ = true

[node name="ColorRect" type="ColorRect" parent="."]
layout_mode = 1
anchors_preset = 15
anchor_right = 1.0
anchor_bottom = 1.0
grow_horizontal = 2
grow_vertical = 2
color = Color(0.44705883, 0.49803922, 1, 1)
metadata/_edit_lock_ = true

[node name="Icon" type="TextureRect" parent="."]
layout_mode = 1
anchors_preset = -1
anchor_right = 1.0
anchor_bottom = 1.0
offset_left = 1774.0
offset_top = 792.0
offset_right = -1259.0
offset_bottom = -747.0
grow_horizontal = 2
grow_vertical = 2
texture = ExtResource("1_c8lhi")

[node name="ash1" type="CPUParticles2D" parent="."]
position = Vector2(1832, -17)
amount = 40
lifetime = 15.0
preprocess = 15.0
local_coords = true
emission_shape = 3
emission_rect_extents = Vector2(2000, 1)
gravity = Vector2(27, 27)
orbit_velocity_max = 0.03
angle_min = 45.0
angle_max = 90.0
scale_amount_min = 10.0
scale_amount_max = 10.0

[node name="ash2" type="CPUParticles2D" parent="."]
position = Vector2(1832, -17)
amount = 40
lifetime = 15.0
preprocess = 15.0
local_coords = true
emission_shape = 3
emission_rect_extents = Vector2(2000, 1)
gravity = Vector2(27, 27)
orbit_velocity_max = 0.03
angle_min = 45.0
angle_max = 90.0
scale_amount_min = 10.0
scale_amount_max = 10.0
color = Color(0.121879734, 0.15283081, 0.33476263, 1)
```

`test_character.tscn`:
```tscn
[gd_scene load_steps=3 format=3 uid="uid://c4dnpxxd6ldei"]

[ext_resource type="Script" uid="uid://6m0cydgurd52" path="res://Scripts/NTestCharacter.cs" id="creature_visuals"]
[ext_resource type="Texture2D" uid="uid://ddxmxgyyfy8mn" path="res://icon.svg" id="1_hxav6"]

[node name="TestCharacter" type="Node2D"]
script = ExtResource("creature_visuals")

[node name="Visuals" type="Sprite2D" parent="."]
unique_name_in_owner = true
position = Vector2(0, -73)
texture = ExtResource("1_hxav6")

[node name="Bounds" type="Control" parent="."]
unique_name_in_owner = true
layout_mode = 3
anchors_preset = 0
offset_left = -70.0
offset_top = -140.0
offset_right = 70.0

[node name="IntentPos" type="Marker2D" parent="."]
unique_name_in_owner = true
position = Vector2(0, -159)

[node name="CenterPos" type="Marker2D" parent="."]
unique_name_in_owner = true
position = Vector2(0, -72)
```

`test_energy_counter.tscn`:

```tscn
[gd_scene load_steps=7 format=3 uid="uid://cs3a5onikvhi4"]

[ext_resource type="Texture2D" uid="uid://ddxmxgyyfy8mn" path="res://icon.svg" id="1_85qf2"]
[ext_resource type="Script" uid="uid://b4eaf7kin174o" path="res://Scripts/NTestEnergyCounter.cs" id="1_tmfxn"]
[ext_resource type="Script" uid="uid://b8vmh6070x38m" path="res://Scripts/NTestParticlesContainer.cs" id="2_1ytbd"]
[ext_resource type="Script" uid="uid://camgj4bhk5dps" path="res://Scripts/TestMegaLabel.cs" id="4_6cpd0"]

[ext_resource type="FontVariation" path="res://themes/kreon_bold_shared.tres" id="kreon_bold_shared_font"]

[sub_resource type="FontVariation" id="FontVariation_kreon_bold_shared_font"]
spacing_glyph = 2

[node name="TestEnergyCounter" type="Control"]
layout_mode = 3
anchors_preset = 0
offset_right = 128.0
offset_bottom = 128.0
script = ExtResource("1_tmfxn")
metadata/_edit_lock_ = true

[node name="EnergyVfxBack" type="Node2D" parent="."]
unique_name_in_owner = true
position = Vector2(64, 64)
script = ExtResource("2_1ytbd")

[node name="Layers" type="Control" parent="."]
unique_name_in_owner = true
layout_mode = 1
anchors_preset = 15
anchor_right = 1.0
anchor_bottom = 1.0
grow_horizontal = 2
grow_vertical = 2

[node name="RotationLayers" type="Control" parent="Layers"]
unique_name_in_owner = true
anchors_preset = 0
offset_right = 40.0
offset_bottom = 40.0

[node name="Layer1" type="TextureRect" parent="Layers"]
layout_mode = 1
anchors_preset = 15
anchor_right = 1.0
anchor_bottom = 1.0
grow_horizontal = 2
grow_vertical = 2
texture = ExtResource("1_85qf2")
expand_mode = 1

[node name="EnergyVfxFront" type="Node2D" parent="."]
unique_name_in_owner = true
position = Vector2(64, 64)
script = ExtResource("2_1ytbd")

[node name="Label" type="Label" parent="."]
layout_mode = 1
anchors_preset = 15
anchor_right = 1.0
anchor_bottom = 1.0
offset_left = 16.0
offset_top = -29.0
offset_right = -16.0
offset_bottom = 29.0
grow_horizontal = 2
grow_vertical = 2
theme_override_colors/font_color = Color(1, 0.9647059, 0.8862745, 1)
theme_override_colors/font_shadow_color = Color(0, 0, 0, 0.1882353)
theme_override_colors/font_outline_color = Color(0.1, 0.1, 0.8, 1)
theme_override_constants/shadow_offset_x = 3
theme_override_constants/shadow_offset_y = 2
theme_override_constants/outline_size = 16
theme_override_constants/shadow_outline_size = 16
theme_override_fonts/font = SubResource("FontVariation_kreon_bold_shared_font")
theme_override_font_sizes/font_size = 36
text = "3/3"
horizontal_alignment = 1
vertical_alignment = 1
script = ExtResource("4_6cpd0")
```

## 变量与描述

这里介绍如何编写塔2的卡牌、遗物、药水、能力等的描述。

## Godot原生

由于是`RichTextLabel`，Godot原生的bbcode都可以使用，参考 https://docs.godotengine.org/zh-cn/4.x/tutorials/ui/bbcode_in_richtextlabel.html 。

速览：

| BBCode | 说明 | 示例 |
|-----------|------|------|
| `[b]...[/b]` | 粗体 | `[b]bold[/b]` |
| `[i]...[/i]` | 斜体 | `[i]italic[/i]` |
| `[u]...[/u]` | 下划线 | `[u]underline[/u]` |
| `[color=...]...[/color]` | 文字颜色 | `[color=red]red text[/color]` |
| `[font=...]...[/font]` | 字体 | `[font=Arial]Arial text[/font]` |
| `[size=...]...[/size]` | 字号 | `[size=24]large text[/size]` |

## 游戏自定义tag

| 标签名 | 作用 |
| - | - |
| `[ancient_banner]...[/ancient_banner]` | 古代横幅风格 |
| `[aqua]...[/aqua]` | 水绿色文字 |
| `[blue]...[/blue]` | 蓝色文字 |
| `[fade_in]...[/fade_in]` | 渐显动画效果 |
| `[fly_in]...[/fly_in]` | 飞入动画效果 |
| `[gold]...[/gold]` | 金色文字 |
| `[green]...[/green]` | 绿色文字 |
| `[jitter]...[/jitter]` | 抖动动画效果 |
| `[orange]...[/orange]` | 橙色文字 |
| `[pink]...[/pink]` | 粉色文字 |
| `[purple]...[/purple]` | 紫色文字 |
| `[red]...[/red]` | 红色文字 |
| `[sine]...[/sine]` | 正弦波动动画效果 |
| `[thinky_dots]...[/thinky_dots]` | 思考点点动画效果 |

## 占位变量

会被model中的dynamicvars中的对应数值替换。

| 名称 | 说明 | 示例 |
|-----------|------|------|
| `{Damage}` | 伤害 | `造成{Damage:diff()}点伤害。` |
| `{Block}` | 格挡 | `获得{Block:diff()}点格挡。` |
| `{Cards}` | 卡牌数量 | `抽{Cards:diff()}张牌。` |
| `{Energy}` | 能量（动态值） | `获得{Energy:energyIcons()}。` |
| `{energyPrefix}` | 能量（固定数值） | `获得{energyPrefix:energyIcons(1)}。` |
| `{Repeat}` | 重复次数 | `造成{Damage:diff()}点伤害{Repeat:diff()}次。` |
| `{Heal}` | 治疗 | `回复{Heal:diff()}点生命。` |
| `{HpLoss}` | 失去生命 | `失去{HpLoss:Diff()}点生命。` |
| `{MaxHp}` | 最大生命 | `获得{MaxHp:diff()}点最大生命。` |
| `{Gold}` | 金币 | `获得{Gold:diff()}金币。` |
| `{Summon}` | 召唤 | `召唤{Summon:diff()}。` |
| `{Forge}` | 铸造 | `铸造{Forge:diff()}。` |
| `{Stars}` | 辉星 | `获得{Stars:starIcons()}。` |
| `{StrengthPower}` | 力量 | `获得{StrengthPower:diff()}点力量。` |
| `{DexterityPower}` | 敏捷 | `获得{DexterityPower:diff()}点敏捷。` |
| `{WeakPower}` | 虚弱 | `给予{WeakPower:diff()}层虚弱。` |
| `{VulnerablePower}` | 易伤 | `给予{VulnerablePower:diff()}层易伤。` |
| `{PoisonPower}` | 中毒 | `给予{PoisonPower:diff()}层中毒。` |
| `{DoomPower}` | 灾厄 | `给予{DoomPower:diff()}层灾厄。` |
| `{CalculatedDamage}` | 计算出的伤害量 | `（造成{CalculatedDamage:diff()}点伤害）` |
| `{CalculatedBlock}` | 计算出的格挡值 | `（获得{CalculatedBlock:diff()}点格挡）` |

## formatter

用于格式化一个变量的表现形式，使用`SmartFormat`库。

例如`{Energy:energyIcons()}`表示展示n个能量图标，n为`Energy`的数值。具体逻辑查看对应的formatter类。

游戏自定义的formatter：

| 名称 | 说明 | 示例 |
|-----------|------|------|
| `diff()` | 高于基础变绿，低于基础变红。用于战斗或者升级预览 | `造成{Damage:diff()}点伤害。` |
| `inverseDiff()` | 高于基础变红，低于基础变绿 | `失去{HpLoss:inverseDiff()}点生命。` |
| `energyIcons()` | 把数值渲染成能量 | `获得{Energy:energyIcons()}。` |
| `starIcons()` | 把数值渲染为辉星 | `获得{Stars:starIcons()}。` |
| `IfUpgraded:show` | 根据升级情况显示不同文本 | `{IfUpgraded:show:升级文本\|未升级文本}` |
| `abs` | 绝对值 | `{Damage:abs()}` |
| `percentMore()` / `percentLess()` | 百分比 | `额外造成{Boost:percentMore()}伤害。` |

`SmartFormat`的内置formatter：

https://github.com/axuno/SmartFormat/wiki

| 名称 | 说明 | 示例 |
|-----------|------|------|
| `cond` | 条件分支，例如`{X:cond:>0?生效\|不生效}` | `{FanOfKnivesAmount:cond:>0? 对所有敌人\|}造成{Damage:diff()}点伤害。` |
| `choose` | 按索引或值选择分支，例如`{X:choose(1\|2\|3):one\|two\|three\|other}`，X为1、2、3时分别为对应值，为其他值时为other | `你打出的下{Skills:choose(1):一\|{:diff()}}张技能牌会被额外打出一次。` |
| `plural` | 复数 | 英语环境下，`Draw {Cards:diff()} {Cards:plural:card\|cards}.` |
| `list` | 拼接 | https://github.com/axuno/SmartFormat/wiki/v2-Lists |

其他的参考wiki。

## 卡牌独有

卡牌有一些额外的上下文变量，例如：

| 名称 | 含义 | 典型写法 |
| - | - | - |
| `singleStarIcon` | 星星图标 | `每获得{singleStarIcon}时` |
| `InCombat` | 是否处于战斗 | `{InCombat:\n（命中{CalculatedHits:diff()}次）\|}` |
| `IsTargeting` | 当前是否有目标 | `{IsTargeting:\n（造成{CalculatedDamage:diff()}）\|}` |
| `OnTable` | 牌是否在手牌或出牌区 | `{OnTable:cond:true?在场上\|不在场上}` |
| `IfUpgraded` | 是否升级 | `[gold]升级[/gold]你[gold]手牌[/gold]中的{IfUpgraded:show:所有牌\|一张牌}。` |


