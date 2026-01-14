using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Assertions;


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
