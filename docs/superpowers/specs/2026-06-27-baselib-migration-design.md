# BaseLib Migration Design

**Date:** 2026-06-27  
**Status:** Approved  
**Approach:** A — NuGet Package Reference

## Goal

Migrate the Joi mod from a locally-modified BaseLib v0.2.1 to the official `Alchyr.Sts2.BaseLib` NuGet package (latest v3.3.x), removing all local BaseLib dependencies and fixing API compatibility.

## Current State

- BaseLib v0.2.1 DLL, PCK, and JSON are stored in `BaseLib/` directory
- `.csproj` references `BaseLib.dll` as a local `<Reference>`
- Build copies `BaseLib.dll`, `BaseLib.pck`, `BaseLib.json` to mods folder
- Source code uses: `BaseLib.Abstracts`, `BaseLib.Utils`, `BaseLib.Extensions`, `BaseLib.Utils.NodeFactories`, `BaseLib.Hooks`

## Target State

- NuGet package `Alchyr.Sts2.BaseLib` (floating version `*`) provides the DLL
- `BaseLib/` directory retains only `.pck` and `.json` resource files (sourced from official GitHub releases)
- All code compiles against the official BaseLib API
- Build copies official BaseLib `.pck` and `.json` to mods folder (DLL handled by NuGet output)

## Changes

### 1. .csproj Modifications

**Remove:**
- `<BaseLibDir>`, `<BaseLibDllPath>`, `<BaseLibJsonPath>`, `<BaseLibPckPath>` PropertyGroup entries
- `<Reference Include="BaseLib">` ItemGroup block
- BaseLib existence checks from `CheckDependencyPaths` target
- BaseLib file copies from `CopyToModsFolderOnBuild` target (DLL portion)

**Add:**
- `<PackageReference Include="Alchyr.Sts2.BaseLib" Version="*" />`

**Modify:**
- `CopyToModsFolderOnBuild` — continue copying `BaseLib.pck` and `BaseLib.json` to mods folder

### 2. Resource Files

- Replace `BaseLib/BaseLib.pck` and `BaseLib/BaseLib.json` with versions from official [GitHub releases](https://github.com/Alchyr/BaseLib-StS2/releases)
- Remove `BaseLib/BaseLib.dll` and `BaseLib/.gdignore` (no longer needed)
- Remove `BaseLib/` from `<None Include="BaseLib\**" />` (or narrow to only .pck/.json)

### 3. API Compatibility Fixes

Compile after NuGet switch, then fix all errors. Expected areas needing attention:

| Namespace | Risk | Notes |
|-----------|------|-------|
| `BaseLib.Abstracts` | Medium | Base classes may have new abstract members or changed signatures |
| `BaseLib.Utils` | High | `CommonActions`, `SummonActions`, `SummonDefinition` APIs likely evolved across 3 major versions |
| `BaseLib.Extensions` | Medium | Extension methods on models may have changed |
| `BaseLib.Utils.NodeFactories` | Low | `NodeFactory<T>.CreateFromScene` — verify still exists |
| `BaseLib.Hooks` | Low | No actual usage found — imports may be removable |

### 4. Validation

- `dotnet build` succeeds with zero errors
- `dotnet publish -c ExportRelease` succeeds
- All cards, powers, relics, and ZhouXin minion function correctly in-game

## Out of Scope

- Upgrading any other dependencies
- Changing mod architecture beyond BaseLib migration
- Re-implementing any custom BaseLib modifications in the Joi codebase (if official BaseLib lacks needed functionality, that will be addressed as a separate follow-up)
