*The shared framework every other Cosmere mod is built on.*

​

![About](../.github/assets/fallback/about.png)

Cosmere Core carries the Investiture plumbing the rest of the Cosmere mods sit on. On its own it gives you the
framework, not the magic. Allomancy, Surgebinding, and the rest arrive with their own Shardworld mods.

Included features:

- **New StatDefs**
    - `MentalBreakAddFactor`: Changes how likely a pawn is to suffer a mental break
    - `MentalBreakRemoveFactor`: Changes how likely a pawn is to be calmed out of one
    - `Cosmere_Time_Dilation_Factor`: Used mostly by Scadrial's Bendalloy and Cadmium burners, for tuning things
      that have no stat of their own (metal burn rate, for one)
    - `Cosmere_Investiture`: How much Investiture a pawn holds

- **Investiture Need**  
  A pawn need that powers Invested abilities across every Shardworld

- **Investiture Research Project**  
  Opens up Investiture-related gameplay

- **Invested Trait**  
  Innate Investiture, modeled on the Heightenings from Nalthis

- **Shard Selection System**  
  A game component for picking which Shards exist in your world  
  (Scadrial, Roshar, and Nalthis each switch themselves on based on the scenario you pick)

​

![Compatibility](../.github/assets/fallback/compatibility.png)

- Every other Cosmere expansion requires this mod
- Safe to add at game start
- Requires Biotech. The gene and xenotype systems run through everything here, so there is no pulling them apart
- Built for RimWorld 1.6, and works alongside Biotech, Ideology, Royalty, and Anomaly

​

![For Modders](../.github/assets/fallback/for modders.png)

Core owns the shared traits, needs, and stats so every Shardworld reads the same.  
Register a new Shard through the game component and hook into the Investiture system.

​
