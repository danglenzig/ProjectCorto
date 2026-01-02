# Cards System Overview

This folder contains a small, data‑driven card framework built around ScriptableObjects and strict separation of data vs runtime behavior.[^1][^2]

Core ideas:

- Cards are identified by GUID strings (CardID) and live as `CardSO` assets.
- Decks store **only card IDs** at runtime.
- A `CardCatalogSO` acts as a registry that maps IDs → `CardSO`.
- Card effects are defined as `CardEffectSO` assets that create runtime `IEffectCommand` instances.


## ScriptableObject Types

### CardSO

Represents a single card definition.[^1]

Key fields:

- `cardID` (string, GUID) – unique identifier, auto‑generated in `OnValidate` if empty.
- `displayName` (string) – name shown in UI.
- `description` (string, multiline) – rules text / flavor.
- `effects` (`List<CardEffectSO>`) – effect assets this card will trigger.
- `tags` (`List<EnumCardTag>`) – simple typed tags (ATTACK, DEFENSE, …).
- `customTags` (`List<string>`) – freeform tags.
- `targetingMode` (`EnumTargetingMode`) – how this card selects targets.

Public API:

- `CardID`, `DisplayName`, `Description`.
- `Effects`, `Tags`, `CustomTags` (all exposed as `IReadOnlyList<>`).
- `TargetingMode`.

Behavior:

- `OnValidate` assigns a new GUID to `cardID` when blank, ensuring uniqueness in the catalog.[^3][^4]


### CardEffectSO + IEffectCommand

Defines data for an effect and acts as a factory for runtime commands.[^5][^1]

Supporting types:

- `CardContext` – simple data carrier used at execution time:
    - `SourceEntityID` – ID of the entity playing the card.
    - `TargetEntityID` – ID of the primary target.
    - Later: a reference to an encounter API.
- `IEffectCommand` – runtime interface:
    - `void Execute(CardContext context)`.

Base class:

```csharp
public abstract class CardEffectSO : ScriptableObject
{
    [SerializeField] private string effectID;
    [SerializeField] private string effectDescription;

    public string EffectID => effectID;
    public string EffectDescription => effectDescription;

    public abstract IEffectCommand CreateRuntimeCommand();

    private void OnValidate()
    {
        if (string.IsNullOrEmpty(effectID))
        {
            effectID = System.Guid.NewGuid().ToString();
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }
    }
}
```

Concrete examples:

#### DamageEffect

- Data: `damageAmount` (int).
- Runtime: nested `DamageEffectCommand` holds `damageAmount` and implements `Execute` using `CardContext` (later via an `EncounterController`).[^6][^7]

Key pattern:

```csharp
public override IEffectCommand CreateRuntimeCommand()
{
    return new DamageEffectCommand(damageAmount);
}
```


#### DefenseEffect

- Data: `defenseAmount` (int).
- Runtime: similar nested command class; will later apply defense via the encounter API.


### DeckSO

Represents a **list of card IDs** that make up a deck.

Serialized fields:

- `cardSOs` (`List<CardSO>`) – **editor‑only** convenience list for configuring decks via drag‑and‑drop.
- `cardIDs` (`List<string>`, hidden in inspector) – **runtime** list of IDs synchronized from `cardSOs`.

Behavior:

- `OnValidate`:
    - Ensures both lists are non‑null.
    - Clears `cardIDs` and repopulates it from `cardSOs`’s `CardID` values (ignoring null entries).
    - Marks the asset dirty in the editor so changes are saved.[^8][^9]

Public API:

- `IReadOnlyList<string> CardIDs` – the runtime ID list.
- `AddCardID(string)` – appends an ID.
- `RemoveCardID(string)` – removes; logs an error if the ID isn’t present.
- `ClearDeck()` – clears the ID list.
- `ReplaceDeck(IReadOnlyList<string>)` – replaces the list contents.

Design note: runtime systems **never** touch `cardSOs`; they work only with `CardIDs`.

### CardCatalogSO

Acts as a registry mapping `CardID` → `CardSO` to decouple decks/save data from asset references.[^10][^11]

