using UnityEngine;


public enum EnumStatusEffect
{
    FRAIL,  // take more damage
    WEAK,   // give less damage
    TOUGH,  // take less damage
    STRONG  // give more damage
}

public interface IEncounterRules
{
    void ApplyDamage(string sourceID, string targetID, int damageAmount);
    void ApplyBlock(string targetID, int blockAmount);
    void ApplyStatus(string sourceID, string targetID, EnumStatusEffect statusEffect, int stacks);
}
