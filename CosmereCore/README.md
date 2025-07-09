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
    - `Cosmere_Time_Dilation_Factor`: Used mostly by Scadrial Bendalloy and Cadmium burners. Used to tweak a few things that don't natively have stats (Burn rate of metals for example)
    - `Cosmere_Investiture`: How much investiture a pawn has!

- **Investiture Need**  
  A unique pawn need used to power magical abilities across all Shardworlds

- **Investiture Research Project**  
  Unlocks access to investiture-related gameplay

- **Invested Trait**  
  Represents innate investiture, modeled after the Heightenings from Nalthis

- **Shard Selection System**  
  Adds a game component that allows players to configure which Shards are present in their world  
  (Used by mods like Scadrial, Roshar, Nalthis — each will auto-enable based on scenario)

---

## Use & Compatibility

- This is a *required core mod* for all other Cosmere expansions
- Safe to add at game start
- Requires Biotech (These mods all use too much of the gene and xenotype system to decouple)
- Compatible with RimWorld 1.6 + Biotech, Ideology, Royalty, and Anomaly

---

## For Modders

Cosmere Core defines shared traits, needs, and stats to ensure consistency across Shardworlds.  
Mods can register new Shards via the provided game component and hook into the Investiture system.

---
