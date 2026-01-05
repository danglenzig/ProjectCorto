using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "CardCatalogSO", menuName = "Cards/CardCatalog")]
public class CardCatalogSO : ScriptableObject
{
    [SerializeField] private List<CardSO> cardSOs = new();
    private Dictionary<string, CardSO> cardDict;

    private void OnEnable()
    {
        BuildDictionary();
    }
    private void BuildDictionary()
    {
        if (cardSOs == null) cardSOs = new();
        if (cardDict == null) cardDict = new();
        cardDict.Clear();

        foreach(var cardSO in cardSOs)
        {
            if (cardSO == null) continue;
            var id = cardSO.CardID;
            if (string.IsNullOrEmpty(id)) continue;
            if (cardDict.ContainsKey(id))
            {
                Debug.LogWarning($"Duplicate CardID: {id}", cardSO);
                continue;
            }
            cardDict[id] = cardSO;
        }
    }

    public void RuntimeInit(List<CardSO> _cardSOs)
    {
        cardSOs = new List<CardSO>(_cardSOs);
        BuildDictionary();
    }

    private void OnValidate()
    {
        BuildDictionary();
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif
    }

    /////////
    // API //
    /////////

    public CardSO GetCardOrNull(string cardID)
    {
        if (cardDict == null) BuildDictionary();
        return cardDict.TryGetValue(cardID, out var card) ? card : null;
    }
}
