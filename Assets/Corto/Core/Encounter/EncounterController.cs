using UnityEngine;

public interface IEncounterView
{
    void ShowIntro();
    void ShowPlayerTurn();
    void ShowEnemyTurn();
    void ShowVictory(bool playerWon);
    void SetStatusText(string statusString);
}

public class EncounterController : MonoBehaviour
{

    public event System.Action<bool> OnEncounterComplete;

    private EncounterStateMachine encounterStateMachine = new EncounterStateMachine();
    private IEncounterView view;

    private void Awake()
    {
        view = GetComponent<IEncounterView>();
    }

    private void Start()
    {
        encounterStateMachine.RequestTransition(EncounterStateMachine.EnumTransition.TO_INTRO);
        StartCoroutine(TestRoutine());
    }

    private void OnEnable()
    {
        encounterStateMachine.OnStateEntered += HandleOnStateEntered;
        encounterStateMachine.OnStateExited += HandleOnStateExited;
        if (view is EncounterUI)
        {
            (view as EncounterUI).OnEncounterCompleteUI += HandleOnEncounterCompleteUI;
        }
    }
    private void OnDisable()
    {
        encounterStateMachine.OnStateEntered -= HandleOnStateEntered;
        encounterStateMachine.OnStateExited -= HandleOnStateExited;
        if (view is EncounterUI)
        {
            (view as EncounterUI).OnEncounterCompleteUI -= HandleOnEncounterCompleteUI;
        }
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
    private System.Collections.IEnumerator TestRoutine()
    {
        const float interval = 2f;

        yield return new WaitForSeconds(interval);
        encounterStateMachine.RequestTransition(EncounterStateMachine.EnumTransition.TO_PLAYER_TURN);
        yield return new WaitForSeconds(interval);
        encounterStateMachine.RequestTransition(EncounterStateMachine.EnumTransition.TO_ENEMY_TURN);
        yield return new WaitForSeconds(interval);
        encounterStateMachine.RequestTransition(EncounterStateMachine.EnumTransition.TO_PLAYER_TURN);
        yield return new WaitForSeconds(interval);
        encounterStateMachine.RequestTransition(EncounterStateMachine.EnumTransition.TO_ENEMY_TURN);
        yield return new WaitForSeconds(interval);
        encounterStateMachine.RequestTransition(EncounterStateMachine.EnumTransition.TO_PLAYER_TURN);
        yield return new WaitForSeconds(interval);
        encounterStateMachine.RequestTransition(EncounterStateMachine.EnumTransition.TO_PLAYER_VICTORY);
    }

    private void HandleOnEncounterCompleteUI()
    {
        bool playerWon = encounterStateMachine.CurrentState.StateName == EncounterStateMachine.EnumStateName.PLAYER_VICTORY;
        Debug.Log(playerWon);
        OnEncounterComplete?.Invoke(playerWon);
    }


}
