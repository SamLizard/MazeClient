using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class StaticData
{
    public static bool firstTimeMenu = true;
    public static Dictionary<string, int> playerPoints = new Dictionary<string, int>(); // name, points. the first character of the name is the color.    
    public static int maxTrophies;
    public static List<KeyValuePair<string, int>> Classement(){
        // order the playerPoints by value. from the highest to the lowest. The first one is the highest.
        List<KeyValuePair<string, int>> list = new List<KeyValuePair<string, int>>(playerPoints);
        list.Sort(
            delegate (KeyValuePair<string, int> firstPair, KeyValuePair<string, int> nextPair)
            {
                return nextPair.Value.CompareTo(firstPair.Value);
            }
        );
        // Debug.Log the ordered list
        int i = 0;
        foreach (KeyValuePair<string, int> pair in list)
        {
            Debug.Log("24 - StaticData\ni = " + i + ". " + pair.Key + " : " + pair.Value);
            i++;
        }
        return list;
    }
}