Serialized field:

- `cardSOs` (`List<CardSO>`) – all card definitions that belong to this catalog.

Runtime cache:

- `cardDict` (`Dictionary<string, CardSO>`) – built from `cardSOs`.

Behavior:

- `OnEnable` → calls `BuildDictionary()` so the cache exists in builds.
- `OnValidate` → also calls `BuildDictionary()` and marks dirty for editor updates.[^12][^8]
- `BuildDictionary()`:
    - Ensures lists/dictionary exist.
    - Clears the dictionary.
    - Iterates `cardSOs`, skipping nulls and blank IDs.
    - Adds each `CardSO` under its `CardID`.
    - Logs a warning when duplicate IDs are found and skips the duplicates.

Public API:

- `CardSO GetCardOrNull(string cardID)` – returns `null` if the ID is not present (no exception), using `TryGetValue` under the hood.[^13][^14]


### Enums

- `EnumCardTag` – typed tags for filtering and synergies (e.g., ATTACK, DEFENSE).
- `EnumTargetingMode` – describes how the card targets entities:
    - `SINGLE_COMBATANT`, `COMBATANT_PARTY`, `SINGLE_ENEMY`, `ENEMY_PARTY`.

These enums are purely data; encounter logic will interpret them later.

***

## How the System Fits Together

High‑level flow for playing a card in combat:

1. Player selects a card from their hand UI.
2. Player selects a target entity.
3. The encounter controller receives `cardID`, `SourceEntityID`, `TargetEntityID`.
4. Controller uses `CardCatalogSO` to get the `CardSO` for that `cardID`.
5. For each `CardEffectSO` in `cardSO.Effects`:
    - Create a command: `var cmd = effectSO.CreateRuntimeCommand();`
    - Build a `CardContext` and call `cmd.Execute(context)`.

This keeps card data in assets, deck composition as IDs, and behavior in small, testable commands.[^6][^1]

***

## How to Create a Player Deck (Step‑by‑Step)

This is the minimal workflow to set up cards, a catalog, and a player deck, plus a simple runtime debug check.

### 1) Create Card Effects

1. Right‑click in your project:
`Create → Cards → Effects → Damage Effect`.
    - Set `Effect Description` (e.g., “This is a damage effect.”).
    - Set `Damage Amount` to some value.
2. Create a `Defense Effect`:
`Create → Cards → Effects → Defense Effect`.
    - Set `Effect Description` (e.g., “This is a defense effect.”).
    - Set `Defense Amount` as appropriate.

Guid: `EffectID` will auto‑populate in the inspector when you first save/validate the asset.

### 2) Create Card Assets

For each card you want in the game:

1. Right‑click: `Create → Cards → Card`.
2. Fill out fields:
    - `Display Name` (e.g., “Basic Attack”, “Heavy Attack”, “Block”).
    - `Description` (rules text).
    - `Targeting Mode` (e.g., `OPPONENT` equivalent: `SINGLE_ENEMY`).
3. Add effects:
    - Drag one or more `CardEffectSO` assets into the `Effects` list.
4. Add tags:
    - Choose from `EnumCardTag` (e.g., ATTACK or DEFENSE).
    - Optionally add strings to `CustomTags`.
5. Let `OnValidate` generate a unique `CardID` (leave `cardID` blank initially).

Repeat for each card you want in the deck.

### 3) Create a Card Catalog

1. Right‑click: `Create → Cards → CardCatalog`.
2. Name it (e.g., `BaseGameCardCatalog`).
3. In the inspector, populate `cardSOs` with all `CardSO` assets that belong to this catalog (drag them in).
4. Unity will call `OnValidate`/`OnEnable` and build the internal dictionary.
    - If you accidentally assign two cards with the same `CardID`, you’ll see a warning in the console.

At runtime, other systems should only talk to this catalog via `GetCardOrNull(cardID)`.

### 4) Create a Deck

