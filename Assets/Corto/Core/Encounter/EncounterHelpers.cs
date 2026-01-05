//////////////////////////////////////
// Supporing classes, structs, etc. //
//////////////////////////////////////

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

public interface ICombatantResolver
{
    //CombatantEncounterData TryGetCombatant(string id);
    bool TryGetCombatant(string id, out CombatantEncounterData? data);
}

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
        // and so on

        drawPile = new List<string>(_configData.Deck.CardIDs);
        inHand = new List<string>();
        discardPile = new List<string>();
    }
}

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
