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

> **IMPORTANT**: All steps below are MANDATORY. Do NOT skip card image generation (step 3) — every new card must have a card portrait generated.

1. **Create Card Class**
   - Inherit from `JoiCard`
   - Add `[Pool(typeof(JoiCardPool))]` attribute
   - Implement constructor, CanonicalVars, OnPlay, and OnUpgrade

2. **Add Localizations**
   - Add entries to both `eng/cards.json` and `zhs/cards.json`
   - Format: `JOI-CARD_NAME.title` and `JOI-CARD_NAME.description`

3. **Create Card Images**
   - Generate card portrait using `generate_card_art.ps1`:
     ```powershell
     powershell.exe -File generate_card_art.ps1 -Prompt "Slay the Spire card art style, [description]" -OutputName card_name
     ```
   - Script automatically uses `Joi.png` as character reference
   - If image is too large (>1MB), resize using `resize_image.ps1`:
     ```powershell
     powershell.exe -File resize_image.ps1 -InputFile "Joi/images/card_portraits/card_name.png" -Size 512
     ```
   - If Godot shows "Not a PNG file" error, delete the `.import` file and recompile
   - **IMPORTANT**: Use snake_case with underscores (e.g., `photon_jet.png`, NOT `photonjet.png`)
   - Filename must match: ClassName → snake_case (PhotonJet → photon_jet.png)

4. **Add Power Localizations (if needed)**
   - Add to both `eng/powers.json` and `zhs/powers.json`
   - Include title, description, and smartDescription

