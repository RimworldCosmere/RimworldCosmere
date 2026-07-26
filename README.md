# RimWorld: Cosmere

A full-conversion mod series bringing Brandon Sanderson’s **Cosmere** universe into RimWorld.  
This project introduces Investiture-based systems like Allomancy, Feruchemy, and more — reimagined in RimWorld’s
gameplay framework.

---

## Project Structure

This is a modular monorepo with a consolidated single-assembly architecture for improved performance.

### Core Assembly

- **[Cosmere Core](./CosmereCore)** – Single C# assembly (`Cosmere.dll`) containing all shared code:
  - Base framework, utilities, and extension methods
  - Investiture system, stats, traits, needs
  - All metals, alloys, gems, and godmetals
  - World-specific systems (Allomancy, Surgebinding) in `System.{World}` namespaces

### World Modules (Content Only)

- **[Cosmere - Scadrial](CosmereScadrial)** – XML definitions, textures, and assets for Allomancy, Feruchemy, >!Hemalurgy!<, Mistborn genes, vial systems, snapping, Skaa, Nobles, and Terris
- **[Cosmere - Roshar](CosmereRoshar)** – XML definitions, textures, and assets for Surgebinding, spren bonding, stormlight, Ideals, and Radiant Orders
- **Cosmere - Nalthis** *(Coming Soon)* – Awakening, Breath economy, Commands, Divine Breaths, and the Returned
- **Cosmere - Sel** *(Coming Soon)* – Elantrians, Aon Dor, and Forgery

World modules contain only XML defs and assets. All C# code is consolidated in CosmereCore for performance.

---

## For Developers

Follow [CONTRIBUTING.md](./CONTRIBUTING.md).

If you’d like to contribute:

1. Fork the repo
2. Submit a PR with clear notes and testing info

---

## Status

| Module             | Status      | Notes                                   |
|--------------------|-------------|-----------------------------------------|
| Cosmere Core       | Stable      | Consolidated single assembly - required for all world mods |
| Cosmere - Scadrial | In Progress | Allomancy and Feruchemy mostly complete |
| Cosmere - Roshar   | In Progress | Radiant Orders and Fabrials in progress |
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

Besides adhering to the above clause, this project has adopted a dual license:

This project uses dual licensing:

- **Code**: All source code is licensed under the [MIT License](LICENSE.md)
- **Assets**: All art, images, audio, and creative assets are licensed under [CC BY-SA 4.0](LICENSE-ASSETS.md)

We do not permit the use of our code to train LLM Models without express consent.

### Quick Reference

| Content Type | License      | Attribution Required | Commercial Use | 
|-------------|--------------|---------------------|----------------|
| Source code (*.cs, *.js, etc.) | MIT          | Yes (keep license) | ✅ Allowed |
| Images, sprites, textures | CC BY-SA 4.0 | Yes (credit author) | ✅ Allowed |
| Audio, music, sound effects | CC-SA BY 4.0 | Yes (credit author) | ✅ Allowed |
| 3D models, animations | CC BY-SA 4.0 | Yes (credit author) | ✅ Allowed |
| Documentation, text | MIT          | Yes (keep license) | ✅ Allowed |
