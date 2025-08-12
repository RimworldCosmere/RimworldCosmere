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
        Write-Host "  all           - Full build pipeline (clean, generate, build everything)"
        Write-Host "  generatables  - Generate code and build assets only (no solution build)"
        Write-Host "  build         - Build everything (no clean) for IDE run configurations"
        Write-Host "  build-all     - Full build pipeline (alias for all, use with IDE run configurations)"
        Write-Host "  run           - Full build and launch RimWorld"
        Write-Host "  quick         - Quick development cycle (generate + build main solution)"
        Write-Host "  dev           - Development build (generate + build main + build assets)"
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
        Write-Host "  build-main-debug - Build main solution in debug mode"
        Write-Host "  build-tools   - Build Tools CLI"
        Write-Host "  build-tools-debug - Build Tools CLI in debug mode"
        Write-Host "  build-assets  - Build Unity AssetBundles"
        Write-Host "  build-assets-force - Force rebuild all AssetBundles"
        Write-Host "  build-debug   - Build in debug mode"
        
        Write-Section "Development Tools"
        Write-Host "  clean         - Clean build outputs"
        Write-Host "  restore       - Restore NuGet packages"
        Write-Host "  format        - Format C# code"
        Write-Host "  test          - Run tests"
        Write-Host "  lint          - Run linting"
        Write-Host "  precommit     - Pre-commit checks (generate, format, lint, test)"
        Write-Host "  release-prep  - Prepare for release (full build + test)"
        
        Write-Section "Project Management"
        Write-Host "  install-deps  - Install Node.js dependencies (legacy .scripts)"
        Write-Host "  check-deps    - Check for outdated dependencies"
        Write-Host "  status        - Show git status and solution info"
        
        Write-Section "Development Workflow"
        Write-Host "  watch         - Watch for changes and auto-rebuild main solution"
        
        Write-Section "Debugging & Analysis"
        Write-Host "  debug-main    - Build and prepare main solution for debugging"
        Write-Host "  debug-tools   - Build tools in debug mode"
        Write-Host "  profile       - Build with profiling enabled"
        
        Write-Section "Maintenance"
        Write-Host "  clean-all     - Nuclear clean (build outputs + generated files)"
        Write-Host "  rebuild       - Full rebuild from scratch"
        Write-Host "  update-deps   - Update all dependencies"
        
        Write-Section "Legacy Support"
        Write-Host "  legacy-generate - Use legacy Node.js generator"
        Write-Host "  legacy-clean  - Clean using legacy method"
        
        Write-Section "Information"
        Write-Host "  info          - Show project information"
        Write-Host "  versions      - Show version information"
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
    
    "build-main-debug" {
        Write-Info "Building main solution (Debug)..."
        Invoke-DotNet "build Cosmere.sln --configuration Debug --verbosity minimal"
    }
    
    "build-tools-debug" {
        Write-Info "Building Tools solution (Debug)..."
        Invoke-DotNet "build Cosmere.Tools.sln --configuration Debug --verbosity minimal"
    }
    
    "build-assets-force" {
        Write-Info "Force building Unity AssetBundles..."
        powershell.exe -ExecutionPolicy Bypass -File "./buildAllCosmereBundles.ps1"
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
    
    "release-prep" {
        Write-Info "Preparing for release (full build + test)..."
        & $PSCommandPath all
        & $PSCommandPath test
        Write-Success "Release preparation complete!"
    }
    
    "install-deps" {
        Write-Info "Installing Node.js dependencies..."
        Set-Location .scripts
        npm install
        Set-Location ..
        Write-Success "Node.js dependencies installed!"
    }
    
    "check-deps" {
        Write-Info "Checking .NET dependencies..."
        Invoke-DotNet "list Cosmere.sln package --outdated"
        Invoke-DotNet "list Cosmere.Tools.sln package --outdated"
        Write-Info "Checking Node.js dependencies..."
        Set-Location .scripts
        try { npm outdated } catch { }
        Set-Location ..
    }
    
    "watch" {
        Write-Info "Watching for changes... (Ctrl+C to stop)"
        Invoke-DotNet "watch --project CosmereCore/CosmereCore/CosmereCore.csproj build"
    }
    
    "debug-main" {
        Write-Info "Preparing debug build..."
        & $PSCommandPath build-main-debug
        Write-Warning "Debug build ready. Attach debugger to RimWorld process."
    }
    
    "debug-tools" {
        Write-Info "Building tools for debugging..."
        & $PSCommandPath build-tools-debug
    }
    
    "profile" {
        Write-Info "Building with profiling..."
        Invoke-DotNet "build Cosmere.sln --configuration Release -p:DefineConstants=`"PROFILING`" --verbosity minimal"
    }
    
    "clean-all" {
        Write-Warning "Performing nuclear clean..."
        & $PSCommandPath clean
        & $PSCommandPath gen-clean
        Get-ChildItem -Path . -Recurse -Directory -Name "bin" | Remove-Item -Recurse -Force -ErrorAction SilentlyContinue
        Get-ChildItem -Path . -Recurse -Directory -Name "obj" | Remove-Item -Recurse -Force -ErrorAction SilentlyContinue
        Write-Success "Nuclear clean complete!"
    }
    
    "rebuild" {
        Write-Info "Full rebuild from scratch..."
        & $PSCommandPath clean-all
        & $PSCommandPath restore
        & $PSCommandPath generate
        & $PSCommandPath build-main
        & $PSCommandPath build-tools
        Write-Success "Full rebuild complete!"
    }
    
    "update-deps" {
        Write-Info "Updating dependencies..."
        Write-Warning "Run manually: dotnet add package <PackageName>"
        Set-Location .scripts
        npm update
        Set-Location ..
        Write-Success "Node.js dependencies updated!"
    }
    
    "legacy-generate" {
        Write-Warning "Using legacy Node.js generator..."
        Set-Location .scripts
        npm start -- -f -v
        Set-Location ..
    }
    
    "legacy-clean" {
        Write-Warning "Using legacy Node.js clean..."
        Set-Location .scripts
        npm start -- -d -v
        Set-Location ..
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