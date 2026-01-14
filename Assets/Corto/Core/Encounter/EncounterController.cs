using UnityEngine;
using System.Collections.Generic;
using System.Linq;


public interface ITurnOrderDecider
{
    List<string> GetTurnOrderList(EncounterController controller);
}
public interface ICombatantResolver
{
    //CombatantEncounterData TryGetCombatant(string id);
    bool TryGetCombatant(string id, out CombatantEncounterData? data);
}

public class TeamwiseTurnOrderDecider: ITurnOrderDecider
{
    public List<string> GetTurnOrderList(EncounterController controller)
    {
        List<string> turnOrder = new();
        foreach(string id in controller.PlayerDataDict.Keys.ToList<string>())
        {
            turnOrder.Add(id);
        }
        foreach(string id in controller.EnemyDataDict.Keys.ToList<string>())
        {
            turnOrder.Add(id);
        }
        return turnOrder;
    }
}

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
