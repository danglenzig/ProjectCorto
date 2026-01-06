using UnityEngine;
using System.Collections.Generic;

public class JunkMono : MonoBehaviour
{
    List<string> inPile = new();
    List<string> outPile = new();

    private void Start()
    {
        inPile.Add("Alice");
        inPile.Add("Bob");
        inPile.Add("Cindy");
        inPile.Add("David");
        inPile.Add("Evelyn");

        //DebugList("In Pile:", inPile);
        MoveToOutPile(2);

    }

    private void MoveToOutPile(uint amount)
    {
        amount = (uint) Mathf.Min(amount, inPile.Count);
        List<string> toMove = inPile.GetRange(0, (int) amount);

        outPile.AddRange(toMove);
        inPile.RemoveRange(0, (int)amount);

        DebugList("In Pile:", inPile);
        DebugList("Out Pile: ", outPile);

    }


    private void DebugList(string listName, List<string> stringList)
    {
        string debugStr = $"{listName}: ";
        foreach(string str in stringList)
        {
            debugStr += $"{str}, ";
        }
        Debug.Log(debugStr);
    }

}
