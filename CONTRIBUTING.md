# Contributing to RimWorld: The Cosmere

Thank you for your interest in contributing to RimWorld: The Cosmere. This document explains how to set up your
development environment, add new shardworlds, and follow our commit and release workflow.

## Local Development Setup

### Requirements

- [.Net SDK 9.0](https://dotnet.microsoft.com/en-us/download/dotnet/9.0)
- [.Net Framework 4.8](https://dotnet.microsoft.com/en-us/download/dotnet-framework/net48)
- [Unity Hub](https://unity.com/download)
- [Unity Editor 2022.3.35f1](https://unity.com/releases/editor/whats-new/2022.3.35)
- [Make](https://gnuwin32.sourceforge.net/packages/make.htm) (optional - Windows users can use `make.ps1` instead)

To develop locally:

1. Generate code and build assets:

    **Option A: Using Make (if installed):**
    ```bash
    make generatables  # Generate code and build assets
    ```

    **Option B: Using PowerShell script (Windows):**
    ```powershell
    .\make.ps1 generatables  # Generate code and build assets
    ```

    **Available commands:**
    - `make help` or `.\make.ps1 help` - Show all available commands
    - `make generatables` - Generate code and build assets (recommended for development)
    - `make generate` - Generate code only
    - `make build-assets` - Build Unity AssetBundles only (requires Unity 2022.3.35f1)
    - `make all` - Full build pipeline (clean, generate, build, assets)

    **Note:** AssetBundle building requires Unity 2022.3.35f1 to be installed. The AssetBundleBuilder tool will automatically find and use your Unity installation.

2. For development, use the provided RimWorld run configurations in your IDE.
    - The [GarethP RimWorld plugin](https://plugins.jetbrains.com/plugin/18442-rimworld) is highly recommended for Rider users as it provides RimWorld-specific tooling and run configurations.

3. Ensure generated files remain gitignored. Don't check them in.

4. Symlink the mod folders you are working on into your RimWorld Mods directory.

   Example (Windows PowerShell):

    ```powershell
    New-Item -ItemType SymbolicLink -Path "$env:APPDATA\..\LocalLow\Ludeon Studios\RimWorld by Ludeon Studios\Mods\CosmereScadrial" -Target "C:\Path\To\RimworldCosmere\CosmereScadrial"
    ```

## Creating a New Shardworld (or Mod)

Use `CosmereScadrial` as a reference for setting up a new shardworld.

1. Add C# code in `CosmereCore/CosmereCore/System/{WorldName}/` following the namespace pattern `Cosmere.System.{WorldName}.*`
2. Create a `Mod.cs` in your world directory inheriting from `CosmereMod`
3. For settings, see `Cosmere.System.Scadrial.Settings.ScadrialModSettings`
4. For quickstarting, see `Cosmere.System.Scadrial.Quickstart.PreCatacendreQuickstart`
5. Create your world mod directory `Cosmere{WorldName}/` with:
    - `About/About.xml` - Mod metadata
    - `Defs/` - XML definitions
    - `Assets/` - Unity assets (Textures, Materials, Audio, etc.)
    - `loadFolders.xml` - Load order configuration
    - `.steamignore` - Files to exclude from Steam

6. Add global using statement to `CosmereCore/CosmereCore/Cosmere.csproj`:
    ```xml
    <Using Include="Cosmere.System.{WorldName}.Extension"/>
    ```

New world mods only need to depend on `CosmereCore` - all shared code is in the single assembly.

## Development Workflow

Create a branch for your changes. Branch names don’t matter.

Pushes to the following branches will trigger CI/CD:

- `alpha`: unstable or experimental work
- `beta`: stable work ready for broader testing
- `main`: production-ready code that will be released to Steam

Rebase merges are preferred for all contributions.

### Testing and Submitting

- Always test your changes in-game.
- Provide screenshots and detailed steps to reproduce or test the behavior you’re adding or changing.

### CI/CD and Releasing

This project uses [semantic-release](https://github.com/semantic-release/semantic-release) for automated versioning and
publishing.

-

Use [Angular-style commit messages](https://github.com/angular/angular/blob/main/contributing-docs/commit-message-guidelines.md)

- Follow [Semantic Versioning](https://semver.org/)

Commits are not linted, but proper formatting is required for inclusion in releases.

CI will:

- Create prerelease builds for `alpha` and `beta` branches (published on GitHub only)
- Create full releases for `main` (published to GitHub and Steam)

GitHub Actions runs are visible to all contributors.

## Notes

- You do not need to manually tag versions. CI handles release tagging.
- If you are unsure where your code should land (`alpha`, `beta`, or `main`), ask in Discord.
