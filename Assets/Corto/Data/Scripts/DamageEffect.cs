using UnityEngine;

[CreateAssetMenu(fileName = "DamageEffect", menuName = "Cards/Effects/Damage Effect")]
public class DamageEffect : CardEffectSO
{
    [SerializeField] private int damageAmount;
    private class DamageEffectCommand: IEffectCommand
    {
        private readonly int damageAmount;
        public DamageEffectCommand(int _damageAmount)
        {
            damageAmount = _damageAmount;
        }
        public void Execute(CardContext context)
        {
            string sourceID = context.SourceEntityID;
            string targetID = context.TargetEntityID;
            // later (when we have an EncounterController) something like...
            // encounterController.DealDamage(sourceID, targetID)
        }
    }

    public override IEffectCommand CreateRuntimeCommand()
    {
        DamageEffectCommand damageEffectCommand = new DamageEffectCommand(damageAmount);

        // ...?

        return damageEffectCommand;
    }
}
