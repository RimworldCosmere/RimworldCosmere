# RimWorld: Cosmere

A full-conversion mod series bringing Brandon Sanderson’s **Cosmere** universe into RimWorld.  
This project introduces Investiture-based systems like Allomancy, Feruchemy, and more — reimagined in RimWorld’s
gameplay framework.

---

## Project Structure

This is a modular monorepo. Each component exists as its own loadable mod:

### Core Mods

- **[Cosmere Framework](./CosmereFramework)** – Shared C# utilities and base helpers (no content)
- **[Cosmere Core](./CosmereCore)** – Shared stats, traits, needs, and the Investiture system
- **[Cosmere Resources](./CosmereResources)** – Defines all base metals, alloys, and godmetals used across the Cosmere

### Shard World Modules

- **[Cosmere - Scadrial](CosmereScadrial)** – Allomancy, Feruchemy, >!Hemalurgy!<, Mistborn genes, vial systems,
  snapping, Skaa, Nobles, and Terris, and more
- **Cosmere - Roshar** *(Coming Soon)* – Surgebinding, spren bonding, stormlight, Ideals, and many Xenotypes
- **Cosmere - Nalthis** *(Coming Soon)* – Awakening, Breath economy, Commands, Divine Breaths, and the Returned
- **Cosmere - Sel** *(Coming Soon)* – Elantrians, Aon Dor, and Forgery

Each module is optional, but relies on the shared foundation laid by the **Core**, **Framework**, and **Resources**
mods.

---

## Installation

Clone locally, as your Mods directory for development:

```bash
git clone https://github.com/RimworldCosmere/RimworldCosmere.git Mods
```

Enable in the following order:

1. Cosmere Framework
2. Cosmere Resources
3. Cosmere Core
4. Any shard-world mod (e.g., Cosmere - Scadrial)

---

## For Developers

This project uses:

- C# (Harmony patches, XML Defs, custom comps and needs)
- Custom XML and CS generators (`.scripts/`)
- Modular load order system for shardworld-specific features

To Run:

- Set up your environment so `npm --prefix .scripts start -- -d -f` runs before your solution builds (Generated files
  arent kept in source)

If you’d like to contribute:

1. Fork the repo
2. Submit a PR with clear notes and testing info

---

## Status

| Module             | Status      | Notes                                   |
|--------------------|-------------|-----------------------------------------|
| Cosmere Framework  | Stable      | Internal C# helpers only                |
| Cosmere Core       | Stable      | Needed for all content mods             |
| Cosmere Metals     | Stable      | MetalDefs and worldgen integration      |
| Cosmere - Scadrial | In Progress | Allomancy and Feruchemy mostly complete |
| Cosmere - Roshar   | Planned     | Design stage                            |
| Cosmere - Nalthis  | Planned     | Design stage                            |
| Cosmere - Sel      | Planned     | Design stage                            |

---

## Community

- GitHub Issues: Use for bug reports and feature requests
- Discord: https://discord.gg/jTcrKfXdYU

---

## License

This project is open-source but is definitely subject to **Cosmere IP restrictions**. Thank you Brandon Sanderson, and
Dragonsteel for all of your work!
Please do not redistribute standalone modules without credit and attribution.  
This is a fan project not affiliated with Brandon Sanderson or Dragonsteel Entertainment.

Besides adhearing to the above clause, this project has adopted the [MIT License](./License.md).
We do not allow using our code to train LLM Models without express consent.
