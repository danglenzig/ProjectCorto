# Project Corto - Deckbuilding Game Documentation

This document provides a comprehensive overview of the Unity deckbuilding game project. It explains the architecture, data flow, and key systems that have been implemented so far.

## Table of Contents

1. [Project Overview](#project-overview)
2. [Project Structure](#project-structure)
3. [Entry Point & Program Flow](#entry-point--program-flow)
4. [Core Systems](#core-systems)
   - [GameServices](#gameservices)
   - [Encounter System](#encounter-system)
   - [State Machine](#state-machine)
   - [Turn Controller](#turn-controller)
5. [Data Systems](#data-systems)
   - [Card System](#card-system)
   - [Combatant System](#combatant-system)
   - [Catalog Pattern](#catalog-pattern)
6. [Effects System](#effects-system)
7. [UI System](#ui-system)
8. [Key Classes Reference](#key-classes-reference)

---

## Project Overview

This is a deckbuilding card game built in Unity using C#. The project follows a data-driven architecture with strict separation between data (ScriptableObjects) and runtime behavior. The core systems are:

- **Card System**: Cards are defined as ScriptableObjects and identified by GUIDs
- **Encounter System**: Turn-based combat encounters with player and enemy parties
- **Effects System**: Modular card effects using the Command pattern
- **State Machine**: Encounter flow management (Intro → Player Turn → Enemy Turn → Victory)

---

## Project Structure

```
Assets/Corto/
├── Core/                          # Core gameplay systems
│   ├── GameServices.cs            # Main game service singleton
│   ├── Singleton.cs               # Generic singleton base class
│   ├── MiscTools.cs               # Utility functions (shuffling, etc.)
│   └── Encounter/                 # Encounter-specific systems
│       ├── EncounterController.cs # Main encounter orchestrator
│       ├── EncounterStateMachine.cs # State machine for encounter flow
│       ├── EncounterStateHandler.cs # Handles state transitions
│       ├── TurnController.cs      # Manages turn order and logic
│       ├── EffectActuator.cs      # Applies effects (damage, block, status)
│       ├── EncounterHelpers.cs    # Data structures (RuntimeCombatant, etc.)
│       ├── EncounterUI.cs         # UI controller for encounters
│       ├── EncounterCanvas.cs     # Canvas UI component
│       └── IEncounterRules.cs     # Interface for encounter rules
│
├── Data/                          # Data definitions (ScriptableObjects)
│   ├── Scripts/
│   │   ├── CardSO.cs              # Card definition
│   │   ├── CardEffectSO.cs        # Base class for card effects
│   │   ├── CardCatalogSO.cs       # Registry of all cards
│   │   ├── DeckSO.cs              # Deck definition (list of card IDs)
│   │   ├── CombatantDataSO.cs     # Combatant definition
│   │   ├── CombatantCatalogSO.cs  # Registry of all combatants
│   │   ├── Party.cs               # Party data structure
│   │   ├── DamageEffect.cs         # Damage effect implementation
│   │   ├── DefenseEffect.cs        # Defense/block effect
│   │   ├── StatusEffect.cs        # Status effect implementation
│   │   ├── EnumCardTag.cs         # Card tag enum
│   │   └── EnumTargetingMode.cs   # Targeting mode enum
│   │
│   ├── Cards/                     # Card asset instances
│   ├── Combatants/                # Combatant asset instances
│   └── Effects/                   # Effect asset instances
│
├── Scenes/
│   ├── Demo.unity                 # Main demo scene (entry point)
│   └── EncounterDemo.unity        # Encounter scene (loaded during combat)
│
└── Junk/
    └── DemoEncounterButton.cs     # Entry point button handler
```

---

## Entry Point & Program Flow

### Demo.unity Scene

The `Demo.unity` scene contains:
- A **Canvas** with a **Button** component
- The button has `DemoEncounterButton.cs` attached
- A **GameServices** GameObject (singleton)

### DemoEncounterButton.cs

This is the entry point script. When the button is clicked:

```1:29:Assets/Corto/Junk/DemoEncounterButton.cs
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
public class DemoEncounterButton : MonoBehaviour
{
    private Button encounterButton;

    [SerializeField] private List<CombatantDataSO> playerCombatants;
    [SerializeField] private List<CombatantDataSO> enemyCombatants;
    [SerializeField] private GameObject environmentPrefab;

    private void Awake()
    {
        encounterButton = GetComponent<Button>();
        encounterButton.onClick.AddListener(HandleButtonPressed);
    }

    private void OnDestroy()
    {
        encounterButton.onClick.RemoveAllListeners();
    }

    private void HandleButtonPressed()
    {
        Party playerParty = GameServices.GetParty(playerCombatants);
        Party enemyParty = GameServices.GetParty(enemyCombatants);
        GameServices.LaunchEncounter(new EncounterData(playerParty, enemyParty, environmentPrefab));
    }
}
```

**Flow:**
1. Button click triggers `HandleButtonPressed()`
2. Converts `CombatantDataSO` lists to `Party` objects (via `GameServices.GetParty()`)
3. Creates `EncounterData` with player party, enemy party, and optional environment prefab
4. Calls `GameServices.LaunchEncounter()` to load the encounter scene

---

## Core Systems

### GameServices

`GameServices` is a singleton that manages scene transitions and encounter data.

```7:60:Assets/Corto/Core/GameServices.cs
public class GameServices : Singleton<GameServices>
{
    public const string ENCOUNTER_SCENE_NAME = "EncounterDemo";
    private static EncounterData currentEncounterData = null;

    private void OnEnable()
    {
        SceneManager.sceneLoaded += HandleOnSceneLoaded;
    }
    private void OnDisable()
    {
        SceneManager.sceneLoaded -= HandleOnSceneLoaded;
    }

    

    private static void HandleOnSceneLoaded(Scene scene, LoadSceneMode loadMode)
    {
        if (scene.name == ENCOUNTER_SCENE_NAME)
        {
            EncounterController ec = FindFirstObjectByType<EncounterController>();
            if (ec == null) { Debug.LogError("Scene has no EncounterController"); return; }
            if (currentEncounterData == null)
            {
                Debug.LogWarning($"No current EncounterData");
                ec.SetUpEncounter(new EncounterData());
            }
            else
            {
                ec.SetUpEncounter(currentEncounterData);
            }
        }
    }

    /////////
    // API //
    /////////
    
    public static void LaunchEncounter(EncounterData inData)
    {
        currentEncounterData = inData;
        SceneManager.LoadScene(ENCOUNTER_SCENE_NAME);
    }

    public static Party GetParty(List<CombatantDataSO> combatants)
    {
        List<string> ids = new List<string>();
        foreach (CombatantDataSO combatant in combatants)
        {
            ids.Add(combatant.CombatantID);
        }
        return new Party(ids);
    }
}
```

**Key Responsibilities:**
- Stores `EncounterData` temporarily during scene transition
- Listens for scene load events
- When `EncounterDemo` scene loads, finds `EncounterController` and passes data to it
- Converts `CombatantDataSO` lists to `Party` objects (lists of combatant IDs)

---

### Encounter System

The encounter system is orchestrated by `EncounterController`, which manages:
- Player and enemy parties
- Card catalog and combatant catalog references
- Turn order and state transitions
- Effect application (damage, block, status)

#### EncounterController

```33:202:Assets/Corto/Core/Encounter/EncounterController.cs
public class EncounterController : MonoBehaviour, IEncounterRules, ICombatantResolver
{
    public event System.Action<bool> OnEncounterComplete;
    private enum EnumTurnDeciderMode
    {
        TEAMWISE,
        // etc.
    }

    [SerializeField] private EnumTurnDeciderMode turnOrderMode = EnumTurnDeciderMode.TEAMWISE;
    [SerializeField] private CardCatalogSO cardCatalog;
    [SerializeField] private CombatantCatalogSO combatantCatalog;
    [SerializeField] private int maxInHand = 5;
    private EncounterStateMachine encounterStateMachine = new EncounterStateMachine();
    private IEncounterView view;
    private IEncounterEnvironment env = null;
    private Dictionary<string, CombatantEncounterData> playerDataDict;
    private Dictionary<string, CombatantEncounterData> enemyDataDict;
    private ITurnOrderDecider decider;
    private TurnController turnController;

    public IEncounterView View { get => view; }
    public IReadOnlyDictionary<string, CombatantEncounterData> PlayerDataDict { get => playerDataDict; }
    public IReadOnlyDictionary<string, CombatantEncounterData> EnemyDataDict { get => enemyDataDict; }
    public ITurnOrderDecider Decider { get => decider; }
    public TurnController TurnController { get => turnController; }
    public int MaxInHand { get => maxInHand; }
    public EncounterStateMachine EncounterStateMachine { get => encounterStateMachine; }


    private void Awake()
    {
        decider = new TeamwiseTurnOrderDecider();
        // in the future, switch against the turnOrderMode enum

        turnController = new TurnController(this);
        view = GetComponent<IEncounterView>();
    }

    private void Start()
    {
        if (GameServices.Instance == null)
        {
            SetUpEncounter(new EncounterData());
        }
    }

    private void OnEnable()
    {
        encounterStateMachine.OnStateEntered += HandleOnStateEntered;
        encounterStateMachine.OnStateExited += HandleOnStateExited;
    }
    private void OnDisable()
    {
        encounterStateMachine.OnStateEntered -= HandleOnStateEntered;
        encounterStateMachine.OnStateExited -= HandleOnStateExited;
    }

    private void HandleOnStateEntered(EncounterStateMachine.StructState enteredState)
    {
        EncounterStateHandler.HandleOnStateEntered(enteredState, this);
    }
    private void HandleOnStateExited(EncounterStateMachine.StructState leavingState)
    {
        EncounterStateHandler.HandleOnStateExited(leavingState, this);
    }
    private CardContext GetCardContext(string sourceID, string targetID)
    {
        return new CardContext(sourceID, targetID, this);
    }

    private CardSO GetCardOrNull(string id)
    {
        return cardCatalog.GetCardOrNull(id);
    }
    private CombatantDataSO GetCombatantOrNull(string id)
    {
        return combatantCatalog.GetCombatantOrNull(id);
    }

    private void FixPlayerRuntimeCombatants(List<string> playerCatalogIDs)
    {
        if (playerDataDict == null) playerDataDict = new();
        playerDataDict.Clear();

        foreach (string id in playerCatalogIDs)
        {
            CombatantDataSO catalogData = combatantCatalog.GetCombatantOrNull(id);
            if (catalogData == null) continue;
            RuntimeCombatant runtimeData = new RuntimeCombatant(catalogData);
            CombatantEncounterData data = new CombatantEncounterData(runtimeData, catalogData);
            runtimeData.DrawPile = MiscTools.ShuffleListFisherYates<string>(runtimeData.DrawPile);

            playerDataDict[runtimeData.RuntimeID] = data;
        }
    }
    private void FixEnemyRuntimeCombatants(List<string> enemyCatalogIDs)
    {
        if (enemyDataDict == null) enemyDataDict = new();
        enemyDataDict.Clear();
        foreach (string id in enemyCatalogIDs)
        {
            CombatantDataSO catalogData = combatantCatalog.GetCombatantOrNull(id);
            if (catalogData == null) continue;
            RuntimeCombatant runtimeData = new RuntimeCombatant(catalogData);
            CombatantEncounterData data = new CombatantEncounterData(runtimeData, catalogData);
            runtimeData.DrawPile = MiscTools.ShuffleListFisherYates<string>(runtimeData.DrawPile);
            enemyDataDict[runtimeData.RuntimeID] = data;
        }
    }


    /////////
    // API //
    /////////
    public void SetUpEncounter(EncounterData inData)
    {
        Debug.Log("Setting things up...");

        if (inData.EnvironmentPrefab != null)
        {
            env = Instantiate(inData.EnvironmentPrefab).GetComponent<IEncounterEnvironment>();
        }

        List<string> playerCatalogIDs = new List<string>(inData.PlayerParty.CombatantIDs);
        List<string> enemyCatalogIDs = new List<string>(inData.EnemyParty.CombatantIDs);
        FixPlayerRuntimeCombatants(playerCatalogIDs);
        FixEnemyRuntimeCombatants(enemyCatalogIDs);
        turnController.InitializeTurnController();

        encounterStateMachine.RequestTransition(EncounterStateMachine.EnumTransition.TO_INTRO);

    }
    
    public void SignalEncounterCompleteUI(bool playerWon)
    {
        // EncounterUI calls this when the outro routine is complete
        OnEncounterComplete?.Invoke(playerWon);
        Debug.Log(playerWon);
    }
    public void SignalIntroCompleteUI()
    {
        turnController.AdvanceTurn();
    }
    

    // IEncounterRules Methods
    public void ApplyDamage(string sourceID, string targetID, int damageAmount)
    {
        EffectActuator.ApplyDamage(sourceID, targetID, damageAmount, this);
    }
    public void ApplyBlock(string targetID, int blockAmount)
    {
        EffectActuator.ApplyBlock(targetID, blockAmount, this);
    }
    public void ApplyStatus(string sourceID, string targetID, EnumStatusEffect statusEffect, int stacks)
    {
        EffectActuator.ApplyStatus(sourceID, targetID, statusEffect, stacks, this);
    }

    // ICombatantResolver Methods
    public bool TryGetCombatant(string id, out CombatantEncounterData? data)
    {
        if (playerDataDict.ContainsKey(id)) { data = playerDataDict[id]; return true; }
        if (enemyDataDict.ContainsKey(id)) { data = enemyDataDict[id]; return true; }
        data = null;
        return false;
    }
}
```

**Key Components:**
- **playerDataDict / enemyDataDict**: Runtime combatant data keyed by runtime IDs
- **encounterStateMachine**: Manages encounter states (Intro, Player Turn, Enemy Turn, Victory)
- **turnController**: Manages turn order and card drawing
- **cardCatalog / combatantCatalog**: References to catalog ScriptableObjects

**Setup Flow (`SetUpEncounter`):**
1. Instantiates environment prefab (if provided)
2. Converts party combatant IDs to runtime combatant data
3. Shuffles each combatant's draw pile
4. Initializes turn controller
5. Transitions to INTRO state

---

### State Machine

The encounter uses a state machine to manage flow:

**States:**
- `INTRO`: Initial encounter setup/animation
- `PLAYER_TURN`: Player can play cards
- `ENEMY_TURN`: Enemy AI plays
- `PLAYER_VICTORY`: Player won
- `ENEMY_VICTORY`: Enemy won

**Transitions:**
- `TO_INTRO`, `TO_PLAYER_TURN`, `TO_ENEMY_TURN`, `TO_PLAYER_VICTORY`, `TO_ENEMY_VICTORY`

The state machine maintains a history queue (max 10 states) and fires events when states are entered/exited.

#### State Handler

`EncounterStateHandler` responds to state changes:

```6:67:Assets/Corto/Core/Encounter/EncounterStateHandler.cs
public class EncounterStateHandler
{
    public static void HandleOnStateEntered(EncounterStateMachine.StructState enteredState, EncounterController encounterController)
    {
        Assert.IsTrue(SanityCheckController(encounterController), "Problem with the EncounterController");

        IEncounterView view = encounterController.View;
        TurnController turnController = encounterController.TurnController;

        switch (enteredState.StateName)
        {
            case EncounterStateMachine.EnumStateName.INTRO:
                view.ShowIntro();
                view.SetStatusText("Encounter begins...");
                break;
            case EncounterStateMachine.EnumStateName.PLAYER_TURN:
                view.ShowPlayerTurn();
                view.SetStatusText("Your turn");
                turnController.HandlePlayerTurn();
                break;
            case EncounterStateMachine.EnumStateName.ENEMY_TURN:
                view.ShowEnemyTurn();
                view.SetStatusText("Enemy turn");
                turnController.HandleEnemyTurn();
                break;
            case EncounterStateMachine.EnumStateName.PLAYER_VICTORY:
                view.ShowVictory(true);
                view.SetStatusText("Victory!");
                break;
            case EncounterStateMachine.EnumStateName.ENEMY_VICTORY:
                view.ShowVictory(false);
                view.SetStatusText("You dead :(");
                break;
        }
        //Debug.Log($"Entering state: {enteredState.StateName}");
    }

    public static void HandleOnStateExited(EncounterStateMachine.StructState leavingState, EncounterController encounterController)
    {
        Assert.IsTrue(SanityCheckController(encounterController), "Problem with the EncounterController");

        switch (leavingState.StateName)
        {
            case EncounterStateMachine.EnumStateName.INTRO:
                break;
            case EncounterStateMachine.EnumStateName.PLAYER_TURN:
                break;
            case EncounterStateMachine.EnumStateName.ENEMY_TURN:
                break;
            case EncounterStateMachine.EnumStateName.PLAYER_VICTORY:
                break;
            case EncounterStateMachine.EnumStateName.ENEMY_VICTORY:
                break;
        }
    }

    private static bool SanityCheckController(EncounterController ctlr)
    {
        if (ctlr == null)                   return false;
        if (ctlr.View == null)              return false;
        if (ctlr.TurnController == null)    return false;
        return true;
    }
}
```

**State Flow:**
1. `INTRO` → `ShowIntro()` → waits 2 seconds → `SignalIntroCompleteUI()` → `AdvanceTurn()`
2. `AdvanceTurn()` determines if next turn is player or enemy
3. `PLAYER_TURN` → `HandlePlayerTurn()` → draws cards up to `maxInHand`
4. `ENEMY_TURN` → `HandleEnemyTurn()` → (AI logic not yet implemented)
5. Victory states → `ShowVictory()` → waits 2 seconds → `SignalEncounterCompleteUI()`

---

### Turn Controller

Manages turn order and card drawing:

```6:127:Assets/Corto/Core/Encounter/TurnController.cs
public class TurnController
{
    private enum EnumAffiliation
    {
        PLAYER,
        ENEMY,
        NONE
    }

    private EncounterController encounterController;
    private List<string> turnOrder = new();
    private int turnIdx = -1;

    public TurnController(EncounterController _encounterController)
    {
        encounterController = _encounterController;
    }
    
    public void InitializeTurnController()
    {
        turnOrder = encounterController.Decider.GetTurnOrderList(encounterController);
    }
    public void AdvanceTurn()
    {
        if (turnOrder.Count <= 0)
        {
            // will this ever happen? tbd...
            Debug.LogError("The turnOrder list is empty"); return;
        }
        turnIdx = (turnIdx + 1) % turnOrder.Count;
        string activeCombatantID = turnOrder[turnIdx];

        EncounterStateMachine sm = encounterController.EncounterStateMachine;
        if (TryGetCombatantAndAffiliation(activeCombatantID, out var playerData) == EnumAffiliation.PLAYER)
        {
            sm.RequestTransition(EncounterStateMachine.EnumTransition.TO_PLAYER_TURN);
            return;
        }
        if (TryGetCombatantAndAffiliation(activeCombatantID, out var enemyData) == EnumAffiliation.ENEMY)
        {
            sm.RequestTransition(EncounterStateMachine.EnumTransition.TO_ENEMY_TURN);
            return;
        }
    }

    public void HandlePlayerTurn()
    {
        // called when EncounterStateMachine enters EnumStateName.PLAYER_TURN
        if (!encounterController.TryGetCombatant(turnOrder[turnIdx], out var combData))
        {
            Debug.LogError($"Problem getting combatant data for {turnOrder[turnIdx]}");
            return;
        }

        string combName = combData.catalogData.CombatantName;
        List<string> drawPile       = new List<string>(combData.runtimeData.DrawPile);
        List<string> inHand         = new List<string>(combData.runtimeData.InHand);
        List<string> discardPile    = new List<string>(combData.runtimeData.DiscardPile);

        // deal up to the max inhand size
        int cardsNeeded = (encounterController.MaxInHand - inHand.Count);
        if (drawPile.Count < cardsNeeded)
        {
            Debug.Log("Recycling discards...");
            drawPile.Clear();
            drawPile = ShuffleDiscardsIntoDrawPile(drawPile, discardPile);
            discardPile.Clear();
        }

        Assert.IsTrue(drawPile.Count >= cardsNeeded, "Something weird happened");

        for (int i = 0; i < cardsNeeded; i++)
        {
            inHand.Add(drawPile[0]);
            drawPile.RemoveAt(0);
        }

        combData.runtimeData.DrawPile = new List<string>(drawPile);
        combData.runtimeData.InHand = new List<string>(inHand);
        combData.runtimeData.DiscardPile = new List<string>(discardPile);

        

    }

    public void HandleEnemyTurn()
    {
        // called when EncounterStateMachine enters EnumStateName.PLAYER_TURN
        // ...
    }

    private List<string> ShuffleDiscardsIntoDrawPile(List<string> drawPile, List<string> discardPile)
    {
        List<string> newDrawPile = new();
        newDrawPile.AddRange(drawPile);
        newDrawPile.AddRange(discardPile);
        newDrawPile = MiscTools.ShuffleListFisherYates(newDrawPile); // returns a shuffled copy
        return newDrawPile;
    }


    private EnumAffiliation TryGetCombatantAndAffiliation(string runtimeID, out CombatantEncounterData? data)
    {
        if (encounterController.PlayerDataDict.ContainsKey(runtimeID))
        {
            data = encounterController.PlayerDataDict[runtimeID];
            return EnumAffiliation.PLAYER;
        }
        if (encounterController.EnemyDataDict.ContainsKey(runtimeID))
        {
            data = encounterController.EnemyDataDict[runtimeID];
            return EnumAffiliation.ENEMY;
        }

        Debug.LogError("Something is worng.");
        data = null;
        return EnumAffiliation.NONE;
    }



}
```

**Turn Order:**
- Uses `ITurnOrderDecider` interface (currently `TeamwiseTurnOrderDecider`)
- Teamwise order: all player combatants, then all enemy combatants
- Cycles through turn order list

**Card Drawing (`HandlePlayerTurn`):**
- Draws cards until hand reaches `maxInHand` (default 5)
- If draw pile is empty, shuffles discard pile into draw pile
- Updates combatant's draw pile, hand, and discard pile

---

## Data Systems

### Card System

The card system is data-driven using ScriptableObjects. Cards are identified by GUID strings, not Unity object references.

#### CardSO

A `CardSO` represents a single card definition:

```5:49:Assets/Corto/Data/Scripts/CardSO.cs
[CreateAssetMenu(fileName = "CardSO", menuName = "Cards/Card")]
public class CardSO : ScriptableObject
{
    [SerializeField] private string cardID;
    [SerializeField] private string displayName;
    [Multiline] [SerializeField] string description;
    [SerializeField] private List<CardEffectSO> effects;
    [SerializeField] private List<EnumCardTag> tags;
    [SerializeField] private List<string> customTags;
    [SerializeField] private EnumTargetingMode targetingMode;

    public string CardID { get => cardID; }
    public string DisplayName { get => displayName; }
    public string Description { get => description; }
    public IReadOnlyList<CardEffectSO> Effects { get => effects; }
    public IReadOnlyList<EnumCardTag> Tags { get => tags; }
    public IReadOnlyList<string> CustomTags { get => customTags; }
    public EnumTargetingMode TargetingMode { get => targetingMode; }

    public void RuntimeInit(string _displayName, CardEffectSO _effect, EnumCardTag _tag, EnumTargetingMode _targetingMode)
    {
        effects = new();
        tags = new();
        customTags = new();
        cardID = System.Guid.NewGuid().ToString();
        displayName = _displayName;
        description = _displayName;
        effects.Add(_effect);
        tags.Add(_tag);
        targetingMode = _targetingMode;
    }


    private void OnValidate()
    {
        if (string.IsNullOrEmpty(cardID))
        {
            cardID = System.Guid.NewGuid().ToString();
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }
    }

}
```

**Key Fields:**
- `cardID`: Auto-generated GUID (unique identifier)
- `effects`: List of `CardEffectSO` assets
- `tags`: Typed tags (ATTACK, DEFENSE, STATUS)
- `targetingMode`: How the card targets (SINGLE_COMBATANT, SINGLE_ENEMY, etc.)

#### DeckSO

A `DeckSO` stores a list of card IDs (not card references):

```4:60:Assets/Corto/Data/Scripts/DeckSO.cs
[CreateAssetMenu(fileName = "DeckSO", menuName = "Cards/Deck")]
public class DeckSO : ScriptableObject
{
    [SerializeField] private List<CardSO> cardSOs = new();
    [SerializeField, HideInInspector] private List<string> cardIDs = new();
    private void OnValidate()
    {
        if (cardSOs == null) cardSOs = new();
        if (cardIDs == null) cardIDs = new();
        CreateIDList(cardSOs);
# if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
# endif
    }

    private void CreateIDList(List<CardSO> cards)
    {
        cardIDs.Clear();
        foreach (CardSO cardSO in cardSOs)
        {
            if (cardSO == null) continue;
            cardIDs.Add(cardSO.CardID);
        }
    }

    /////////
    // API //
    /////////
    public IReadOnlyList<string> CardIDs { get => cardIDs; }
    public void AddCardID(string cardID)
    {
        cardIDs.Add(cardID);
    }
    public void RemoveCardID(string cardID)
    {
        if (!cardIDs.Remove(cardID))
        {
            Debug.LogError($"Card with ID {cardID} not in deck. Can't remove");
        }
    }
    public void ClearDeck()
    {
        cardIDs.Clear();
    }
    public void ReplaceDeck(IReadOnlyList<string> newDeck)
    {
        cardIDs.Clear();
        cardIDs.AddRange(newDeck);
    }

    public void RuntimeInit(List<CardSO> cards)
    {
        cardSOs = cards;
        if (cardIDs == null) cardIDs = new();
        CreateIDList(cardSOs);
    }
}
```

**Design:**
- `cardSOs`: Editor-only convenience list (drag-and-drop in inspector)
- `cardIDs`: Runtime list synchronized from `cardSOs`
- Runtime systems only use `CardIDs`, never `cardSOs`

This allows decks to be serialized as simple ID lists (useful for save files).

---

### Combatant System

#### CombatantDataSO

Defines a combatant (player or enemy):

```3:51:Assets/Corto/Data/Scripts/CombatantDataSO.cs
[CreateAssetMenu(fileName = "CombatantDataSO", menuName = "Combatants/Combatant Data")]
public class CombatantDataSO : ScriptableObject
{
    [SerializeField] private string combatantID = string.Empty;
    [SerializeField] private string combatantName = string.Empty;
    [Multiline][SerializeField] private string combatantDescription = string.Empty;
    [SerializeField] private DeckSO deck;
    [SerializeField] private int maxHealth = 100;

    // more later as needed
    // - portrait texture
    // - animated sprite
    // - other base stats
    // - etc.

    public string CombatantID { get => combatantID; }
    public string CombatantName { get => combatantName; }
    public string CombatantDescription { get => combatantDescription; }
    public DeckSO Deck { get => deck; }
    public int MaxHealth { get => maxHealth; }
    // and so on

    public void RuntimeInit(string _combatantName, string _combandDescription, DeckSO _deck, int _maxHealth = 100)
    {
        // for unit testing only
        combatantID = System.Guid.NewGuid().ToString();
        combatantName = _combatantName;
        combatantDescription = _combandDescription;
        deck = _deck;
        maxHealth = _maxHealth;
    }
    

    private void OnValidate()
    {
        if (string.IsNullOrEmpty(combatantID))
        {
            combatantID = System.Guid.NewGuid().ToString();
# if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }
    }
    public RuntimeCombatant GetRuntimeCombatant()
    {
        return new RuntimeCombatant(this);
    }
}
```

#### RuntimeCombatant

Runtime representation of a combatant during an encounter:

```40:85:Assets/Corto/Core/Encounter/EncounterHelpers.cs
public class RuntimeCombatant
{
    private string runtimeID;
    private string catalogID;
    private int currentHealth;
    private int currentBlock;
    private List<EnumStatusEffect> currentStatusEffects;
    // and so on -- currentXYZ for every other "starting value" stat in the SO

    private List<string> drawPile;
    private List<string> inHand;
    private List<string> discardPile;


    public string RuntimeID { get => runtimeID; }
    public string CatalogID { get => catalogID; }
    public int CurrentHealth
    {
        get => currentHealth;
        set { currentHealth = Mathf.Max(0, value); }
    }
    public int CurrentBlock
    {
        get => currentBlock;
        set { currentBlock = Mathf.Max(0, value); }
    }
    public List<EnumStatusEffect> CurrentStatusEffects { get => currentStatusEffects; }
    public List<string> DrawPile { get => drawPile; set => drawPile = value; }
    public List<string> InHand { get => inHand; set => inHand = value; }
    public List<string> DiscardPile { get => discardPile; set => discardPile = value; }


    public RuntimeCombatant(CombatantDataSO _configData)
    {
        runtimeID = System.Guid.NewGuid().ToString();
        catalogID = _configData.CombatantID;
        currentHealth = _configData.MaxHealth;
        currentBlock = 0;
        currentStatusEffects = new();

        drawPile = new List<string>(_configData.Deck.CardIDs);
        inHand = new List<string>();
        discardPile = new List<string>();
    }
}
```

**Key Distinctions:**
- `catalogID`: References the `CombatantDataSO` (shared across encounters)
- `runtimeID`: Unique ID for this specific encounter instance
- `drawPile`, `inHand`, `discardPile`: Lists of card IDs (not card objects)
- `currentHealth`, `currentBlock`: Runtime state that changes during encounter

#### CombatantEncounterData

Wrapper combining runtime and catalog data:

```87:96:Assets/Corto/Core/Encounter/EncounterHelpers.cs
public class CombatantEncounterData
{
    public RuntimeCombatant runtimeData;
    public CombatantDataSO catalogData;
    public CombatantEncounterData(RuntimeCombatant _runtimeData, CombatantDataSO _catalogData)
    {
        runtimeData = _runtimeData;
        catalogData = _catalogData;
    }
}
```

---

### Catalog Pattern

Both cards and combatants use a catalog pattern:

- **CardCatalogSO**: Maps card IDs to `CardSO` assets
- **CombatantCatalogSO**: Maps combatant IDs to `CombatantDataSO` assets

**Benefits:**
- Decouples IDs from Unity references
- Enables serialization of IDs in save files
- Allows runtime lookup without maintaining references

Example (`CardCatalogSO`):

```4:57:Assets/Corto/Data/Scripts/CardCatalogSO.cs
[CreateAssetMenu(fileName = "CardCatalogSO", menuName = "Cards/CardCatalog")]
public class CardCatalogSO : ScriptableObject
{
    [SerializeField] private List<CardSO> cardSOs = new();
    private Dictionary<string, CardSO> cardDict;

    private void OnEnable()
    {
        BuildDictionary();
    }
    private void BuildDictionary()
    {
        if (cardSOs == null) cardSOs = new();
        if (cardDict == null) cardDict = new();
        cardDict.Clear();

        foreach(var cardSO in cardSOs)
        {
            if (cardSO == null) continue;
            var id = cardSO.CardID;
            if (string.IsNullOrEmpty(id)) continue;
            if (cardDict.ContainsKey(id))
            {
                Debug.LogWarning($"Duplicate CardID: {id}", cardSO);
                continue;
            }
            cardDict[id] = cardSO;
        }
    }

    public void RuntimeInit(List<CardSO> _cardSOs)
    {
        cardSOs = new List<CardSO>(_cardSOs);
        BuildDictionary();
    }

    private void OnValidate()
    {
        BuildDictionary();
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif
    }

    /////////
    // API //
    /////////

    public CardSO GetCardOrNull(string cardID)
    {
        if (cardDict == null) BuildDictionary();
        return cardDict.TryGetValue(cardID, out var card) ? card : null;
    }
}
```

---

## Effects System

Card effects use the **Command Pattern**. Each `CardEffectSO` creates runtime command objects that execute effects.

### CardEffectSO (Base Class)

```22:44:Assets/Corto/Data/Scripts/CardEffectSO.cs
// DamageEfectSO, UtilityEffectSO, etc will inherit from this abstract base class
// and implement their own Execute(CardContext)
[CreateAssetMenu(fileName = "CardEffectSO", menuName = "Cards/Effects/Card Effect")]
public abstract class CardEffectSO : ScriptableObject
{
    [SerializeField] private string effectID;
    [SerializeField] private string effectDescription;

    public string EffectID { get => effectID; }
    public string EffectDescription { get => effectDescription; }
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

**Interface:**
```17:20:Assets/Corto/Data/Scripts/CardEffectSO.cs
public interface IEffectCommand
{
    void Execute(CardContext context);
}
```

### CardContext

Context passed to effect commands:

```3:16:Assets/Corto/Data/Scripts/CardEffectSO.cs
[System.Serializable]
public class CardContext
{
    public string SourceEntityID;
    public string TargetEntityID;
    public IEncounterRules EncounterRules;

    public CardContext(string _sourceEntityID, string _targetEntityID, IEncounterRules _rules)
    {
        SourceEntityID = _sourceEntityID;
        TargetEntityID = _targetEntityID;
        EncounterRules = _rules;
    }
}
```

### Effect Implementations

#### DamageEffect

```3:32:Assets/Corto/Data/Scripts/DamageEffect.cs
[CreateAssetMenu(fileName = "DamageEffect", menuName = "Cards/Effects/Damage Effect")]
public class DamageEffect : CardEffectSO
{
    [SerializeField] private int damageAmount;
    private class DamageEffectCommand: IEffectCommand
    {
        private readonly int damageAmount;
        public DamageEffectCommand(int _damageAmount)
        {
            damageAmount = _damageAmount;
        }
        public void Execute(CardContext context)
        {
            string sourceID = context.SourceEntityID;
            string targetID = context.TargetEntityID;
            context.EncounterRules.ApplyDamage(sourceID, targetID, damageAmount);
        }
    }

    public void RuntimeInit(int _damageAmount)
    {
        damageAmount = _damageAmount;
    }

    public override IEffectCommand CreateRuntimeCommand()
    {
        DamageEffectCommand damageEffectCommand = new DamageEffectCommand(damageAmount);
        return damageEffectCommand;
    }
}
```

**Pattern:**
1. `DamageEffect` (ScriptableObject) stores `damageAmount` (data)
2. `CreateRuntimeCommand()` creates a `DamageEffectCommand` instance (behavior)
3. `DamageEffectCommand.Execute()` calls `EncounterRules.ApplyDamage()`

#### DefenseEffect & StatusEffect

Similar pattern:
- `DefenseEffect` → `DefenseEffectCommand` → `ApplyBlock()`
- `StatusEffect` → `StatusEffectCommand` → `ApplyStatus()`

### Effect Actuator

`EffectActuator` contains static methods that apply effects to combatants:

```4:55:Assets/Corto/Core/Encounter/EffectActuator.cs
public class EffectActuator
{
    public static void ApplyDamage(string sourceID, string targetID, int damageAmount, ICombatantResolver combatantResolver)
    {
        damageAmount = Mathf.Max(0, damageAmount);
        if (!combatantResolver.TryGetCombatant(targetID, out var targetData))
        {
            Debug.LogError($"Invalid targetID: {targetID}"); return;
        }
        targetData.runtimeData.CurrentHealth -= damageAmount;

        string targetName = targetData.catalogData.CombatantName;
        int targetHealth = targetData.runtimeData.CurrentHealth;
        Debug.Log($"{targetName} takes {damageAmount} damage. New health is: {targetHealth}");

    }
    public static void ApplyBlock(string targetID, int blockAmount, ICombatantResolver combatantResolver)
    {
        blockAmount = Mathf.Max(0, blockAmount);
        if(!combatantResolver.TryGetCombatant(targetID, out var targetData))
        {
            Debug.LogError($"Invalid targetID: {targetID}"); return;
        }
        targetData.runtimeData.CurrentBlock += blockAmount;

        string targetName = targetData.catalogData.CombatantName;
        int targetBlock = targetData.runtimeData.CurrentBlock;
        Debug.Log($"{targetName} gains {blockAmount} damage. New block is: {targetBlock}");
    }
    public static void ApplyStatus(string sourceID, string targetID, EnumStatusEffect statusEffect, int stacks, ICombatantResolver combatantResolver)
    {
        if (!combatantResolver.TryGetCombatant(targetID, out var targetData))
        {
            Debug.LogError($"Invalid targetID: {targetID}"); return;
        }
        for (int i = 0; i< stacks; i++)
        {
            targetData.runtimeData.CurrentStatusEffects.Add(statusEffect);
        }

        string debugStr = string.Empty;
        string targetName = targetData.catalogData.CombatantName;
        List<EnumStatusEffect> effects = targetData.runtimeData.CurrentStatusEffects;
        debugStr += $"{targetName} status effects: ";
        foreach (EnumStatusEffect effect in effects)
        {
            debugStr += $"{effect.ToString()}, ";
        }
        Debug.Log(debugStr);

    }
}
```

**Status Effects:**
```4:10:Assets/Corto/Core/Encounter/IEncounterRules.cs
public enum EnumStatusEffect
{
    FRAIL,  // take more damage
    WEAK,   // give less damage
    TOUGH,  // take less damage
    STRONG  // give more damage
}
```

**Note:** Status effects are stored but not yet applied to damage calculations.

---

## UI System

### IEncounterView Interface

```3:10:Assets/Corto/Core/Encounter/EncounterUI.cs
public interface IEncounterView
{
    void ShowIntro();
    void ShowPlayerTurn();
    void ShowEnemyTurn();
    void ShowVictory(bool playerWon);
    void SetStatusText(string statusString);
}
```

### EncounterUI

`EncounterUI` implements `IEncounterView`:

```12:55:Assets/Corto/Core/Encounter/EncounterUI.cs
public class EncounterUI : MonoBehaviour, IEncounterView
{
    [SerializeField] private TMP_Text statusText;

    public void ShowIntro()
    {
        StartCoroutine(SimulateIntroRoutine());
    }
    public void ShowPlayerTurn()
    {
        // ...
    }
    public void ShowEnemyTurn()
    {
        // ...
    }

    public void ShowVictory(bool playerWon)
    {
        StartCoroutine(SimulateVictoryRoutine(playerWon));
    }
    public void SetStatusText(string statusString)
    {
        statusText.text = statusString;
    }

    private System.Collections.IEnumerator SimulateVictoryRoutine(bool playerWon)
    {
        const float duration = 2f;
        yield return new WaitForSeconds(duration);

        EncounterController ec = GetComponent<EncounterController>();
        ec.SignalEncounterCompleteUI(playerWon);
    }

    private System.Collections.IEnumerator SimulateIntroRoutine()
    {
        const float duration = 2f;
        yield return new WaitForSeconds(duration);
        EncounterController ec = GetComponent<EncounterController>();
        ec.SignalIntroCompleteUI();
    }

}
```

**Current Implementation:**
- `ShowIntro()`: Waits 2 seconds, then signals completion
- `ShowVictory()`: Waits 2 seconds, then signals completion
- `ShowPlayerTurn()` / `ShowEnemyTurn()`: Placeholder (not yet implemented)

---

## Key Classes Reference

### Data Structures

- **`Party`**: Simple wrapper around a list of combatant IDs
- **`EncounterData`**: Contains player party, enemy party, and optional environment prefab
- **`RuntimeCombatant`**: Runtime state for a combatant (health, cards, etc.)
- **`CombatantEncounterData`**: Pairs `RuntimeCombatant` with `CombatantDataSO`
- **`CardContext`**: Context passed to effect commands (source, target, rules)

### Enums

- **`EnumCardTag`**: ATTACK, DEFENSE, STATUS
- **`EnumTargetingMode`**: SINGLE_COMBATANT, COMBATANT_PARTY, SINGLE_ENEMY, ENEMY_PARTY
- **`EnumStatusEffect`**: FRAIL, WEAK, TOUGH, STRONG

### Interfaces

- **`IEncounterRules`**: Methods for applying damage, block, status
- **`IEncounterView`**: UI display methods
- **`IEffectCommand`**: Execute method for card effects
- **`ICombatantResolver`**: Resolve combatant by ID
- **`ITurnOrderDecider`**: Determine turn order
- **`IEncounterEnvironment`**: (Placeholder, not yet implemented)

### Utility Classes

- **`Singleton<T>`**: Generic singleton base class
- **`MiscTools`**: Utility functions (shuffling, random selection)

---

## Program Flow Summary

1. **Demo.unity** → User clicks button
2. **DemoEncounterButton** → Creates `EncounterData` → Calls `GameServices.LaunchEncounter()`
3. **GameServices** → Stores `EncounterData` → Loads `EncounterDemo` scene
4. **EncounterController** → Receives data via `SetUpEncounter()`
   - Creates runtime combatants from catalog IDs
   - Shuffles draw piles
   - Initializes turn controller
   - Transitions to INTRO state
5. **EncounterStateMachine** → INTRO state entered
6. **EncounterStateHandler** → Calls `view.ShowIntro()`
7. **EncounterUI** → Waits 2 seconds → Calls `SignalIntroCompleteUI()`
8. **EncounterController** → `AdvanceTurn()` → Determines next turn
9. **TurnController** → If player turn: `HandlePlayerTurn()` → Draws cards
10. **EncounterStateMachine** → Transitions to PLAYER_TURN / ENEMY_TURN
11. (Card playing logic not yet implemented)
12. Victory conditions → Transitions to victory state → Waits → Signals completion

---

## Not Yet Implemented

- **Card Playing**: Player selecting and playing cards from hand
- **Enemy AI**: Enemy turn logic
- **Victory Conditions**: Checking when all enemies/players are defeated
- **Status Effect Application**: Actually modifying damage based on status effects
- **Block/Damage Interaction**: Block reducing incoming damage
- **Card Targeting UI**: UI for selecting targets based on `EnumTargetingMode`
- **Hand UI**: Displaying cards in hand
- **Combatant UI**: Displaying combatant health, status effects

---

## Design Patterns Used

1. **Singleton Pattern**: `GameServices`, `Singleton<T>`
2. **Command Pattern**: `IEffectCommand` for card effects
3. **Catalog/Registry Pattern**: `CardCatalogSO`, `CombatantCatalogSO`
4. **State Machine Pattern**: `EncounterStateMachine`
5. **Strategy Pattern**: `ITurnOrderDecider` for turn order algorithms
6. **Data-Driven Design**: ScriptableObjects for cards, combatants, effects

---

*This documentation was generated from code analysis. For more details, see the existing `CARDS_README.md` in `Assets/Corto/Data/` for information about the card system specifically.*
