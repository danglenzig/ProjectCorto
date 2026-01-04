using UnityEngine;

[CreateAssetMenu(fileName = "StatusEffect", menuName = "Cards/Effects/Status Effect")]
public class StatusEffect : CardEffectSO
{
    [SerializeField] EnumStatusEffect statusEffect;
    [SerializeField] int stacks = 1;

    private class StatusEffectCommand: IEffectCommand
    {
        private readonly EnumStatusEffect statusEffect;
        private readonly int stacks;
        public StatusEffectCommand(EnumStatusEffect _statusEffect, int _stacks)
        {
            statusEffect = _statusEffect;
            stacks = _stacks;
        }
        public void Execute(CardContext context)
        {
            string sourceID = context.SourceEntityID;
            string targetID = context.TargetEntityID;
            context.EncounterRules.ApplyStatus(sourceID, targetID, statusEffect, stacks);
        }
    }

    public override IEffectCommand CreateRuntimeCommand()
    {

        StatusEffectCommand statusEffectCommand = new StatusEffectCommand(statusEffect, stacks);
        return statusEffectCommand;
    }
}
