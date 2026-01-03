using UnityEngine;
using System.Collections.Generic;
using System.Linq;


public class EncounterStateMachine
{
    private const int STATE_HISTORY_SIZE = 10;

    public event System.Action<StructState> OnStateEntered;
    public event System.Action<StructState> OnStateExited;

    
    public EncounterStateMachine()
    {
        Initialize();
    }
    public enum EnumStateName
    {
        INTRO,
        PLAYER_TURN,
        ENEMY_TURN,
        PLAYER_VICTORY,
        ENEMY_VICTORY
    }

    public enum EnumTransition
    {
        TO_INTRO,
        TO_PLAYER_TURN,
        TO_ENEMY_TURN,
        TO_PLAYER_VICTORY,
        TO_ENEMY_VICTORY
    }
    public struct StructState
    {
        private EnumStateName stateName;
        private List<EnumTransition> transitions;
        public EnumStateName StateName { get => stateName; }
        public IReadOnlyList<EnumTransition> Transisions { get => transitions; }
        public StructState (EnumStateName _stateName, List<EnumTransition> _transitions)
        {
            stateName = _stateName;
            transitions = new List<EnumTransition>(_transitions);
        }
    }
    public IReadOnlyList<StructState> StateHistory { get => stateHistory.ToList<StructState>(); }
    public StructState CurrentState { get => stateHistory.LastOrDefault<StructState>(); }

    private Queue<StructState> stateHistory = new Queue<StructState>();
    private List<StructState> allStates = new List<StructState>();
    private bool initialized = false;

    private StructState introState = new StructState
        (
            EnumStateName.INTRO,
            new List<EnumTransition> { EnumTransition.TO_INTRO, EnumTransition.TO_PLAYER_TURN, EnumTransition.TO_ENEMY_TURN }
            // INTRO is allowed to transition to transition to itself 
        );
    private StructState playerTurnState = new StructState
        (
            EnumStateName.PLAYER_TURN,
            new List<EnumTransition> { EnumTransition.TO_ENEMY_TURN, EnumTransition.TO_PLAYER_VICTORY, EnumTransition.TO_ENEMY_VICTORY }
        );
    private StructState enemyTurmState = new StructState
        (
            EnumStateName.ENEMY_TURN,
            new List<EnumTransition> { EnumTransition.TO_PLAYER_TURN, EnumTransition.TO_PLAYER_VICTORY, EnumTransition.TO_ENEMY_VICTORY }
        );
    private StructState playerVictoryState = new StructState
        (
            EnumStateName.PLAYER_VICTORY,
            new List<EnumTransition>()
        );
    private StructState enemyVictoryState = new StructState
        (
            EnumStateName.ENEMY_VICTORY,
            new List<EnumTransition>()
        );


    public void Initialize()
    {
        stateHistory.Enqueue(introState);
        initialized = true;
    }

    public void RequestTransition(EnumTransition transition)
    {
        if (!initialized) return;
        if (!CurrentState.Transisions.Contains(transition))
        {
            Debug.LogWarning($"{CurrentState.StateName.ToString()} has no transition {transition.ToString()}");
            return;
        }
        HandleTransition(transition);
    }

    private void HandleTransition(EnumTransition transition)
    {
        OnStateExited?.Invoke(CurrentState);
        switch (transition)
        {
            case EnumTransition.TO_INTRO:
                stateHistory.Enqueue(introState);
                break;
            case EnumTransition.TO_PLAYER_TURN:
                stateHistory.Enqueue(playerTurnState);
                break;
            case EnumTransition.TO_ENEMY_TURN:
                stateHistory.Enqueue(enemyTurmState);
                break;
            case EnumTransition.TO_PLAYER_VICTORY:
                stateHistory.Enqueue(playerVictoryState);
                break;
            case EnumTransition.TO_ENEMY_VICTORY:
                stateHistory.Enqueue(enemyVictoryState);
                break;
        }
        OnStateEntered?.Invoke(CurrentState);
        if (stateHistory.Count > EncounterStateMachine.STATE_HISTORY_SIZE) { stateHistory.Dequeue(); }
    }
}
