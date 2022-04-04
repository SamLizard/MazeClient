using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System;

public class Data : MonoBehaviourPun
{
    private int size;
    public void setTable(int tableSize, string tableString, bool isMaster)
    {
        Debug.Log("IsMaster: " + isMaster + ". TableString" + tableString);
        this.photonView.RPC("InstantiateMazeInActualClient", RpcTarget.All, tableString, tableSize);
    }


    [PunRPC]
    public void InstantiateMazeInActualClient(string tableString, int tableSize)
    {
        Debug.Log("Methode : InstantiateMazeInActualClient started");
        size = tableSize;
        int[,] table = stringToTable(tableString);
        Static_Methods.size = tableSize;
        Debug.Log("Size variable in Static_Methods put to: " + tableSize);
        Static_Methods.InstantiateMaze(table);
    }

    private int[,] stringToTable(string tableString)
    {
        int[,] table = new int[size, size];
        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                table[i, j] = int.Parse(tableString[i * size + j].ToString());
            }
        }
        return table;
    }
}
