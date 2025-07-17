# Contributing to RimWorld: The Cosmere

Thank you for your interest in contributing to RimWorld: The Cosmere. This document explains how to set up your
development environment, add new shardworlds, and follow our commit and release workflow.

## Local Development Setup

### Requirements

- [.Net SDK 9.0](https://dotnet.microsoft.com/en-us/download/dotnet/9.0)
- [.Net Framework 4.8](https://dotnet.microsoft.com/en-us/download/dotnet-framework/net48)
- [Unity Hub](https://unity.com/download)
- [Unity Editor 2022.3.35f1](https://unity.com/releases/editor/whats-new/2022.3.35) (located in
  `C:\Program Files\Unity\Hub\Editor\2022.3.35f1\Editor\Unity.exe`)
- [Node.js v24](https://nodejs.org/en/download/current)
- [Powershell 7](https://learn.microsoft.com/en-us/powershell/scripting/install/installing-powershell-on-windows)

To develop locally:

1. Clone [AssetBuilder](https://github.com/RimworldCosmere/AssetBuilder) into a sibling directory (not inside this
   repo):

    ```
    /RimworldCosmere
    /AssetBuilder
    ```

2. Navigate to `./.scripts/` and install dependencies:

    ```bash
    cd .scripts
    npm install
    ```

    - Node.js v24 is required.

3. Start the asset watcher:

    ```bash
    npm start -- -d -f
    ```

    - This can be added as a pre-build step in your IDE.
    - If using Rider, a run configuration is already provided.
    - The [GarethP RimWorld plugin](https://plugins.jetbrains.com/plugin/18442-rimworld) is recommended for Rider.

4. From the root of this repo, run:

    ```powershell
    ./buildAllCosmereBundles.ps1
    ```

    - This can also be added as a pre-build step in your IDE.
    - It generates the final bundles for each mod.

5. Ensure generated files remain gitignored. Don’t check them in.

6. Symlink the mod folders you are working on into your RimWorld Mods directory.

   Example (Windows PowerShell):

    ```powershell
    New-Item -ItemType SymbolicLink -Path "$env:APPDATA\..\LocalLow\Ludeon Studios\RimWorld by Ludeon Studios\Mods\CosmereScadrial" -Target "C:\Path\To\RimworldCosmere\CosmereScadrial"
    ```

## Creating a New Shardworld (or Mod)

Use `CosmereScadrial` as a reference for setting up a new shardworld.

1. Create your `CosmereX.CosmereX` mod class using the same pattern.
2. For settings, see `CosmereScadrial.Settings`.
3. For quickstarting colonists or scenarios, see `CosmereScadrial.Quickstart`.
4. Organize assets in the following structure:

    ```
    Assets/Textures/
    Assets/Materials/
    Assets/Terrain/
    Assets/Audio/
    ```

5. Update `buildAllCosmereBundles.ps1` to include your mod in the `$mods` list on line 8.
6. Copy a `.steamignore` file from another mod so that unneeded files are excluded from Steam uploads.

New mods should reference the following shared projects:

- `CosmereCore`
- `CosmereFramework`
- `CosmereResources` (optional but likely)

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
