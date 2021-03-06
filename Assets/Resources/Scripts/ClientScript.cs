using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ClientScript : MonoBehaviour
{
    private readonly int square_size = 10;
    public int size;
    public GameObject PlayerPrefab;

    void Start()
    {
        size = 7;
        int startingPosition = ((size - (1 * size % 2)) / 2) * -square_size;
        PlayerPrefab.transform.position = new Vector3(startingPosition, 0.5f, startingPosition);
        int[,] table = Static_Methods.CreateMazeList(size);
        table[0, 0] = 2; // set the possibilitie of putting here (on the player starting position) a throphy to false;
        Static_Methods.InstantiateMaze(table);
        Static_Methods.InstansiateTrophies(1, table, Resources.Load("Trophy_Alone") as GameObject);
    }

    public void TrophyTaken()
    {
        StaticData.firstTimeMenu = true;
        StaticData.commingFromAloneMode = true;
        SceneManager.LoadScene("Lobby");
    }
}
