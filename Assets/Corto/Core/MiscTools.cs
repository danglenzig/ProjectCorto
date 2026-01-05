using UnityEngine;
using System.Collections.Generic;

public class MiscTools
{
    public static List<T> ShuffleListFisherYates<T>(List<T> inList)
    {
        // not sure if explicit seed is needed...
        //int seed = (int)System.DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        //Random.InitState(seed);
        List<T> inListCopy = new List<T>(inList);

        for (int i = inListCopy.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (inListCopy[i], inListCopy[j]) = (inListCopy[j], inListCopy[i]);
        }
        return inListCopy;
    }

    public static List<T> GetUniqueRandomElements<T>(List<T> inList, int count)
    {
        if (count < 1)
        {
            Debug.LogWarning("Count must be > 0 -- setting to 1");
            count = 1;
        }
        List<T> outList = new List<T>();
        List<T> shuffledInListCopy = ShuffleListFisherYates<T>(inList);
        outList = shuffledInListCopy.GetRange(0, count);
        return outList;
    }
}
