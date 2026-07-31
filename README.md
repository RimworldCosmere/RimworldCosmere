# RimWorld: Cosmere

A full-conversion mod series that brings Brandon Sanderson's **Cosmere** universe into RimWorld. It adds
Investiture-based systems like Allomancy, Feruchemy, and Surgebinding, rebuilt to fit how RimWorld already works.

---

## Project Structure

A monorepo. All the C# lives in one assembly, which keeps load and runtime cost down.

### Core Assembly

- **[Cosmere Core](./CosmereCore)** - Single C# assembly (`Cosmere.dll`) holding all shared code:
  - Base framework, utilities, and extension methods
  - Investiture system, stats, traits, needs
  - All metals, alloys, gems, and godmetals
  - World-specific systems (Allomancy, Surgebinding) in `System.{World}` namespaces

### World Modules (Content Only)

- **[Cosmere - Scadrial](CosmereScadrial)** - XML definitions, textures, and assets for Allomancy, Feruchemy, >!Hemalurgy!<, Mistborn genes, vial systems, snapping, Skaa, Nobles, and Terris
- **[Cosmere - Roshar](CosmereRoshar)** - XML definitions, textures, and assets for Surgebinding, spren bonding, Stormlight, Ideals, and Radiant Orders
- **Cosmere - Nalthis** *(Coming Soon)* - Awakening, Breath economy, Commands, Divine Breaths, and the Returned
- **Cosmere - Sel** *(Coming Soon)* - Elantrians, Aon Dor, and Forgery

World modules ship only XML defs and assets. The C# all lives in CosmereCore.

---

## For Developers

Follow [CONTRIBUTING.md](./CONTRIBUTING.md).

If you'd like to contribute:

1. Fork the repo
2. Submit a PR with clear notes and testing info

---

## Status

| Module             | Status      | Notes                                   |
|--------------------|-------------|-----------------------------------------|
| Cosmere Core       | Stable      | Consolidated single assembly, required by every world mod |
| Cosmere - Scadrial | In Progress | Allomancy and Feruchemy mostly complete |
| Cosmere - Roshar   | In Progress | Radiant Orders and Fabrials in progress |
| Cosmere - Nalthis  | Planned     | Design stage                            |
| Cosmere - Sel      | Planned     | Design stage                            |

---

## Community

- GitHub Issues: bug reports and feature requests
- Discord: https://discord.gg/jTcrKfXdYU

---

## License

The code here is open source. The Cosmere is not ours, so **Cosmere IP restrictions apply**. Thank you to Brandon
Sanderson and Dragonsteel for all of your work.

Please do not redistribute standalone modules without credit and attribution. This is a fan project, not affiliated
with Brandon Sanderson or Dragonsteel Entertainment.

On top of that clause, the repo is dual licensed:

- **Code**: All source code is under the [MIT License](LICENSE.md)
- **Assets**: All art, images, audio, and creative assets are under [CC BY-SA 4.0](LICENSE-ASSETS.md)

We do not permit the use of our code to train LLM models without express consent.

### Quick Reference

| Content Type                   | License      | Attribution Required | Commercial Use |
|--------------------------------|--------------|----------------------|----------------|
| Source code (*.cs, *.js, etc.) | MIT          | Yes (keep license)   | Allowed        |
| Images, sprites, textures      | CC BY-SA 4.0 | Yes (credit author)  | Allowed        |
| Audio, music, sound effects    | CC BY-SA 4.0 | Yes (credit author)  | Allowed        |
| 3D models, animations          | CC BY-SA 4.0 | Yes (credit author)  | Allowed        |
| Documentation, text            | MIT          | Yes (keep license)   | Allowed        |
