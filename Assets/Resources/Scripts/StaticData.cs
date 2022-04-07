using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class StaticData
{
    public static bool firstTimeMenu = true;
    public static bool commingFromAloneMode = false; 
    public static string[] table = new string[4];
    public static string colorName;
    public static int mazeSize;
    public static Vector3 position;
    public static int rotation;
    public static int trophiesInGame;
    public static Dictionary<string, int> playerPoints = new Dictionary<string, int>(); // name, points. the first character of the name is the color.    

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

    public static void CleanList()
    {
        playerPoints.Clear();
    }

    public static void CleanTable()
    {
        for (int i = 0; i < table.Length; i++)
        {
            table[i] = "";
        }
    }

    public static void PutStringInTable(string str, int position) // change position to maxplayerperrooms
    {
        table[position] = str;
    }

    public static bool IsEmptyAt(int position)
    {
        return table[position] == "";
    }
}
