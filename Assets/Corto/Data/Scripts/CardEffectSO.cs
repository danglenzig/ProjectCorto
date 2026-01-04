using UnityEngine;

[System.Serializable]
public class CardContext
{
    public string SourceEntityID;
    public string TargetEntityID;
    public IEncounterRules EncounterRules;

    public CardContext(string _sourceEntityID, string _targetEntityID, IEncounterRules _rules)
    {
        SourceEntityID = _sourceEntityID;
        TargetEntityID = _targetEntityID;
        EncounterRules = _rules;
    }
}
public interface IEffectCommand
{
    void Execute(CardContext context);
}

// DamageEfectSO, UtilityEffectSO, etc will inherit from this abstract base class
// and implement their own Execute(CardContext)
[CreateAssetMenu(fileName = "CardEffectSO", menuName = "Cards/Effects/Card Effect")]
public abstract class CardEffectSO : ScriptableObject
{
    [SerializeField] private string effectID;
    [SerializeField] private string effectDescription;

    public string EffectID { get => effectID; }
    public string EffectDescription { get => effectDescription; }
    public abstract IEffectCommand CreateRuntimeCommand();

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
