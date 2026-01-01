# Corto – Unity Deckbuilding Framework

Corto is an extensible deckbuilding framework for Unity 6 (6000.2.9f1), built with the built‑in 2D pipeline. It targets developers who want a clean, reusable foundation for Slay‑the‑Spire‑style or lighter card systems.

## Goals

- Provide a clear, data‑driven card model (ScriptableObjects) that is easy to extend.
- Offer plug‑and‑play systems for deck, hand, draw, discard, and exhaust piles.
- Expose well‑documented extension points for custom effects and combat logic.
- Include editor tools that make card and deck authoring efficient for designers.

## Features (planned)

- Card data layer: cost, type, tags, rarity, effects, targeting, and VFX hooks.
- Deck and draw systems with configurable mulligan/shuffle rules.
- Event‑driven effect execution (e.g., damage, block, status, keywords).
- Basic 2D demo scene showcasing a turn‑based combat loop.
- Inspector and editor tooling for creating and validating card sets.

## Tech

- Unity 6000.2.9f1 (2D, Built‑in Render Pipeline).
- C# scripts organized for framework reuse (minimal project‑specific code).
- No external dependencies beyond standard Unity packages.

## Getting started

1. Clone the repo.
2. Open the project in Unity 6000.2.9f1 or later.
3. Open the `Demo` scene (once available) and press Play.
4. Browse the `Corto` folder for framework code, data, and editor tools.

## Roadmap

- v0.1: Core card data model and minimal demo loop.
- v0.2: Deck/hand systems and basic UI.
- v0.3: Editor tooling and validation.
- v0.4+: Balancing helpers and AI‑assisted content workflows.

## License

TBD.