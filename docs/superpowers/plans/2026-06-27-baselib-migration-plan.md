# BaseLib Migration Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Migrate Joi mod from locally-modified BaseLib v0.2.1 DLL to official `Alchyr.Sts2.BaseLib` NuGet package (v3.3.x).

**Architecture:** Replace local `<Reference>` with NuGet `<PackageReference>`, update `BaseLib/` to contain only official `.pck`/`.json` resource files from GitHub releases, fix API breakages discovered at compile time.

**Tech Stack:** .NET 9.0, Godot 4.5.1, Harmony, NuGet

## Global Constraints

- Target framework: net9.0
- All code paths must compile with `dotnet build`
- All code paths must compile with `dotnet publish -c ExportRelease`
- BaseLib.pck and BaseLib.json must come from official [GitHub releases](https://github.com/Alchyr/BaseLib-StS2/releases)
- Do NOT modify any game logic — only BaseLib migration changes
- Must work on macOS (the current dev machine)

---

### Task 1: Update .csproj — Replace local DLL with NuGet package

**Files:**
- Modify: `Joi.csproj`

**What this does:** Removes the local BaseLib DLL reference and adds the official NuGet package. Also removes BaseLib from build-time existence checks since NuGet handles that.

- [ ] **Step 1: Remove BaseLib path variables from PropertyGroup**

Replace the block at lines 10-13:
```xml
<BaseLibDir>$(MSBuildProjectDirectory)\BaseLib</BaseLibDir>
<BaseLibDllPath>$(BaseLibDir)\BaseLib.dll</BaseLibDllPath>
<BaseLibJsonPath>$(BaseLibDir)\BaseLib.json</BaseLibJsonPath>
<BaseLibPckPath>$(BaseLibDir)\BaseLib.pck</BaseLibPckPath>
```

With nothing (delete these 4 lines).

- [ ] **Step 2: Remove the local DLL reference**

Delete the entire block at lines 88-93:
```xml
<ItemGroup Condition="Exists('$(BaseLibDllPath)')">
    <Reference Include="BaseLib">
        <HintPath>$(BaseLibDllPath)</HintPath>
        <Private>false</Private>
    </Reference>
</ItemGroup>
```

- [ ] **Step 3: Add NuGet package reference**

In the existing `<ItemGroup>` that contains the `ModAnalyzers` package (lines 95-98), add a new line:

```xml
<ItemGroup>
    <PackageReference Include="Alchyr.Sts2.ModAnalyzers" Version="*" />
    <PackageReference Include="Alchyr.Sts2.BaseLib" Version="*" />
    <AdditionalFiles Include="Joi/localization/**/*.json" />
</ItemGroup>
```

- [ ] **Step 4: Remove BaseLib checks from CheckDependencyPaths target**

In the `CheckDependencyPaths` target (lines 125-131), remove lines 128-130:
```xml
<Error Text=" BaseLib.dll not found at path '$(BaseLibDllPath)'" Condition="'$(BaseLibDllPath)' == '' or !Exists('$(BaseLibDllPath)')" />
<Error Text=" BaseLib.json not found at path '$(BaseLibJsonPath)'" Condition="'$(BaseLibJsonPath)' == '' or !Exists('$(BaseLibJsonPath)')" />
<Error Text=" BaseLib.pck not found at path '$(BaseLibPckPath)'" Condition="'$(BaseLibPckPath)' == '' or !Exists('$(BaseLibPckPath)')" />
```

- [ ] **Step 5: Update CopyToModsFolderOnBuild target**

Replace lines 137-138:
```xml
<Copy SourceFiles="$(BaseLibDllPath);$(BaseLibJsonPath);$(BaseLibPckPath)" DestinationFolder="$(ModsPath)BaseLib/" />
```

With (keeping the message line above it):
```xml
<Message Text="Copying BaseLib resource files to mods folder." Importance="high" />
<Copy SourceFiles="$(MSBuildProjectDirectory)\BaseLib\BaseLib.json;$(MSBuildProjectDirectory)\BaseLib\BaseLib.pck" DestinationFolder="$(ModsPath)BaseLib/" />
```

- [ ] **Step 6: Restore NuGet packages**

Run: `dotnet restore`
Expected: Restore succeeds, `Alchyr.Sts2.BaseLib` package downloaded.

- [ ] **Step 7: Commit**

```bash
git add Joi.csproj
git commit -m "build: replace local BaseLib DLL with NuGet package reference"
```

---

### Task 2: Update BaseLib resource files from official releases

**Files:**
- Modify: `BaseLib/BaseLib.json`
- Modify: `BaseLib/BaseLib.pck`
- Delete: `BaseLib/BaseLib.dll`

**What this does:** Replaces the v0.2.1 `.pck` and `.json` with the latest official versions from GitHub releases. Removes the now-obsolete local DLL.

- [ ] **Step 1: Determine latest BaseLib release version**

Fetch the latest release info:
```bash
curl -sL https://api.github.com/repos/Alchyr/BaseLib-StS2/releases/latest | jq -r '.tag_name'
```
Note the tag name (e.g., `v3.3.2`).

- [ ] **Step 2: Download official BaseLib.pck**

```bash
curl -sL https://github.com/Alchyr/BaseLib-StS2/releases/latest/download/BaseLib.pck -o BaseLib/BaseLib.pck
```

- [ ] **Step 3: Download official BaseLib.json**

```bash
curl -sL https://github.com/Alchyr/BaseLib-StS2/releases/latest/download/BaseLib.json -o BaseLib/BaseLib.json
```

- [ ] **Step 4: Verify BaseLib.json version**

```bash
cat BaseLib/BaseLib.json | jq '.version'
```
Expected: Should show `v3.x.x` (not `v0.2.1`).

- [ ] **Step 5: Delete the obsolete local BaseLib.dll**

```bash
rm BaseLib/BaseLib.dll
```

- [ ] **Step 6: Delete BaseLib/.gdignore if present**

```bash
rm -f BaseLib/.gdignore
```

- [ ] **Step 7: Commit**

```bash
git add BaseLib/BaseLib.json BaseLib/BaseLib.pck
git rm BaseLib/BaseLib.dll
git rm --cached BaseLib/.gdignore 2>/dev/null || true
git commit -m "chore: update BaseLib resource files to official release, remove local DLL"
```

---

### Task 3: First compile — discover API breakages

**Files:**
- All `JoiCode/**/*.cs` (read via compiler errors — no changes yet)

**What this does:** Runs `dotnet build` to get the full list of API compatibility errors between v0.2.1 and v3.3.x BaseLib.

- [ ] **Step 1: Run the build**

```bash
dotnet build 2>&1
```

- [ ] **Step 2: Capture and categorize errors**

The build will fail. Save the output and categorize errors into groups:
```bash
dotnet build 2>&1 | tee /tmp/baselib-migration-errors.txt
```

Review `/tmp/baselib-migration-errors.txt` and categorize:
- **Group A:** Deleted/missing types or namespaces (e.g., `BaseLib.Hooks` removed)
- **Group B:** Renamed or moved types (e.g., `CommonActions` moved to different namespace)
- **Group C:** Changed method signatures (e.g., `SummonActions.SummonAlly` takes different params)
- **Group D:** New abstract members that must be implemented
- **Group E:** Deprecated APIs with replacements

- [ ] **Step 3: Document findings**

Record each distinct error type in the commit message or a tracking comment for the fix tasks.

---

### Task 4: Fix API breakages — iterative compile-fix loop

**Files:**
- Modify: `JoiCode/**/*.cs` (as needed per compiler errors)

**What this does:** Fixes all compilation errors discovered in Task 3. This task is structured as a loop — fix a batch of related errors, recompile, repeat until clean.

**Loop strategy:** Fix one error category at a time. After each fix batch, run `dotnet build` to verify. Commit after each successful fix batch.

- [ ] **Step 1: Remove unused `using BaseLib.Hooks;` imports**

If compiler reports `BaseLib.Hooks` namespace doesn't exist, remove `using BaseLib.Hooks;` from all files.

Files to check (from Task 3 output):
```
JoiCode/Powers/EventHorizonPower.cs
JoiCode/Powers/SingularityPower.cs
JoiCode/Powers/GravityFieldPower.cs
JoiCode/Powers/StellarRequiemPower.cs
JoiCode/Powers/BirthPower.cs
JoiCode/Powers/StoragePower.cs
JoiCode/Powers/VacuumFluctuationPower.cs
JoiCode/Powers/CosmicConstantPower.cs
JoiCode/Powers/OmniscientPower.cs
JoiCode/Powers/GravitationalLensPower.cs
JoiCode/Minions/ZhouXin.cs
```

Run build, verify reduced error count.

- [ ] **Step 2: Fix namespace changes for BaseLib.Utils types**

If `CommonActions`, `SummonActions`, `SummonDefinition`, `SummonPolicy`, `CreatureVisualSpec`, `VfxCmd` moved to a different namespace (e.g., `BaseLib` or `BaseLib.Actions`), update the `using` directives.

Common pattern — replace:
```csharp
using BaseLib.Utils;
```
With the correct namespace (determined from compiler error suggestions or ILSpy inspection of the NuGet DLL).

Run build, verify reduced error count.

- [ ] **Step 3: Fix `BaseLib.Utils.NodeFactories` → verify new location**

If `NodeFactory<T>.CreateFromScene` moved, update `ZhouXin.cs`:
```csharp
// Old
using BaseLib.Utils.NodeFactories;
// New namespace TBD from compiler errors
```

Run build, verify reduced error count.

- [ ] **Step 4: Fix abstract member additions**

If `CustomCardModel`, `CustomPowerModel`, `CustomRelicModel`, `CustomMonsterModel`, or `PlaceholderCharacterModel` now require new abstract members, implement them.

For each missing member, read the definition from the NuGet DLL (use ILSpy or compiler hints) and implement the required override in the appropriate base class (`JoiCard.cs`, `JoiPower.cs`, `JoiRelic.cs`, `ZhouXin.cs`, `Joi.cs`).

Run build, verify reduced error count.

- [ ] **Step 5: Fix changed method signatures**

If any method signatures changed (e.g., `CommonActions.CardAttack`, `SummonActions.SummonAlly`, `PowerCmd.Apply`), update call sites to match new signatures. Add newly required parameters, remove obsolete ones.

Run build, verify reduced error count.

- [ ] **Step 6: Fix any remaining errors**

Handle any remaining compiler errors not covered above. Run `dotnet build` until zero errors.

- [ ] **Step 7: Commit**

```bash
git add JoiCode/
git commit -m "fix: resolve BaseLib API compatibility after migration to v3.x"
```

---

### Task 5: Verify publish build

**Files:**
- All project files (read-only verification)

**What this does:** Runs the full publish build to verify everything works end-to-end.

- [ ] **Step 1: Run publish build**

```bash
dotnet publish -c ExportRelease 2>&1
```
Expected: SUCCESS with zero errors.

Note: If `$(Sts2DataDir)` is not available on this machine, the build may skip the Godot export step — that's fine. The `.dll` compilation is what matters.

- [ ] **Step 2: Verify output files exist**

```bash
ls -la bin/ExportRelease/net9.0/publish/Joi.dll
```
Expected: `Joi.dll` exists.

- [ ] **Step 3: Verify BaseLib NuGet DLL is included in publish output**

```bash
ls bin/ExportRelease/net9.0/publish/ | grep -i baselib
```
Expected: `BaseLib.dll` should appear in publish output (from NuGet).

- [ ] **Step 4: Commit any final changes**

```bash
git add -A
git diff --cached --stat
git commit -m "chore: final adjustments for BaseLib migration publish build"
```

---

### Task 6: Cleanup and final verification

**Files:**
- Verify: `BaseLib/` directory is clean
- Verify: Git diff is as expected

**What this does:** Ensures no leftover files from the old BaseLib setup remain.

- [ ] **Step 1: Verify BaseLib/ directory contents**

```bash
ls -la BaseLib/
```
Expected: Only `BaseLib.pck` and `BaseLib.json` — no `.dll`, no `.gdignore`.

- [ ] **Step 2: Verify no references to old BaseLib paths remain**

```bash
grep -r "BaseLibDllPath\|BaseLibJsonPath\|BaseLibPckPath\|BaseLibDir" . --include="*.csproj" --include="*.cs" --include="*.md" 2>/dev/null
```
Expected: No results (or only in docs/specs/plan files).

- [ ] **Step 3: Final git status check**

```bash
git status
```
Expected: Clean working tree.

- [ ] **Step 4: Commit**

```bash
git commit --allow-empty -m "chore: BaseLib migration complete — switched to official NuGet package"
```
