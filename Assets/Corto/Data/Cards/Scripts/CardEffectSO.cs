using UnityEngine;

[System.Serializable]
public class CardContext
{
    public string SourceEntityID;
    public string TargetEntityID;
    // some reference to a TBD encounter API
}


// DamageEfectSO, UtilityEffectSO, etc will inherit from this abstract base class
[CreateAssetMenu(fileName = "CardEffectSO", menuName = "Cards/Card Effect")]
public abstract class CardEffectSO : ScriptableObject
{
    [SerializeField] private string effectID;

    public string EffectID { get => effectID; }

    public abstract void Execute(CardContext cardContext);


    private void OnValidate()
    {
        if (string.IsNullOrEmpty(effectID))
        {
            effectID = System.Guid.NewGuid().ToString();
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }
    }
}
