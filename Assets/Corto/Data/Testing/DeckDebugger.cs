using UnityEngine;
using System.Collections.Generic;

public class DeckDebugger : MonoBehaviour
{
    [SerializeField] private DeckSO deck;
    [SerializeField] private CardCatalogSO catalog;

    private int idx = 0;

    private void Start()
    {
        string debugStr = string.Empty;

        foreach(string cardID in deck.CardIDs)
        {
            CardSO cardSO = catalog.GetCardOrNull(cardID);
            debugStr += GetCardDebugString(cardSO, idx);
            idx++;
        }

        Debug.Log(debugStr);
    }

    private string GetCardDebugString(CardSO cardSO, int cardIdx)
    {
        string debugStr = string.Empty;

        debugStr += $"\nCard #{cardIdx} -- {cardSO.DisplayName}\n";
        debugStr += $"Description: {cardSO.Description}\n";
        debugStr += $"Card ID: {cardSO.CardID}\n";
        debugStr += $"Targets: {cardSO.TargetingMode.ToString()}\n";
        debugStr += "Effects:\n";
        debugStr += GetEffectsDebugString(cardSO.Effects);
        debugStr += "Tags:\n";
        debugStr += GetTagsString(cardSO.Tags);
        debugStr += "\n";
        return debugStr;
    }

    private string GetEffectsDebugString(IReadOnlyList<CardEffectSO> effectsList)
    {
        string debugStr = string.Empty;

        if (effectsList.Count <= 0) return debugStr;
        foreach(CardEffectSO effect in effectsList)
        {
            debugStr += $"  - {effect.EffectDescription}\n";
        }
        return debugStr;
    }
    private string GetTagsString(IReadOnlyList<EnumCardTag> tagsList)
    {
        string debugStr = string.Empty;
        if (tagsList.Count <= 0) return debugStr;
        foreach(EnumCardTag tag in tagsList)
        {
            debugStr += $"  - {tag.ToString()}";
        }
        return debugStr;
    }


}
