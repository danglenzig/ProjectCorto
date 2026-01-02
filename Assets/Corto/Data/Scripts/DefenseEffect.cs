using UnityEngine;

[CreateAssetMenu(fileName = "DefenseEffect", menuName = "Cards/Effects/Defense Effect")]
public class DefenseEffect : CardEffectSO
{
    [SerializeField] private int defenseAmount;

    class DefenseEffectCommand : IEffectCommand
    {
        public void Execute(CardContext context)
        {
            // ...?
        }
    }
    public override IEffectCommand CreateRuntimeCommand()
    {
        DefenseEffectCommand defenseEffectCommand = new DefenseEffectCommand();
        // ...?
        return defenseEffectCommand;
    }
}
