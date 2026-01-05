using UnityEngine;
using System.Collections.Generic;

public class EffectActuator
{
    public static void ApplyDamage(string sourceID, string targetID, int damageAmount, ICombatantResolver combatantResolver)
    {
        damageAmount = Mathf.Max(0, damageAmount);
        if (!combatantResolver.TryGetCombatant(targetID, out var targetData))
        {
            Debug.LogError($"Invalid targetID: {targetID}"); return;
        }
        targetData.runtimeData.CurrentHealth -= damageAmount;

        string targetName = targetData.catalogData.CombatantName;
        int targetHealth = targetData.runtimeData.CurrentHealth;
        Debug.Log($"{targetName} takes {damageAmount} damage. New health is: {targetHealth}");

    }
    public static void ApplyBlock(string targetID, int blockAmount, ICombatantResolver combatantResolver)
    {
        blockAmount = Mathf.Max(0, blockAmount);
        if(!combatantResolver.TryGetCombatant(targetID, out var targetData))
        {
            Debug.LogError($"Invalid targetID: {targetID}"); return;
        }
        targetData.runtimeData.CurrentBlock = blockAmount;
    }
    public static void ApplyStatus(string sourceID, string targetID, EnumStatusEffect statusEffect, int stacks, ICombatantResolver combatantResolver)
    {
        if (!combatantResolver.TryGetCombatant(targetID, out var targetData))
        {
            Debug.LogError($"Invalid targetID: {targetID}"); return;
        }
        for (int i = 0; i< stacks; i++)
        {
            targetData.runtimeData.CurrentStatusEffects.Add(statusEffect);
        }
    }
}
