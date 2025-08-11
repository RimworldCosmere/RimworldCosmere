# RimWorld Cosmere - Development PowerShell Script
# Windows companion to Makefile for users without Make
#
# Usage: .\make.ps1 [command] [-VerboseOutput]
# Example: .\make.ps1 help

param(
    [Parameter(Position=0)]
    [string]$Command = "help",
    [switch]$VerboseOutput
)

# Colors for output
$Green = "Green"
$Blue = "Cyan"
$Yellow = "Yellow"
$Red = "Red"

function Write-ColoredLine {
    param([string]$Text, [string]$Color = "White")
    Write-Host $Text -ForegroundColor $Color
}

function Write-Section {
    param([string]$Title)
    Write-ColoredLine "`n$Title" $Blue
}

function Write-Success {
    param([string]$Text)
    Write-ColoredLine "[OK] $Text" $Green
}

function Write-Info {
    param([string]$Text)
    Write-ColoredLine $Text $Blue
}

function Write-Warning {
    param([string]$Text)
    Write-ColoredLine $Text $Yellow
}

function Invoke-DotNet {
    param([string]$Arguments)
    if ($VerboseOutput) { Write-Host "dotnet $Arguments" -ForegroundColor Gray }
    Invoke-Expression "dotnet $Arguments"
    if ($LASTEXITCODE -ne 0) { throw "Command failed: dotnet $Arguments" }
}

function Invoke-ToolsCli {
    param([string]$Arguments)
    if ($VerboseOutput) { Write-Host "dotnet run --project tools/Cosmere.Tools.Cli -- $Arguments" -ForegroundColor Gray }
    Invoke-Expression "dotnet run --project tools/Cosmere.Tools.Cli -- $Arguments"
    if ($LASTEXITCODE -ne 0) { throw "Tools CLI command failed: $Arguments" }
}

