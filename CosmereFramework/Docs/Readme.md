# CosmereFramework Features

## [Extensions](../CosmereFramework/Extension)

There are a bunch of extensions that make working with specific classes a little easier. Check out [Extensions](../CosmereFramework/Extension) to see them all

## [Patches](../CosmereFramework/Patch)

This currently only has one patch, but it fixes the research window so that items with more than 3 prerequisites will display properly in the sidebar

## [Utils](../CosmereFramework/Util)

Two utilities at the moment:

1. [DelayedActionScheduler](../CosmereFramework/Util/DelayedActionScheduler.cs): Schedules an Action to run after x Ticks. Used by the Quickstarter, and Scadrial's mists
2. [UIUtil](../CosmereFramework/Util/UIUtil.cs): Helper methods for UI code. Current only has a method to help generate icon images for Gizmos

## [Quickstart](../CosmereFramework/Quickstart)

*This is probably the coolest thing in here*

[Quickstarter](../CosmereFramework/Quickstart/Quickstarter.cs) is an advanced version of the vanilla -quickstart tool.
Quickstarter searches for all loaded classes that extend [CosmereFramework.Quickstart.AbstractQuickstart](../CosmereFramework/Quickstart/AbstractQuickstart.cs).
When a user has turned on RimWorld's Dev mode, enabled CosmereFramework's debugMode in the mod settings, and specified a Quickstarter in the mod settings, launching the game will
launch the user directly into the selected quickstart. For an example, check out [Scadrial's PreCatacendre Quickstart](../../CosmereScadrial/CosmereScadrial/Quickstart/PreCatacendre.cs).

