using UnityEngine;
using UnityEngine.Assertions;

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
