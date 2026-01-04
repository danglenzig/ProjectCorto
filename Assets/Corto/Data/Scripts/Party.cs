using UnityEngine;
using System.Collections.Generic;
public class Party
{
    // a Party is just a list of combatant IDs
    private List<string> combatantIDs;
    public Party(List<string> _combatantIDs)
    {
        combatantIDs = new List<string>(_combatantIDs);
    }
    public IReadOnlyList<string> CombatantIDs { get => combatantIDs; }
}
