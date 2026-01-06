using UnityEngine;
using System.Collections.Generic;

public class TurnController
{
    private EncounterController encounterController;
    public TurnController(EncounterController _encounterController)
    {
        encounterController = _encounterController;
    }

    public void BeginNextTurn()
    {
        // EncounterController initializes the index to -1, 
        // so on the first turn the value is 0.
        // the index setter does the % turnOrder.Count operation
        encounterController.TurnIdx++;
        
        string currentCombatantID = encounterController.TurnOrder[encounterController.TurnIdx];
        if (!encounterController.TryGetCombatant(currentCombatantID, out var currentCombatantData))
        {
            Debug.LogError($"Invalid combatant runtime ID: {currentCombatantID}");
            return;
        }

        List<string> drawPile = new List<string>(currentCombatantData.runtimeData.DrawPile);
        List<string> inHand = new List<string>(currentCombatantData.runtimeData.InHand);
        List<string> discardPile = new List<string>(currentCombatantData.runtimeData.DiscardPile);

        if (inHand.Count < encounterController.MaxInHand)
        {
            // draw up
        }

        // signal that the hand has been dealt
        // So that the EncounterController waits
        // for an input, either from the player,
        // or from the NPC AI


    }

}
