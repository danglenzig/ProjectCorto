using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "DeckSO", menuName = "Cards/Deck")]
public class DeckSO : ScriptableObject
{
    [SerializeField] private List<CardSO> cardSOs;
    private List<string> cardIDs;
    private void OnValidate()
    {
        cardIDs = new List<string>();
        foreach (CardSO cardSO in cardSOs)
        {
            cardIDs.Add(cardSO.CardID);
        }
# if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
# endif
    }

    /////////
    // API //
    /////////
    public IReadOnlyList<string> CardIDs { get => cardIDs; }
    public void AddCardID(string cardID)
    {
        cardIDs.Add(cardID);
    }
    public void RemoveCardID(string cardID)
    {
        if (!cardIDs.Contains(cardID)) { Debug.LogError($"Card with ID {cardID} not in deck. Can't remove"); return; }
        cardIDs.Remove(cardID);
    }
    public void ClearDeck()
    {
        cardIDs = new List<string>();
    }
    public void ReplaceDeck(IReadOnlyList<string> newDeck)
    {
        cardIDs = new List<string>(newDeck);
    }
}
