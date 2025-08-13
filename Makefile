# RimWorld Cosmere - Development Makefile
# Provides convenient shortcuts for common development tasks
#
# Usage:
#   make help     - Show available commands
#   make all      - Full build (clean, generate, build both solutions, build assets)
#   make quick    - Quick development cycle (generate + build main solution)

.PHONY: help all quick clean generate build-main build-tools build-assets test restore format lint watch dev setup install-deps check-deps status

# Default target
.DEFAULT_GOAL := help

# Colors for output
GREEN := \033[0;32m
BLUE := \033[0;34m
YELLOW := \033[0;33m
RED := \033[0;31m
NC := \033[0m # No Color

##@ General Commands

help: ## Display this help message
	@echo "$(BLUE)RimWorld Cosmere Development Makefile$(NC)"
	@echo ""
	@awk 'BEGIN {FS = ":.*##"; printf "Usage: make $(YELLOW)<target>$(NC)\n\n"} /^[a-zA-Z_0-9-]+:.*?##/ { printf "  $(GREEN)%-15s$(NC) %s\n", $$1, $$2 } /^##@/ { printf "\n$(BLUE)%s$(NC)\n", substr($$0, 5) } ' $(MAKEFILE_LIST)

all: clean generate build-main build-tools build-assets ## Full build pipeline (clean, generate, build everything)
	@echo "$(GREEN)✓ Full build complete!$(NC)"

generatables: generate build-assets ## Generate code and build assets only (no solution build)
	@echo "$(GREEN)✓ Generatables complete!$(NC)"

build: generate build-main build-assets ## Build everything (no clean) for IDE run configurations
	@echo "$(GREEN)✓ Build complete - ready for launch!$(NC)"

build-all: clean generate build-main build-tools build-assets ## Full build pipeline (alias for all, use with IDE run configurations)
	@echo "$(GREEN)✓ Full build complete - ready for launch!$(NC)"

run: all ## Full build and launch RimWorld
	@echo "$(GREEN)Launching RimWorld...$(NC)"
	@powershell -Command "$$steamPath = 'C:\\Program Files (x86)\\Steam\\steamapps\\common\\RimWorld\\RimWorldWin64.exe'; $$epicPath = 'C:\\Program Files\\Epic Games\\RimWorld\\RimWorldWin64.exe'; if (Test-Path $$steamPath) { Start-Process $$steamPath } elseif (Test-Path $$epicPath) { Start-Process $$epicPath } else { Write-Host 'RimWorld not found in common locations. Please launch manually.' -ForegroundColor Yellow }"

quick: generate build-main ## Quick development cycle (generate + build main solution)
	@echo "$(GREEN)✓ Quick build complete!$(NC)"

dev: quick build-assets ## Development build (generate + build main + build assets)
	@echo "$(GREEN)✓ Development build complete!$(NC)"

setup: install-deps restore ## Initial project setup
	@echo "$(GREEN)✓ Project setup complete!$(NC)"

##@ Code Generation

generate: ## Generate all code using Tools CLI
	@echo "$(BLUE)Generating all code...$(NC)"
	@dotnet run --project tools/Cosmere.Tools.Cli -- generate --verbose

generate-clean: ## Clean all generated files
	@echo "$(BLUE)Cleaning generated files...$(NC)"
	@dotnet run --project tools/Cosmere.Tools.Cli -- generate --clean --verbose

generate-dry: ## Show what would be generated (dry run)
	@echo "$(BLUE)Dry run generation...$(NC)"
	@dotnet run --project tools/Cosmere.Tools.Cli -- generate --dry-run --verbose

generate-force: ## Force regenerate all files
	@echo "$(BLUE)Force generating all code...$(NC)"
	@dotnet run --project tools/Cosmere.Tools.Cli -- generate --force --verbose

##@ Code Generation (Specific)

gen-metals: ## Generate only Resources (metals/gems)
	@echo "$(BLUE)Generating Resources...$(NC)"
	@dotnet run --project tools/Cosmere.Tools.Cli -- generate Resources --verbose

gen-genes: ## Generate only GenesAndTraits
	@echo "$(BLUE)Generating GenesAndTraits...$(NC)"
	@dotnet run --project tools/Cosmere.Tools.Cli -- generate GenesAndTraits --verbose

gen-hediffs: ## Generate only FeruchemicalHediffs
	@echo "$(BLUE)Generating FeruchemicalHediffs...$(NC)"
	@dotnet run --project tools/Cosmere.Tools.Cli -- generate FeruchemicalHediffs --verbose

gen-metallic: ## Generate only MetallicArtsMetals
	@echo "$(BLUE)Generating MetallicArtsMetals...$(NC)"
	@dotnet run --project tools/Cosmere.Tools.Cli -- generate MetallicArtsMetals --verbose

gen-roshar: ## Generate only SurgesAndOrders
	@echo "$(BLUE)Generating SurgesAndOrders...$(NC)"
	@dotnet run --project tools/Cosmere.Tools.Cli -- generate SurgesAndOrders --verbose

##@ Building

build-main: ## Build main Cosmere solution
	@echo "$(BLUE)Building main solution...$(NC)"
	@dotnet build Cosmere.sln --configuration Release --verbosity minimal

build-main-debug: ## Build main solution in debug mode
	@echo "$(BLUE)Building main solution (Debug)...$(NC)"
	@dotnet build Cosmere.sln --configuration Debug --verbosity minimal

build-tools: ## Build Tools CLI solution
	@echo "$(BLUE)Building Tools solution...$(NC)"
	@dotnet build Cosmere.Tools.sln --configuration Release --verbosity minimal

build-tools-debug: ## Build Tools CLI in debug mode
	@echo "$(BLUE)Building Tools solution (Debug)...$(NC)"
	@dotnet build Cosmere.Tools.sln --configuration Debug --verbosity minimal

build-assets: ## Build Unity AssetBundles
	@echo "$(BLUE)Building Unity AssetBundles...$(NC)"
	@dotnet script ./scripts/build-assets.csx

build-assets-force: ## Force rebuild all AssetBundles
	@echo "$(BLUE)Force building Unity AssetBundles...$(NC)"
	@dotnet script ./scripts/build-assets.csx -- --force

##@ Development Tools

clean: ## Clean all build outputs
	@echo "$(BLUE)Cleaning build outputs...$(NC)"
	@dotnet clean Cosmere.sln --verbosity minimal
	@dotnet clean Cosmere.Tools.sln --verbosity minimal

restore: ## Restore NuGet packages
	@echo "$(BLUE)Restoring packages...$(NC)"
	@dotnet restore Cosmere.sln --verbosity minimal
	@dotnet restore Cosmere.Tools.sln --verbosity minimal

format: ## Format all C# code
	@echo "$(BLUE)Formatting code...$(NC)"
	@dotnet format Cosmere.sln --include "**/*.cs" --verbosity minimal
	@dotnet format Cosmere.Tools.sln --include "**/*.cs" --verbosity minimal

test: ## Run all tests
	@echo "$(BLUE)Running tests...$(NC)"
	@dotnet test Cosmere.sln --configuration Release --logger "console;verbosity=minimal"

##@ Project Management

install-deps: ## Install Node.js dependencies (legacy .scripts)
	@echo "$(BLUE)Installing Node.js dependencies...$(NC)"
	@cd .scripts && npm install

check-deps: ## Check for outdated dependencies
	@echo "$(BLUE)Checking .NET dependencies...$(NC)"
	@dotnet list Cosmere.sln package --outdated
	@dotnet list Cosmere.Tools.sln package --outdated
	@echo "$(BLUE)Checking Node.js dependencies...$(NC)"
	@cd .scripts && npm outdated || true

status: ## Show git status and solution info
	@echo "$(BLUE)Git Status:$(NC)"
	@git status --short --branch
	@echo ""
	@echo "$(BLUE)Solutions:$(NC)"
	@echo "Main: Cosmere.sln"
	@echo "Tools: Cosmere.Tools.sln"
	@echo ""
	@echo "$(BLUE)Recent commits:$(NC)"
	@git log --oneline -5

##@ Development Workflow

watch: ## Watch for changes and auto-rebuild main solution
	@echo "$(BLUE)Watching for changes... (Ctrl+C to stop)$(NC)"
	@dotnet watch --project CosmereCore/CosmereCore/CosmereCore.csproj build

lint: ## Run linting checks
	@echo "$(BLUE)Running linting...$(NC)"
	@dotnet format Cosmere.sln --verify-no-changes --verbosity minimal
	@dotnet format Cosmere.Tools.sln --verify-no-changes --verbosity minimal

precommit: generate format lint test ## Pre-commit checks (generate, format, lint, test)
	@echo "$(GREEN)✓ Pre-commit checks passed!$(NC)"

release-prep: all test ## Prepare for release (full build + test)
	@echo "$(GREEN)✓ Release preparation complete!$(NC)"

##@ Debugging & Analysis

debug-main: ## Build and prepare main solution for debugging
	@echo "$(BLUE)Preparing debug build...$(NC)"
	@$(MAKE) build-main-debug
	@echo "$(YELLOW)Debug build ready. Attach debugger to RimWorld process.$(NC)"

debug-tools: ## Build tools in debug mode
	@echo "$(BLUE)Building tools for debugging...$(NC)"
	@$(MAKE) build-tools-debug

profile: ## Build with profiling enabled
	@echo "$(BLUE)Building with profiling...$(NC)"
	@dotnet build Cosmere.sln --configuration Release -p:DefineConstants="PROFILING" --verbosity minimal

##@ Maintenance

clean-all: clean generate-clean ## Nuclear clean (build outputs + generated files)
	@echo "$(YELLOW)Performing nuclear clean...$(NC)"
	@find . -name "bin" -type d -exec rm -rf {} + 2>/dev/null || true
	@find . -name "obj" -type d -exec rm -rf {} + 2>/dev/null || true
	@echo "$(GREEN)✓ Nuclear clean complete!$(NC)"

rebuild: clean-all restore generate build-main build-tools ## Full rebuild from scratch
	@echo "$(GREEN)✓ Full rebuild complete!$(NC)"

update-deps: ## Update all dependencies
	@echo "$(BLUE)Updating dependencies...$(NC)"
	@dotnet add Cosmere.sln package --help >/dev/null 2>&1 || echo "$(YELLOW)Run manually: dotnet add package <PackageName>$(NC)"
	@cd .scripts && npm update

##@ Legacy Support

legacy-generate: ## Use legacy Node.js generator
	@echo "$(YELLOW)Using legacy Node.js generator...$(NC)"
	@cd .scripts && npm start -- -f -v

legacy-clean: ## Clean using legacy method
	@echo "$(YELLOW)Using legacy Node.js clean...$(NC)"
	@cd .scripts && npm start -- -d -v

##@ Information

info: ## Show project information
	@echo "$(BLUE)RimWorld Cosmere Development Environment$(NC)"
	@echo ""
	@echo "$(GREEN)Project Structure:$(NC)"
	@echo "  CosmereFoundation    - Shared utilities and base classes"
	@echo "  CosmereCore          - Core game mechanics and systems"
	@echo "  CosmereResources     - Metals, gems, and materials"
	@echo "  CosmereScadrial      - Allomancy, Feruchemy, Hemalurgy"
	@echo "  CosmereRoshar        - Surgebinding and Radiant Orders"
	@echo ""
	@echo "$(GREEN)Tools:$(NC)"
	@echo "  Tools CLI            - Code generation and asset building"
	@echo "  Legacy .scripts      - Node.js generators (deprecated)"
	@echo ""
	@echo "$(GREEN)Common Workflows:$(NC)"
	@echo "  Development:         make dev"
	@echo "  Quick iteration:     make quick"
	@echo "  Full build:          make all"
	@echo "  Clean slate:         make rebuild"

versions: ## Show version information
	@echo "$(BLUE)Tool Versions:$(NC)"
	@echo -n "  .NET: " && dotnet --version
	@echo -n "  Node.js: " && node --version 2>/dev/null || echo "Not installed"
	@echo -n "  PowerShell: " && pwsh --version 2>/dev/null | head -1 || echo "Not installed"
	@echo -n "  Git: " && git --version
	@echo ""
	@echo "$(BLUE)Unity Information:$(NC)"
	@echo "  Target Version: 2022.3.35f1"
	@echo "  AssetBundleBuilder: CryptikLemur.AssetBundleBuilder (dotnet tool)"