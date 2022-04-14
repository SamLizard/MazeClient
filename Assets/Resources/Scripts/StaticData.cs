using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class StaticData
{
    public static bool firstTimeMenu = true;
    public static bool commingFromAloneMode = false; 
    public static string[] table = new string[4];
    public static int mazeSize;
    public static Vector3 position;
    public static int rotation;
    public static int trophiesInGame;
    public static Dictionary<string, int> playerPoints = new Dictionary<string, int>(); // name, points. the first character of the name is the color.

    public static List<KeyValuePair<string, int>> Classement(){
        List<KeyValuePair<string, int>> list = new List<KeyValuePair<string, int>>(playerPoints);
        list.Sort(
            delegate (KeyValuePair<string, int> firstPair, KeyValuePair<string, int> nextPair)
            {
                return nextPair.Value.CompareTo(firstPair.Value);
            }
        );
        return list;
    }

    public static void CleanTable()
    {
        for (int i = 0; i < table.Length; i++)
        {
            table[i] = "";
        }
    }

    public static void PutStringInTable(string str, int position)
    {
        table[position - 2] = str;
    }

    public static bool IsEmptyAt(int position)
    {
        return table[position - 2] == "";
    }
}
