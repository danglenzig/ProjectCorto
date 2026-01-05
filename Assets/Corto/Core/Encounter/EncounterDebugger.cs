using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class EncounterDebugger
{
    public static void DebugCombatants(Dictionary<string, CombatantEncounterData> playerDataDict, Dictionary<string, CombatantEncounterData> enemyDataDict)
    {

        List<string> playerRuntimeIDs = playerDataDict.Keys.ToList<string>();
        List<string> enemyRuntimeIDs = enemyDataDict.Keys.ToList<string>();

        string debugStr = string.Empty;
        debugStr += "--- PLAYER COMBATANTS ---\n";
        int idx = 0;
        foreach (string id in playerRuntimeIDs)
        {
            CombatantDataSO catalogData = playerDataDict[id].catalogData;
            RuntimeCombatant runtimeData = playerDataDict[id].runtimeData;
            string combName = catalogData.CombatantName;
            string rtID = id;
            string ctID = catalogData.CombatantID;
            string combDesc = catalogData.CombatantDescription;
            DeckSO combDeck = catalogData.Deck;
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
        foreach (string id in enemyRuntimeIDs)
        {
            CombatantDataSO catalogData = enemyDataDict[id].catalogData;
            RuntimeCombatant runtimeData = enemyDataDict[id].runtimeData;
            string combName = catalogData.CombatantName;
            string rtID = id;
            string ctID = catalogData.CombatantID;
            string combDesc = catalogData.CombatantDescription;
            DeckSO combDeck = catalogData.Deck;
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
