# RimWorld Cosmere - Development Makefile
# Provides convenient shortcuts for common development tasks
#
# Usage:
#   make help     - Show available commands
#   make all      - Full build (clean, generate, build both solutions, build assets)
#   make quick    - Quick development cycle (generate + build main solution)

.PHONY: help all quick clean generate build-main build-tools build-assets test restore format lint watch dev setup install-deps check-deps status sonar sonar-up sonar-down

# Use bash with xpg_echo so `echo` interprets \033 escape sequences
# (default /bin/sh on many distros is dash, which prints them literally)
SHELL := /bin/bash
.SHELLFLAGS := -O xpg_echo -c

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
	@dotnet build Cosmere.slnx --configuration Release --verbosity minimal

build-main-debug: ## Build main solution in debug mode
	@echo "$(BLUE)Building main solution (Debug)...$(NC)"
	@dotnet build Cosmere.slnx --configuration Debug --verbosity minimal

build-tools: ## Build Tools CLI solution
	@echo "$(BLUE)Building Tools solution...$(NC)"
	@dotnet build Cosmere.Tools.slnx --configuration Release --verbosity minimal

build-tools-debug: ## Build Tools CLI in debug mode
	@echo "$(BLUE)Building Tools solution (Debug)...$(NC)"
	@dotnet build Cosmere.Tools.slnx --configuration Debug --verbosity minimal

build-assets: ## Build Unity AssetBundles for current platform
	@echo "$(BLUE)Building Unity AssetBundles...$(NC)"
	@dotnet script ./scripts/build-assets.csx

build-assets-verbose: ## Build Unity AssetBundles with verbose output
	@echo "$(BLUE)Building Unity AssetBundles (verbose)...$(NC)"
	@dotnet script ./scripts/build-assets.csx -- --verbose

build-assets-all: ## Build Unity AssetBundles for all platforms (Windows, Mac, Linux)
	@echo "$(BLUE)Building Unity AssetBundles for all platforms...$(NC)"
	@echo "  Building for Windows..."
	UNITY_BUILD_TARGET=windows dotnet script ./scripts/build-assets.csx
	@echo "  Building for macOS..."
	UNITY_BUILD_TARGET=mac dotnet script ./scripts/build-assets.csx
	@echo "  Building for Linux..."
	UNITY_BUILD_TARGET=linux dotnet script ./scripts/build-assets.csx
	@echo "$(GREEN)✓ All platform bundles built!$(NC)"

build-assets-all-verbose: ## Build Unity AssetBundles for all platforms with verbose output (for CI)
	@echo "$(BLUE)Building Unity AssetBundles for all platforms (verbose)...$(NC)"
	@echo "  Building for Windows..."
	UNITY_BUILD_TARGET=windows dotnet script ./scripts/build-assets.csx -- --verbose
	@echo "  Building for macOS..."
	UNITY_BUILD_TARGET=mac dotnet script ./scripts/build-assets.csx -- --verbose
	@echo "  Building for Linux..."
	UNITY_BUILD_TARGET=linux dotnet script ./scripts/build-assets.csx -- --verbose
	@echo "$(GREEN)✓ All platform bundles built!$(NC)"

build-assets-force: ## Force rebuild all AssetBundles for current platform
	@echo "$(BLUE)Force building Unity AssetBundles...$(NC)"
	@dotnet script ./scripts/build-assets.csx -- --force

##@ Development Tools

clean: ## Clean all build outputs
	@echo "$(BLUE)Cleaning build outputs...$(NC)"
	@dotnet clean Cosmere.slnx --verbosity minimal
	@dotnet clean Cosmere.Tools.slnx --verbosity minimal

restore: ## Restore NuGet packages
	@echo "$(BLUE)Restoring packages...$(NC)"
	@dotnet restore Cosmere.slnx --verbosity minimal
	@dotnet restore Cosmere.Tools.slnx --verbosity minimal

format: ## Format all C# code
	@echo "$(BLUE)Formatting code...$(NC)"
	@dotnet format Cosmere.slnx --include "**/*.cs" --verbosity minimal
	@dotnet format Cosmere.Tools.slnx --include "**/*.cs" --verbosity minimal

test: ## Run all tests
	@echo "$(BLUE)Running tests...$(NC)"
	@dotnet test Cosmere.slnx --configuration Release --logger "console;verbosity=minimal"

##@ Project Management

install-deps: ## Install Node.js dependencies
	@echo "$(BLUE)Installing Node.js dependencies...$(NC)"
	@npm install

check-deps: ## Check for outdated dependencies
	@echo "$(BLUE)Checking .NET dependencies...$(NC)"
	@dotnet list Cosmere.slnx package --outdated
	@dotnet list Cosmere.Tools.slnx package --outdated
	@echo "$(BLUE)Checking Node.js dependencies...$(NC)"
	@npm outdated || true

