using UnityEngine;
using System.Collections.Generic;

public class Records
{
    private Dictionary<string, string> employeeDict = new() { { "001", "Alice" }, { "002", "Bob" }, { "003", "Cindy" } };

    public bool TryGetEmployee(string employeeID, out string employeeName)
    {
        if (employeeDict.ContainsKey(employeeID)) { employeeName = employeeDict[employeeID]; return true; }
        employeeName = string.Empty;
        return false;
    }

    private void HandleEmployee(string id)
    {
        if (!TryGetEmployee(id, out var name))
        {
            Debug.Log($"There is no employee with id {id}");
        }
        else
        {
            Debug.Log($"ID: {id}, Name: {name}");
        }
    }
}