1. Right‑click: `Create → Cards → Deck`.
2. Name it (e.g., `PlayerStartingDeck`).
3. In the DeckSO inspector, populate `cardSOs` with the `CardSO` assets you want in the deck (drag them in, including duplicates for multiple copies).
4. When the asset validates, it will:
    - Build/refresh the hidden `cardIDs` list from the `CardID` of each `CardSO`.

At runtime, systems read `deck.CardIDs` and do not depend on `cardSOs`.

### 5) Simple Runtime Debug Check (DeckDebugger)

Create a small MonoBehaviour to confirm everything is wired correctly.

Example pattern:

- Fields:
    - `DeckSO deck;`
    - `CardCatalogSO catalog;`
- In `Start()`:
    - Iterate `deck.CardIDs`.
    - For each ID, call `catalog.GetCardOrNull(id)` and log:
        - Card index in deck.
        - `DisplayName`, `Description`, `CardID`.
        - `TargetingMode`.
        - Effect descriptions from `cardSO.Effects`.
        - Tags.

A successful log should show the expected number of cards, with IDs and text matching your assets, including duplicates when the same card is added multiple times to the deck.

***

## Mental Model Summary

- **CardSO**: “what a card is” (data only).
- **DeckSO**: “which cards are in this deck” (list of IDs, runtime‑oriented).
- **CardCatalogSO**: “where to look up card data by ID” (registry).
- **CardEffectSO**: “how to configure an effect and produce a runtime command”.
- **IEffectCommand**: “what actually happens in the encounter when this effect runs”.

This structure is designed so you can later plug in an encounter controller, add more effect types, or swap storage mechanisms while keeping deck/save data stable and decoupled from Unity‑specific details.[^10][^1]

If you want to refine this README further, what’s one diagram or small code snippet you’d like to add next—a sequence sketch of the play‑card flow, or a tiny example of how an encounter controller might call `CreateRuntimeCommand` and `Execute`?
<span style="display:none">[^15][^16]</span>

<div align="center">⁂</div>

[^1]: https://unity.com/how-to/separate-game-data-logic-scriptable-objects

[^2]: https://unity.com/how-to/architect-game-code-scriptable-objects

[^3]: https://stackoverflow.com/questions/58984486/create-scriptable-object-with-constant-unique-id

[^4]: https://www.angry-shark-studio.com/blog/unity-scriptableobjects-game-data-management/

[^5]: https://learn.unity.com/tutorial/use-the-command-pattern-for-flexible-and-extensible-game-systems?uv=6\&projectId=67bc8deaedbc2a23a7389cab

[^6]: https://learn.unity.com/tutorial/command-pattern-2019?version=2022.3

[^7]: https://www.linkedin.com/pulse/top-7-design-patterns-every-unity-game-developer-should-charles-hache

[^8]: https://docs.unity3d.com/6000.5/Documentation/ScriptReference/ScriptableObject.OnValidate.html

[^9]: https://www.wayline.io/blog/onvalidate-in-unity-a-brief-guide

[^10]: https://toqoz.fyi/unity-object-database.html

[^11]: https://bronsonzgeb.com/index.php/2021/09/11/the-scriptable-object-asset-registry-pattern/

[^12]: https://docs.unity3d.com/6000.1/Documentation/Manual/class-ScriptableObject.html

[^13]: https://stackoverflow.com/questions/35410753/why-do-c-sharp-tryadd-tryremove-trygetvalue-return-null-sets-when-they-fail

[^14]: https://www.reddit.com/r/csharp/comments/vqcsgz/returning_null_vs_tryx_with_out_parameter/

[^15]: https://ppl-ai-file-upload.s3.amazonaws.com/web/direct-files/collection_d7ec0717-e14d-4355-a5b8-ae499ed99197/4cec8697-5d7a-460c-842e-e641f4966ec9/generative-ai-is-a-pretty-subs-syKtIofCSF62dti0gf5eqA.md

[^16]: https://ppl-ai-file-upload.s3.amazonaws.com/web/direct-files/attachments/150988828/3656478f-7448-45bc-a713-dcf68acb9ece/SelfStudyCurriculum.md

