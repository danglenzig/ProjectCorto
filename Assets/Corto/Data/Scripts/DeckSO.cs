using UnityEngine;
using System.Collections.Generic;

// Repo link https://github.com/danglenzig/ProjectCorto/blob/main/Assets/Corto/Data/Cards/Scripts/DeckSO.cs

[CreateAssetMenu(fileName = "DeckSO", menuName = "Cards/Deck")]
public class DeckSO : ScriptableObject
{
    [SerializeField] private List<CardSO> cardSOs = new();
    [SerializeField, HideInInspector] private List<string> cardIDs = new();
    private void OnValidate()
    {
        if (cardSOs == null) cardSOs = new();
        if (cardIDs == null) cardIDs = new();

        cardIDs.Clear();

        foreach (CardSO cardSO in cardSOs)
        {
            if (cardSO == null) continue;
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
        if (!cardIDs.Remove(cardID))
        {
            Debug.LogError($"Card with ID {cardID} not in deck. Can't remove");
        }
    }
    public void ClearDeck()
    {
        cardIDs.Clear();
    }
    public void ReplaceDeck(IReadOnlyList<string> newDeck)
    {
        cardIDs.Clear();
        cardIDs.AddRange(newDeck);
    }
}
