using UnityEngine;
using System.Collections.Generic;

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


    private void Awake()
    {
        view = GetComponent<IEncounterView>();
    }

    private void Start()
    {
        //
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


    

    /////////
    // API //
    /////////

    public void SetUpEncounter(Party _playerParty, Party _enemyParty, GameObject envPrefab = null)
    {
        playerParty = _playerParty;
        enemyParty = _enemyParty;
        if (envPrefab)
        {
            env = Instantiate(envPrefab).GetComponent<IEncounterEnvironment>();
        }

        // set up everything

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
}
