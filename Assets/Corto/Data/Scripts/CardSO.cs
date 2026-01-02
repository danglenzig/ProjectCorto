using UnityEngine;
using System.Collections.Generic;


[CreateAssetMenu(fileName = "CardSO", menuName = "Cards/Card")]
public class CardSO : ScriptableObject
{
    [SerializeField] private string cardID;
    [SerializeField] private string displayName;
    [Multiline] [SerializeField] string description;
    [SerializeField] private List<CardEffectSO> effects;
    [SerializeField] private List<EnumCardTag> tags;
    [SerializeField] private List<string> customTags;
    [SerializeField] private EnumTargetingMode targetingMode;

    public string CardID { get => cardID; }
    public string DisplayName { get => displayName; }
    public string Description { get => description; }
    public IReadOnlyList<CardEffectSO> Effects { get => effects; }
    public IReadOnlyList<EnumCardTag> Tags { get => tags; }
    public IReadOnlyList<string> CustomTags { get => customTags; }
    public EnumTargetingMode TargetingMode { get => targetingMode; }


    private void OnValidate()
    {
        if (string.IsNullOrEmpty(cardID))
        {
            cardID = System.Guid.NewGuid().ToString();
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }
    }

}
