using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "CardCatalogSO", menuName = "Cards/CardCatalog")]
public class CardCatalogSO : ScriptableObject
{
    [SerializeField] private List<CardSO> cardSOs;
    private Dictionary<string, CardSO> cardDict;
    private void OnValidate()
    {
        cardDict = new Dictionary<string, CardSO>();
        foreach (CardSO cardSO in cardSOs)
        {
            if (cardDict.ContainsKey(cardSO.CardID)) { Debug.LogWarning($"Duplicate CardID: {cardSO.CardID}"); }
            else
            {
                cardDict[cardSO.CardID] = cardSO;
            }
        }
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif
    }

    /////////
    // API //
    /////////
    
    public CardSO? GetCardOrNull(string cardID)
    {
        if (!cardDict.ContainsKey(cardID)) return null;
        return cardDict[cardID];
    }

}
