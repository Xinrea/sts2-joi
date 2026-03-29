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

### Problem 3: Card Images
**Issue**: Cards need portrait images to display properly.

**Solution**:
- Create SVG files for card art
- Convert to PNG using: `sips -s format png input.svg --out output.png`
- Place in `Joi/images/card_portraits/` with lowercase filename matching card class name
- Example: `PhotonJet` class → `photonjet.png`

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
   - Use lowercase filename matching class name

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

## Key Takeaways

1. **Always add Pool attribute to each card class** - Don't rely on inheritance
2. **Add both English and Chinese localizations** - Missing either will cause errors
3. **Create card portrait images** - Required for cards to display properly
4. **Use Harmony patches for power triggers** - Don't try to override non-existent methods
5. **Compile frequently** - Use `dotnet publish -c ExportRelease` to catch errors early
