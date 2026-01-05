using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "CombatantCatalogSO", menuName = "Combatants/Combatant Catalog")]
public class CombatantCatalogSO : ScriptableObject
{
    [SerializeField] List<CombatantDataSO> combatantSOs;
    private Dictionary<string, CombatantDataSO> combatantDict;


    [SerializeField] private DeckDebugger deckDebugger;

    private void OnEnable()
    {
        BuildDictionary();
    }

    private void BuildDictionary()
    {
        if (combatantSOs == null) combatantSOs = new();
        if (combatantDict == null) combatantDict = new();
        combatantDict.Clear();
        foreach (var combatantSO in combatantSOs)
        {
            if (combatantSO == null) continue;
            var id = combatantSO.CombatantID;
            if (string.IsNullOrEmpty(id)) continue;
            if (combatantDict.ContainsKey(id))
            {
                Debug.LogWarning($"Duplicate combatant ID: {id}", combatantSO);
                continue;
            }
            combatantDict[id] = combatantSO;
        }
    }

    public void RuntimeInit(List<CombatantDataSO> _combatantSOs)
    {
        combatantSOs = new List<CombatantDataSO>(_combatantSOs);
        BuildDictionary();
    }

    private void OnValidate()
    {
        BuildDictionary();
# if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif
    }

    /////////
    // API //
    /////////

    public CombatantDataSO GetCombatantOrNull(string combatantID)
    {
        if (combatantDict == null) BuildDictionary();
        return combatantDict.TryGetValue(combatantID, out var combatant) ? combatant : null;
    }
}