switch ($Command.ToLower()) {
    "help" {
        Write-ColoredLine "RimWorld Cosmere Development Script" $Blue
        Write-Host ""
        Write-Host "Usage: .\make.ps1 [command]" -ForegroundColor White
        Write-Host ""
        
        Write-Section "General Commands"
        Write-Host "  help          - Show this help message"
        Write-Host "  all           - Full build pipeline"
        Write-Host "  build         - Build everything (no clean) for IDE run"
        Write-Host "  build-all     - Full build pipeline (alias for all)"
        Write-Host "  run           - Full build and launch RimWorld"
        Write-Host "  quick         - Quick development cycle"
        Write-Host "  dev           - Development build"
        Write-Host "  setup         - Initial project setup"
        
        Write-Section "Code Generation"
        Write-Host "  generate      - Generate all code"
        Write-Host "  gen-clean     - Clean generated files"
        Write-Host "  gen-dry       - Dry run generation"
        Write-Host "  gen-force     - Force regeneration"
        Write-Host "  gen-metals    - Generate Resources only"
        Write-Host "  gen-genes     - Generate GenesAndTraits only"
        Write-Host "  gen-hediffs   - Generate FeruchemicalHediffs only"
        Write-Host "  gen-metallic  - Generate MetallicArtsMetals only"
        Write-Host "  gen-roshar    - Generate SurgesAndOrders only"
        
        Write-Section "Building"
        Write-Host "  build-main    - Build main solution"
        Write-Host "  build-tools   - Build Tools CLI"
        Write-Host "  build-assets  - Build Unity AssetBundles"
        Write-Host "  build-debug   - Build in debug mode"
        
        Write-Section "Development Tools"
        Write-Host "  clean         - Clean build outputs"
        Write-Host "  restore       - Restore NuGet packages"
        Write-Host "  format        - Format C# code"
        Write-Host "  test          - Run tests"
        Write-Host "  lint          - Run linting"
        Write-Host "  precommit     - Pre-commit checks"
        
        Write-Section "Information"
        Write-Host "  info          - Show project information"
        Write-Host "  versions      - Show tool versions"
        Write-Host "  status        - Git and solution status"
    }
    
    "all" {
        Write-Info "Starting full build pipeline..."
        & $PSCommandPath clean
        & $PSCommandPath generate
        & $PSCommandPath build-main
        & $PSCommandPath build-tools
        & $PSCommandPath build-assets
        Write-Success "Full build complete!"
    }
    
    "generatables" {
        Write-Info "Building generatables (assets and generated code)..."
        & $PSCommandPath generate
        & $PSCommandPath build-assets
        Write-Success "Generatables complete!"
    }
    
    "build" {
        Write-Info "Building everything (no clean)..."
        & $PSCommandPath generate
        & $PSCommandPath build-main
        & $PSCommandPath build-assets
        Write-Success "Build complete - ready for launch!"
    }
    
    "build-all" {
        Write-Info "Full build pipeline..."
        & $PSCommandPath clean
        & $PSCommandPath generate
        & $PSCommandPath build-main
        & $PSCommandPath build-tools
        & $PSCommandPath build-assets
        Write-Success "Full build complete - ready for launch!"
    }
    
    "run" {
        Write-Info "Full build and launching RimWorld..."
        & $PSCommandPath all
        Write-Info "Launching RimWorld..."
        $steamPath = "C:\Program Files (x86)\Steam\steamapps\common\RimWorld\RimWorldWin64.exe"
        $epicPath = "C:\Program Files\Epic Games\RimWorld\RimWorldWin64.exe"
        if (Test-Path $steamPath) {
            Start-Process $steamPath
        } elseif (Test-Path $epicPath) {
            Start-Process $epicPath
        } else {
            Write-Warning "RimWorld not found in common locations. Please launch manually."
        }
    }
    
    "quick" {
        Write-Info "Quick development cycle..."
        & $PSCommandPath generate
        & $PSCommandPath build-main
        Write-Success "Quick build complete!"
    }
    
    "dev" {
        Write-Info "Development build..."
        & $PSCommandPath generate
        & $PSCommandPath build-main
        & $PSCommandPath build-assets
        Write-Success "Development build complete!"
    }
    
    "setup" {
        Write-Info "Setting up project..."
        & $PSCommandPath restore
        Write-Info "Installing Node.js dependencies..."
        Set-Location .scripts
        npm install
        Set-Location ..
        Write-Success "Project setup complete!"
    }
    
    "generate" {
        Write-Info "Generating all code..."
        Invoke-ToolsCli "generate --verbose"
    }
    
    "gen-clean" {
        Write-Info "Cleaning generated files..."
        Invoke-ToolsCli "generate --clean --verbose"
    }
    
    "gen-dry" {
        Write-Info "Dry run generation..."
        Invoke-ToolsCli "generate --dry-run --verbose"
    }
    
    "gen-force" {
        Write-Info "Force generating all code..."
        Invoke-ToolsCli "generate --force --verbose"
    }
    
    "gen-metals" {
        Write-Info "Generating Resources..."
        Invoke-ToolsCli "generate Resources --verbose"
    }
    
    "gen-genes" {
        Write-Info "Generating GenesAndTraits..."
        Invoke-ToolsCli "generate GenesAndTraits --verbose"
    }
    
    "gen-hediffs" {
        Write-Info "Generating FeruchemicalHediffs..."
        Invoke-ToolsCli "generate FeruchemicalHediffs --verbose"
    }
    
    "gen-metallic" {
        Write-Info "Generating MetallicArtsMetals..."
        Invoke-ToolsCli "generate MetallicArtsMetals --verbose"
    }
    
    "gen-roshar" {
        Write-Info "Generating SurgesAndOrders..."
        Invoke-ToolsCli "generate SurgesAndOrders --verbose"
    }
    
    "build-main" {
        Write-Info "Building main solution..."
        Invoke-DotNet "build Cosmere.sln --configuration Release --verbosity minimal"
    }
    
    "build-tools" {
        Write-Info "Building Tools solution..."
        Invoke-DotNet "build Cosmere.Tools.sln --configuration Release --verbosity minimal"
    }
    
    "build-assets" {
        Write-Info "Building Unity AssetBundles..."
        Invoke-ToolsCli "build-assets --verbose"
    }
    
    "build-debug" {
        Write-Info "Building in debug mode..."
        Invoke-DotNet "build Cosmere.sln --configuration Debug --verbosity minimal"
        Invoke-DotNet "build Cosmere.Tools.sln --configuration Debug --verbosity minimal"
    }
    
    "clean" {
        Write-Info "Cleaning build outputs..."
        Invoke-DotNet "clean Cosmere.sln --verbosity minimal"
        Invoke-DotNet "clean Cosmere.Tools.sln --verbosity minimal"
    }
    
    "restore" {
        Write-Info "Restoring packages..."
        Invoke-DotNet "restore Cosmere.sln --verbosity minimal"
        Invoke-DotNet "restore Cosmere.Tools.sln --verbosity minimal"
    }
    
    "format" {
        Write-Info "Formatting code..."
        Invoke-DotNet "format Cosmere.sln --include `"**/*.cs`" --verbosity minimal"
        Invoke-DotNet "format Cosmere.Tools.sln --include `"**/*.cs`" --verbosity minimal"
    }
    
    "test" {
        Write-Info "Running tests..."
        Invoke-DotNet "test Cosmere.sln --configuration Release --logger `"console;verbosity=minimal`""
    }
    
    "lint" {
        Write-Info "Running linting..."
        Invoke-DotNet "format Cosmere.sln --verify-no-changes --verbosity minimal"
        Invoke-DotNet "format Cosmere.Tools.sln --verify-no-changes --verbosity minimal"
    }
    
    "precommit" {
        Write-Info "Running pre-commit checks..."
        & $PSCommandPath generate
        & $PSCommandPath format
        & $PSCommandPath lint
        & $PSCommandPath test
        Write-Success "Pre-commit checks passed!"
    }
    
    "status" {
        Write-Section "Git Status"
        git status --short --branch
        Write-Section "Solutions"
        Write-Host "Main: Cosmere.sln"
        Write-Host "Tools: Cosmere.Tools.sln"
        Write-Section "Recent Commits"
        git log --oneline -5
    }
    
    "info" {
        Write-ColoredLine "RimWorld Cosmere Development Environment" $Blue
        Write-Host ""
        Write-ColoredLine "Project Structure:" $Green
        Write-Host "  CosmereFoundation    - Shared utilities and base classes"
        Write-Host "  CosmereCore          - Core game mechanics and systems"
        Write-Host "  CosmereResources     - Metals, gems, and materials"
        Write-Host "  CosmereScadrial      - Allomancy, Feruchemy, Hemalurgy"
        Write-Host "  CosmereRoshar        - Surgebinding and Radiant Orders"
        Write-Host ""
        Write-ColoredLine "Tools:" $Green
        Write-Host "  Tools CLI            - Code generation and asset building"
        Write-Host "  Legacy .scripts      - Node.js generators (deprecated)"
        Write-Host ""
        Write-ColoredLine "Common Workflows:" $Green
        Write-Host "  Development:         .\make.ps1 dev"
        Write-Host "  Quick iteration:     .\make.ps1 quick"
        Write-Host "  Full build:          .\make.ps1 all"
        Write-Host "  Clean slate:         .\make.ps1 clean; .\make.ps1 all"
    }
    
    "versions" {
        Write-Section "Tool Versions"
        Write-Host "  .NET: " -NoNewline; dotnet --version
        Write-Host "  PowerShell: " -NoNewline; $PSVersionTable.PSVersion
        try {
            Write-Host "  Node.js: " -NoNewline; node --version
        } catch {
            Write-Host "  Node.js: Not installed"
        }
        Write-Host "  Git: " -NoNewline; git --version
        Write-Host ""
        Write-Section "Unity Information"
        Write-Host "  Target Version: 2022.3.35f1"
        Write-Host "  AssetBuilder: ../AssetBuilder (sibling directory)"
    }
    
    default {
        Write-Warning "Unknown command: $Command"
        Write-Host "Run '.\make.ps1 help' to see available commands."
        exit 1
    }
}