5. **Create Power Icons (if needed)**
   - Generate power icon using `generate_power_icon.ps1`:
     ```powershell
     powershell.exe -File generate_power_icon.ps1 -Prompt "simple icon description" -OutputName power_name
     ```
   - Script generates two sizes: 64x64 (small) and 128x128 (big)
   - Small icon saved to `Joi/images/powers/power_name.png`
   - Big icon saved to `Joi/images/powers/big/power_name.png`
   - Icon requirements:
     - Solid bright green (#00FF00) background
     - Simple, symbolic, minimalist design
     - Soft glowing colors (white, light blue, purple, gold, pink)
     - Clean geometric shapes, no realistic details
     - No text, no border, no shadow
   - **IMPORTANT**: Use snake_case filename matching power class name (e.g., `PeriodicTablePower` → `periodic_table_power.png`)

6. **Compile and Test**
   - Run `dotnet publish -c ExportRelease`
   - Check for errors
   - Test in-game

## Localization Formatters

### Using :show Formatter for Upgrade Display
**Issue**: Need to show conditional text based on upgrade status (e.g., "X times" → "X+1 times").

**Solution**: Use the `:show` formatter with correct syntax:

```json
{
  "CARD.description": "Deal X{IfUpgraded:show:+1|} times damage."
}
```

**Syntax Rules:**
- Format: `{IfUpgraded:show:upgraded_text|non_upgraded_text}`
- Use **colon `:`** to separate formatter name from parameters
- Use **pipe `|`** to separate upgraded and non-upgraded text
- Leave non-upgraded text empty if you only want to show text when upgraded

**Examples:**
```json
// Shows "X times" when not upgraded, "X+1 times" when upgraded
"X{IfUpgraded:show:+1|} times"

// Shows "all cards" when upgraded, "1 card" when not upgraded
"{IfUpgraded:show:all cards|1 card}"
```

**Common Mistake:**
- ❌ Wrong: `{IfUpgraded:show|+1|}` (using pipe instead of colon)
- ✅ Correct: `{IfUpgraded:show:+1|}` (using colon after show)

## Common Patterns

### Creating Card Instances at Runtime
**Issue**: Calling `new Orange()` or any card constructor at runtime throws `DuplicateModelException`. Using `ModelDb.Card<T>().ToMutable().CreateDupe()` throws `NullReferenceException` because the card lacks combat context.

**Root Cause**: The game registers each model type as a singleton canonical model. Calling the constructor creates a duplicate. `ToMutable()` creates a card without proper combat scope, so `CreateClone()` fails.

**Solution**: Use `combatState.CreateCard<T>(owner)` to create cards during combat:

```csharp
// ❌ Wrong - causes DuplicateModelException
var card = new Orange();

// ❌ Wrong - causes NullReferenceException on CreateDupe/CreateClone
var card = ModelDb.Card<Orange>().ToMutable().CreateDupe();

// ✅ Correct - use CombatState.CreateCard
var combatState = Owner.Creature.CombatState;
var card = combatState.CreateCard<Orange>(Owner);
if (IsUpgraded) card.UpgradeInternal();
await CardPileCmd.AddGeneratedCardToCombat(card, PileType.Hand, true);
```

### Card with Dynamic Variables
```csharp
protected override IEnumerable<DynamicVar> CanonicalVars =>
[
    new DamageVar(8, ValueProp.Move),
    new DynamicVar("BlackHole", 1)
];
```

### Card with Keywords
Keywords like Exhaust, Ethereal, etc. are added via the `CanonicalKeywords` property:

```csharp
public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust, CardKeyword.Ethereal];
```

**Important**:
- Do NOT use `AddKeyword()` in the constructor - it will cause a "CanonicalModelException"
- Use the `CanonicalKeywords` property instead
- Do NOT add keyword descriptions (like "Exhaust." or "Ethereal.") to the card description text
- The game automatically displays keywords based on the `CanonicalKeywords` property

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

## Image Generation

When generating card art or other images:

**API Configuration**:
- Endpoint: `https://openrouter.ai/api/v1/chat/completions`
- Model: `bytedance-seed/seedream-4.5`
- Requires: `OPENROUTER_API_KEY` environment variable

**Workflow**:
1. Generate image via OPENROUTER API with Slay the Spire style prompt
2. Save as PNG in `Joi/images/card_portraits/`
3. Use snake_case filename matching card class name (e.g., `photon_jet.png`)

**Prompt Template**:
Design specific character actions and gestures for each card, avoid generic descriptions. The action must be impactful and dynamic:
```
the character from the reference image [specific action/gesture], [visual effects], no border, fantasy card game illustration
```

**Examples**:
- Good: "holding up hands to create a gravitational lens, light bending around her fingers"
- Bad: "with gravitational lens effect, light bending"
- Good: "clapping hands together to generate gravitational waves, shockwave ripples emanating from her palms"
- Bad: "creating gravitational waves, wave distortion effects"

**Impact Requirements**:
- The action must feel powerful and dynamic — avoid static poses like "standing tall" or "arms crossed"
- Use motion blur, shockwaves, explosive energy bursts, or dramatic mid-action poses
- Show the peak moment of an action (e.g., fist just landed, energy just released, leap at apex)
- For attack cards: use physical strikes with shockwaves and motion blur
- For dominance/debuff/power cards: use psychological pressure — size contrast, condescending gestures, cowering enemies, oppressive aura draining the target
- Examples of impactful actions: "lunging forward with a devastating punch, golden shockwave exploding from fist on impact", "leaping into the air with both hands raised, massive energy orb crackling between palms"
- Examples of bullying/dominance: "looking down arrogantly with a smug expression, pointing finger condescendingly at a cowering smaller figure below, golden oppressive aura radiating from her, the smaller figure visibly weakened and shrinking"

**Example Usage**:
```bash
powershell.exe -File generate_card_art.ps1 -Prompt "the character from the reference image collecting gold coins and rice, cheerful expression, floating gold coins" -OutputName collect_rice
```

## Key Takeaways

1. **Always add Pool attribute to each card class** - Don't rely on inheritance
2. **Add both English and Chinese localizations** - Missing either will cause errors
3. **Use snake_case for card image filenames** - Must use underscores (e.g., `photon_jet.png`, NOT `photonjet.png`)
4. **Use Harmony patches for power triggers** - Don't try to override non-existent methods
5. **Compile frequently** - Use `dotnet publish -c ExportRelease` to catch errors early
6. **Use ILSpy MCP to understand game mechanics** - Decompile game code to see how built-in features work


## Always Generate Art and Changelog

After completing card creation or significant changes:
1. Generate card art (512x384 PNG, Joi character reference)
2. Generate/update CHANGELOG.md documenting all changes
3. Verify localization files match the actual implementation

Do NOT skip these steps even for 'quick fixes'.
Add under ## Code Style or create new ## Localization section\n\n## Localization First

Before writing ANY card names, descriptions, or tooltips:
1. Read existing Chinese localization files (zh-CN/) to find current terminology
2. Copy the format and style from similar entries
3. Do NOT generate English content and ask to translate later

For Chinese changelogs: Read the Chinese localization files, don't guess terminology.
Add under a new ## Joi Mod Development section\n\n## Mod Architecture Notes

- Use BaseLib references (not Harmony) for character/power registration
- For UI patches: RegisterPetForPlayer (not PetOwner) for ZhouXin positioning
- Remove cards at OnDestroy phase, not during CombatState transitions
- Use CanonicalKeywords property, not AddKeyword in constructor
Add under ## Workflow Guidelines\n\n## Iterative Development

When fixing bugs or implementing complex features:
1. Make ONE change at a time
2. Verify build compiles
3. Test in-game before making additional changes
4. If a patch fails, research the game source code (use ilspy MCP) to find correct hook

Avoid: Multiple simultaneous changes that cause cascading failures requiring rollbacks.


## Reference

@BASELIB.md
