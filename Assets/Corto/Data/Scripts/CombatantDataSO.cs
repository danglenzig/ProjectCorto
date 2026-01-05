using UnityEngine;

[CreateAssetMenu(fileName = "CombatantDataSO", menuName = "Combatants/Combatant Data")]
public class CombatantDataSO : ScriptableObject
{
    [SerializeField] private string combatantID = string.Empty;
    [SerializeField] private string combatantName = string.Empty;
    [Multiline][SerializeField] private string combatantDescription = string.Empty;
    [SerializeField] private DeckSO deck;
    [SerializeField] private int maxHealth = 100;

    // more later as needed
    // - portrait texture
    // - animated sprite
    // - other base stats
    // - etc.

    public string CombatantID { get => combatantID; }
    public string CombatantName { get => combatantName; }
    public string CombatantDescription { get => combatantDescription; }
    public DeckSO Deck { get => deck; }
    public int MaxHealth { get => maxHealth; }
    // and so on

    private void OnValidate()
    {
        if (string.IsNullOrEmpty(combatantID))
        {
            combatantID = System.Guid.NewGuid().ToString();
# if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }
    }
    public RuntimeCombatant GetRuntimeCombatant()
    {
        return new RuntimeCombatant(this);
    }
}
