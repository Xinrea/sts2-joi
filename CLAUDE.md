# Joi Mod Development Guide

## Card Registration Issues and Solutions

### Problem 1: Cards Not Appearing in Game
**Issue**: New cards were implemented but didn't appear in the character's card pool.

**Root Cause**: Missing `[Pool(typeof(JoiCardPool))]` attribute on individual card classes.

**Solution**:
- Even though `JoiCard` base class has the Pool attribute, each card class MUST have its own `[Pool(typeof(JoiCardPool))]` attribute
- The attribute inheritance doesn't work as expected in this context
- Always add the Pool attribute to every card class individually

```csharp
[Pool(typeof(JoiCardPool))]
public class YourCard : JoiCard
{
    // card implementation
}
```

### Problem 2: Missing Localizations
**Issue**: Cards compiled but showed missing localization errors.

**Root Cause**: Only Chinese (zhs) localizations were added, English (eng) localizations were missing.

**Solution**:
- Always add localizations for BOTH languages:
  - `Joi/localization/eng/cards.json` - English card text
  - `Joi/localization/zhs/cards.json` - Chinese card text
  - `Joi/localization/eng/powers.json` - English power text
  - `Joi/localization/zhs/powers.json` - Chinese power text

### Problem 3: Card Images Not Loading
**Issue**: Card images were created but didn't load in-game, showing placeholder graphics instead.

**Root Cause**: Image filenames used camelCase instead of snake_case with underscores.

**Solution**:
- Card image filenames MUST use snake_case with underscores to match the game's naming convention
- The `JoiCard` base class converts class names to lowercase with underscores: `Id.Entry.RemovePrefix().ToLowerInvariant()`
- Examples:
  - `PhotonJet` class → `photon_jet.png` (NOT `photonjet.png`)
  - `BinaryStarCollision` class → `binary_star_collision.png` (NOT `binarystarcollision.png`)
  - `DarkMatterImpact` class → `dark_matter_impact.png` (NOT `darkmatterimpact.png`)
- Create SVG files for card art
- Convert to PNG using: `sips -s format png input.svg --out output.png`
- Place in `Joi/images/card_portraits/` with correct snake_case filename

## Correct Workflow for Adding New Cards

1. **Create Card Class**
   - Inherit from `JoiCard`
   - Add `[Pool(typeof(JoiCardPool))]` attribute
   - Implement constructor, CanonicalVars, OnPlay, and OnUpgrade

2. **Add Localizations**
   - Add entries to both `eng/cards.json` and `zhs/cards.json`
   - Format: `JOI-CARD_NAME.title` and `JOI-CARD_NAME.description`

3. **Create Card Images**
   - Generate or create card portrait
   - Save as PNG in `Joi/images/card_portraits/`
   - **IMPORTANT**: Use snake_case with underscores (e.g., `photon_jet.png`, NOT `photonjet.png`)
   - Filename must match: ClassName → snake_case (PhotonJet → photon_jet.png)

4. **Add Power Localizations (if needed)**
   - Add to both `eng/powers.json` and `zhs/powers.json`
   - Include title, description, and smartDescription

5. **Compile and Test**
   - Run `dotnet publish -c ExportRelease`
   - Check for errors
   - Test in-game

## Common Patterns

### Card with Dynamic Variables
```csharp
protected override IEnumerable<DynamicVar> CanonicalVars =>
[
    new DamageVar(8, ValueProp.Move),
    new DynamicVar("BlackHole", 1)
];
```

### Card Upgrade
```csharp
protected override void OnUpgrade()
{
    DynamicVars.Damage.UpgradeValueBy(3);
    // or
    DynamicVars["BlackHole"].UpgradeValueBy(1);
}
```

### Applying Powers
```csharp
await CommonActions.ApplySelf<BlackHolePower>(this, amount);
```

### Adding Keyword Tooltips
For custom powers that need hover tooltips:

1. **Add tooltip text** to `Joi/localization/eng/static_hover_tips.json` and `Joi/localization/zhs/static_hover_tips.json`:
```json
{
  "JOI-SLEEP.title": "Sleep",
  "JOI-SLEEP.description": "Cannot act. Loses 1 stack at the end of turn."
}
```

2. **Register tooltip in JoiCard.cs** in the `GetJoiTermHoverTips()` method:
```csharp
"SLEEPY_RADIO" => [CreateStaticHoverTip("JOI-SLEEP")],
```

3. **Use in card description** with `[gold]` tags:
```json
"JOI-SLEEPY_RADIO.description": "Apply {SleepPower:diff()} [gold]Sleep[/gold] to ALL enemies."
```

### Power Triggers with Harmony Patches
For powers that trigger on events (like gaining Black Hole), use Harmony patches:

```csharp
[HarmonyPatch(typeof(PowerCmd), nameof(PowerCmd.Apply))]
public static class YourPatch
{
    [HarmonyPostfix]
    static void ApplyPostfix(ref Task __result, PowerModel power, Creature target, decimal amount)
    {
        __result = TriggerYourPower(__result, power, target, amount);
    }
}
```

## Using ILSpy MCP for Reverse Engineering

### Problem: Understanding Game Mechanics
**Issue**: Need to understand how game's built-in powers work to implement custom powers correctly.

**Solution**: Use ILSpy MCP tool to decompile and analyze game assemblies.

### Setup ILSpy MCP
1. Add MCP server configuration:
```bash
claude mcp add -s project ilspy -- dotnet tools/ILSpy-Mcp/bin/Release/net8.0/ILSpy.Mcp.dll
```

2. Restart AWS Code to load the MCP server

### Common ILSpy Commands
- `mcp__ilspy__list_assembly_types` - List all types in an assembly
- `mcp__ilspy__decompile_type` - Decompile a specific class
- `mcp__ilspy__search_members_by_name` - Search for methods/properties by name
- `mcp__ilspy__decompile_method` - Decompile a specific method

### Example: Implementing Sleep Power
**Problem**: Sleep power not preventing enemy actions.

**Investigation Process**:
1. Search for similar powers: `AsleepPower`, `SlumberPower`
2. Decompile to understand implementation
3. Found key method: `CreatureCmd.Stun()`

**Solution**:
```csharp
public override async Task AfterSideTurnStart(CombatSide side, CombatState combatState)
{
    if (side == Owner.Side && Amount > 0)
    {
        await CreatureCmd.Stun(Owner);
    }
}
```

## Key Takeaways

1. **Always add Pool attribute to each card class** - Don't rely on inheritance
2. **Add both English and Chinese localizations** - Missing either will cause errors
3. **Use snake_case for card image filenames** - Must use underscores (e.g., `photon_jet.png`, NOT `photonjet.png`)
4. **Use Harmony patches for power triggers** - Don't try to override non-existent methods
5. **Compile frequently** - Use `dotnet publish -c ExportRelease` to catch errors early
6. **Use ILSpy MCP to understand game mechanics** - Decompile game code to see how built-in features work
