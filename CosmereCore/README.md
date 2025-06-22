# Cosmere Core

*The foundational systems and definitions for all Cosmere-based RimWorld mods.*

---

##  What This Mod Does

This mod lays the groundwork for Investiture across the Cosmere. It introduces key mechanics and systems used by other
Cosmere mods.

Included features:

- **New StatDefs**
    - `MentalBreakAddFactor`: Modifies how likely a pawn is to suffer a mental break
    - `MentalBreakRemoveFactor`: Modifies how likely a pawn is to be calmed from one

- **Investiture Need**  
  A unique pawn need used to power magical abilities across all Shardworlds

- **Investiture Research Project**  
  Unlocks access to investiture-related gameplay

- **Invested Trait**  
  Represents innate investiture, modeled after the Heightenings from Nalthis

- **Shard Selection System**  
  Adds a game component that allows players to configure which Shards are present in their world  
  (Used by mods like Scadrial, Roshar, Nalthis — each will auto-enable based on scenario)

- **Investiture Tab**  
  Adds a new UI tab for tracking each pawn’s Investiture, accessible from their inspect pane

---

## Use & Compatibility

- This is a *required core mod* for all other Cosmere expansions
- Safe to add at game start
- Compatible with RimWorld 1.5 + Biotech, Ideology, Royalty, and Anomaly

---

## For Modders

Cosmere Core defines shared traits, needs, tabs, and stats to ensure consistency across Shardworlds.  
Mods can register new Shards via the provided game component and hook into the Investiture system.

---
