# CosmereFramework Features

## [Settings](../CosmereFramework/Settings)

The cosmere mods all have their own unique settings, and instead of having *each* of them have their own mod settings setup in game, CosmereFramework compiles them all into a single `Cosmere` mod settings window. There are some helpers in here (that we want to expand on) for creating mod settings as well.

## [Extensions](../CosmereFramework/Extension)

There are a bunch of extensions that make working with specific classes a little easier. Check out [Extensions](../CosmereFramework/Extension) to see them all

## [Patches](../CosmereFramework/Patch)

This currently only has one patch, but it fixes the research window so that items with more than 3 prerequisites will display properly in the sidebar

It also does contain a new PatchOperation: UseSetting, that lets you patch conditionally based on a cosmere mod setting:

```xml
<?xml version="1.0" encoding="utf-8"?>
<Patch>
    <Operation Class="CosmereFramework.Patch.Operation.UseSetting">
        <modId>CosmereScadrial</modId>
        <key>pawnsKeepMetalmindsWhenDowned</key>
        <expect>false</expect>
        <apply Class="PatchOperationAdd">
            <xpath>Defs/ThingDef[@Name="Cosmere_Scadrial_Thing_AllomanticVialBase"]/comps/li[@Class="CosmereCore.Comp.Thing.PreventDropOnDownedProperties"]</xpath>
            <value>
                <preventDrop>{pawnsKeepMetalmindsWhenDowned}</preventDrop>
            </value>
        </apply>
    </Operation>
    <Operation Class="CosmereFramework.Patch.Operation.UseSetting">
        <modId>CosmereScadrial</modId>
        <key>pawnsKeepVialsWhenDowned</key>
        <expect>false</expect>
        <apply Class="PatchOperationAdd">
            <xpath>Defs/ThingDef[@Name="Cosmere_Scadrial_Thing_MetalmindBase"]/comps/li[@Class="CosmereCore.Comp.Thing.PreventDropOnDownedProperties"]</xpath>
            <value>
                <preventDrop>{pawnsKeepVialsWhenDowned}</preventDrop>
            </value>
        </apply>
    </Operation>
</Patch>
```

## [Comps](../CosmereFramework/Comps)

There are two helpful renders in here:

* Map/CircleRenderer - Used to draw a circle around a thing, every tick. Scadrial uses this when debug mode is on to render circles around pawns that are burning auras
* Map/LineRenderer - Used to draw a line between two objects. Supports altering thickness, material/color, and alpha. Scadrial uyses this for things like Iron/Steel aura to draw lines to metal items 

Theres also a GiveGene ThingComp, that can be used to give genes when ingesting something.

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

## [Profiler](../CosmereFramework/Profiler.cs)

*Another pretty cool thing*

Slap the [Profile](../CosmereFramework/Attribute/Profile.cs) attribute on a method, and it will log the time it takes to run to the game log

## [UI](../CosmereFramework/UI)

Introduces some new things:

* Padding struct - Helps work with padding in widgets
* Box - A wrapper for creating a Rect with the Padding struct
* DropdownMenu - I'm going to expand on this eventually to make a better dropdown menu