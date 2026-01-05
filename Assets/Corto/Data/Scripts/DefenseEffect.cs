using UnityEngine;

[CreateAssetMenu(fileName = "DefenseEffect", menuName = "Cards/Effects/Defense Effect")]
public class DefenseEffect : CardEffectSO
{
    [SerializeField] private int defenseAmount;

    class DefenseEffectCommand : IEffectCommand
    {
        private readonly int defenseAmount;
        public DefenseEffectCommand(int _defenseAmount)
        {
            defenseAmount = _defenseAmount;
        }
        public void Execute(CardContext context)
        {
            string sourceID = context.SourceEntityID;
            string targetID = context.TargetEntityID;
            context.EncounterRules.ApplyBlock(sourceID, defenseAmount);
        }
    }

    public void RuntimeInit(int _defenseAmount)
    {
        defenseAmount = _defenseAmount;
    }

    public override IEffectCommand CreateRuntimeCommand()
    {
        DefenseEffectCommand defenseEffectCommand = new DefenseEffectCommand(defenseAmount);
        return defenseEffectCommand;
    }
}
