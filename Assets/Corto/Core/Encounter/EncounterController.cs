using UnityEngine;
using System.Collections.Generic;

[System.Serializable] // not sure if needed
public class EncounterData
{
    private Party playerParty;
    private Party enemyParty;
    private GameObject environmentPrefab;

    public Party PlayerParty { get => playerParty; }
    public Party EnemyParty { get => enemyParty; }
    public GameObject EnvironmentPrefab { get => environmentPrefab; }
    public EncounterData()
    {
        playerParty = new Party(new List<string>());
        enemyParty = new Party(new List<string>());
        environmentPrefab = null;
    }
    public EncounterData(Party _playerParty, Party _enemyParty, GameObject _environmentPrefab)
    {
        playerParty = _playerParty;
        enemyParty = _enemyParty;
        environmentPrefab = _environmentPrefab;
    }
}

public interface IEncounterView
{
    void ShowIntro();
    void ShowPlayerTurn();
    void ShowEnemyTurn();
    void ShowVictory(bool playerWon);
    void SetStatusText(string statusString);
}
public interface IEncounterEnvironment
{
    // tbd...
}

public class EncounterController : MonoBehaviour, IEncounterRules
{

    [SerializeField] private CardCatalogSO cardCatalog;
    [SerializeField] private CombatantCatalogSO combatantCatalog;

    public event System.Action<bool> OnEncounterComplete;

    private EncounterStateMachine encounterStateMachine = new EncounterStateMachine();
    private IEncounterView view;
    private IEncounterEnvironment env = null;
    private Party playerParty;
    private Party enemyParty;
    private List<RuntimeCombatant> playerRuntimeCombatants = new List<RuntimeCombatant>();
    private List<RuntimeCombatant> enemyRuntimeCombatants = new List<RuntimeCombatant>();


    private void Awake()
    {
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
        switch (enteredState.StateName)
        {
            case EncounterStateMachine.EnumStateName.INTRO:
                view.ShowIntro();
                view.SetStatusText("Encounter begins...");
                break;
            case EncounterStateMachine.EnumStateName.PLAYER_TURN:
                view.ShowPlayerTurn();
                view.SetStatusText("Your turn");
                break;
            case EncounterStateMachine.EnumStateName.ENEMY_TURN:
                view.ShowEnemyTurn();
                view.SetStatusText("Enemy turn");
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
    }
    private void HandleOnStateExited(EncounterStateMachine.StructState leavingState)
    {
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
        if (playerRuntimeCombatants == null) playerRuntimeCombatants = new();
        playerRuntimeCombatants.Clear();
        foreach (string id in playerCatalogIDs)
        {
            CombatantDataSO combatantData = combatantCatalog.GetCombatantOrNull(id);
            if (combatantData == null) continue;
            RuntimeCombatant runtimeCombatant = new RuntimeCombatant(combatantData);
            playerRuntimeCombatants.Add(runtimeCombatant);
        }
    }
    private void FixEnemyRuntimeCombatants(List<string> enemyCatalogIDs)
    {
        if (enemyRuntimeCombatants == null) enemyRuntimeCombatants = new();
        enemyRuntimeCombatants.Clear();
        foreach (string id in enemyCatalogIDs)
        {
            CombatantDataSO combatantData = combatantCatalog.GetCombatantOrNull(id);
            if (combatantData == null) continue;
            RuntimeCombatant runtimeCombatant = new RuntimeCombatant(combatantData);
            enemyRuntimeCombatants.Add(runtimeCombatant);
        }
    }


    /////////
    // API //
    /////////

    public void SetUpEncounter(EncounterData inData)
    {
        Debug.Log("Setting things up...");

        playerParty = inData.PlayerParty;
        enemyParty = inData.EnemyParty;
        if (inData.EnvironmentPrefab != null)
        {
            env = Instantiate(inData.EnvironmentPrefab).GetComponent<IEncounterEnvironment>();
        }

        List<string> playerCatalogIDs = new List<string>(inData.PlayerParty.CombatantIDs);
        List<string> enemyCatalogIDs = new List<string>(inData.EnemyParty.CombatantIDs);
        FixPlayerRuntimeCombatants(playerCatalogIDs);
        FixEnemyRuntimeCombatants(enemyCatalogIDs);

        DebugCombatants(); // for testing

        encounterStateMachine.RequestTransition(EncounterStateMachine.EnumTransition.TO_INTRO);
    }

    // EncounterUI calls this when the outro routine is complete
    public void SignalEncounterCompleteUI(bool playerWon)
    {
        OnEncounterComplete?.Invoke(playerWon);
        Debug.Log(playerWon);
    }
    

    // IEncounterRules Methods
    public void ApplyDamage(string sourceID, string targetID, int damageAmount)
    {
        // ...
    }
    public void ApplyBlock(string targetID, int blockAmount)
    {
        // ...
    }
    public void ApplyStatus(string sourceID, string targetID, EnumStatusEffect statusEffect, int stacks)
    {
        // ...
    }


    //////////
    // Junk //
    //////////
    
    private void DebugCombatants()
    {
        
        string debugStr = string.Empty;
        debugStr += "--- PLAYER COMBATANTS ---\n";
        int idx = 0;
        foreach (RuntimeCombatant rtCombatant in playerRuntimeCombatants)
        {
            CombatantDataSO data = combatantCatalog.GetCombatantOrNull(rtCombatant.CatalogID);
            string combName = data.CombatantName;
            string rtID = rtCombatant.RuntimeID;
            string ctID = data.CombatantID;
            string combDesc = data.CombatantDescription;
            DeckSO combDeck = data.Deck;
            int cardsQty = combDeck.CardIDs.Count;
            idx++;

            debugStr += $"\nCombatant number {idx}:\n";
            debugStr += $"Runtime ID: {rtID}\n";
            debugStr += $"Catalog ID: {ctID}\n";
            debugStr += $"Name: {combName}\n";
            debugStr += $"Description: {combDeck}\n";
            debugStr += $"Deck size: {cardsQty} cards\n";
        }
        debugStr += "\n--- Enemy COMBATANTS ---\n";
        idx = 0;
        foreach (RuntimeCombatant rtCombatant in enemyRuntimeCombatants)
        {
            CombatantDataSO data = combatantCatalog.GetCombatantOrNull(rtCombatant.CatalogID);
            string combName = data.CombatantName;
            string rtID = rtCombatant.RuntimeID;
            string ctID = data.CombatantID;
            string combDesc = data.CombatantDescription;
            DeckSO combDeck = data.Deck;
            int cardsQty = combDeck.CardIDs.Count;
            idx++;

            debugStr += $"\nCombatant number {idx}:\n";
            debugStr += $"Runtime ID: {rtID}\n";
            debugStr += $"Catalog ID: {ctID}\n";
            debugStr += $"Name: {combName}\n";
            debugStr += $"Description: {combDeck}\n";
            debugStr += $"Deck size: {cardsQty} cards\n";
        }

        Debug.Log(debugStr);
    }
}