status: ## Show git status and solution info
	@echo "$(BLUE)Git Status:$(NC)"
	@git status --short --branch
	@echo ""
	@echo "$(BLUE)Solutions:$(NC)"
	@echo "Main: Cosmere.slnx"
	@echo "Tools: Cosmere.Tools.slnx"
	@echo ""
	@echo "$(BLUE)Recent commits:$(NC)"
	@git log --oneline -5

##@ Development Workflow

watch: ## Watch for changes and auto-rebuild main solution
	@echo "$(BLUE)Watching for changes... (Ctrl+C to stop)$(NC)"
	@dotnet watch --project CosmereCore/CosmereCore/CosmereCore.csproj build

lint: ## Run linting checks
	@echo "$(BLUE)Running linting...$(NC)"
	@dotnet format Cosmere.slnx --verify-no-changes --verbosity minimal
	@dotnet format Cosmere.Tools.slnx --verify-no-changes --verbosity minimal

precommit: generate format lint test ## Pre-commit checks (generate, format, lint, test)
	@echo "$(GREEN)✓ Pre-commit checks passed!$(NC)"

release-prep: all test ## Prepare for release (full build + test)
	@echo "$(GREEN)✓ Release preparation complete!$(NC)"

##@ Code Quality (SonarQube)

# The scanner is a .NET client, so it cannot connect to 0.0.0.0 even though the
# container binds there. Override SONAR_HOST on another machine.
SONAR_HOST ?= http://10.10.30.233:9000
SONAR_KEY ?= rimworld-cosmere
SONAR_EXCLUSIONS := **/*.generated.*,**/Assemblies/**,**/AssetBundles/**,**/Assets/**,**/obj/**,**/bin/**

sonar-up: ## Start the local SonarQube container
	@echo "$(BLUE)Starting SonarQube...$(NC)"
	@docker start sonarqube 2>/dev/null || docker run -d --name sonarqube \
		-p 0.0.0.0:9000:9000 \
		-v sonarqube_data:/opt/sonarqube/data \
		-v sonarqube_logs:/opt/sonarqube/logs \
		-v sonarqube_extensions:/opt/sonarqube/extensions \
		--restart unless-stopped sonarqube:community
	@echo "$(YELLOW)Waiting for SonarQube to come up...$(NC)"
	@until curl -s $(SONAR_HOST)/api/system/status 2>/dev/null | grep -q '"status":"UP"'; do sleep 5; done
	@echo "$(GREEN)✓ SonarQube up at $(SONAR_HOST)$(NC)"

sonar-down: ## Stop the local SonarQube container
	@docker stop sonarqube >/dev/null 2>&1 || true
	@echo "$(GREEN)✓ SonarQube stopped$(NC)"

sonar: ## Analyse the solution and upload to SonarQube (needs SONAR_TOKEN)
	@if [ -z "$$SONAR_TOKEN" ]; then \
		echo "$(RED)SONAR_TOKEN is not set.$(NC)"; \
		echo "Generate one at $(SONAR_HOST)/account/security then:"; \
		echo "  export SONAR_TOKEN=squ_..."; \
		exit 1; \
	fi
	@$(MAKE) sonar-up
	@echo "$(BLUE)Analysing $(SONAR_KEY)...$(NC)"
	@dotnet sonarscanner begin \
		/k:"$(SONAR_KEY)" \
		/d:sonar.host.url="$(SONAR_HOST)" \
		/d:sonar.token="$$SONAR_TOKEN" \
		/d:sonar.exclusions="$(SONAR_EXCLUSIONS)"
	@dotnet build Cosmere.slnx
	@dotnet sonarscanner end /d:sonar.token="$$SONAR_TOKEN"
	@echo "$(GREEN)✓ $(SONAR_HOST)/dashboard?id=$(SONAR_KEY)$(NC)"

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
	@dotnet build Cosmere.slnx --configuration Release -p:DefineConstants="PROFILING" --verbosity minimal

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
	@dotnet add Cosmere.slnx package --help >/dev/null 2>&1 || echo "$(YELLOW)Run manually: dotnet add package <PackageName>$(NC)"
	@npm update

##@ Legacy Support

legacy-generate: ## Use legacy Node.js generator
	@echo "$(YELLOW)Using legacy Node.js generator...$(NC)"
	@npm start -- -f -v

legacy-clean: ## Clean using legacy method
	@echo "$(YELLOW)Using legacy Node.js clean...$(NC)"
	@npm start -- -d -v

##@ Information

info: ## Show project information
	@echo "$(BLUE)RimWorld Cosmere Development Environment$(NC)"
	@echo ""
	@echo "$(GREEN)Project Structure:$(NC)"
	@echo "  CosmereCore          - Single consolidated C# assembly (Cosmere.Core.dll)"
	@echo "                         Contains all systems: framework, core mechanics, Scadrial, Roshar"
	@echo "  CosmereScadrial      - XML defs and assets for Allomancy, Feruchemy, Hemalurgy"
	@echo "  CosmereRoshar        - XML defs and assets for Surgebinding and Radiant Orders"
	@echo ""
	@echo "$(GREEN)Tools:$(NC)"
	@echo "  Tools CLI            - Code generation and asset building"
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