using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System;
using Photon;
using Photon.Realtime;

public class spawning : MonoBehaviourPun
{
    [SerializeField] private GameObject playerPrefab = null;
    [SerializeField] public GameObject LeaderboardPanel;
    private int size;
    private readonly int square_size = 10;

    void Start()
    {
        LeaderboardPanel = Instantiate(LeaderboardPanel, Vector3.zero, Quaternion.identity);
        GameObject player = PhotonNetwork.Instantiate(string.Concat(playerPrefab.name, StaticData.colorName), StaticData.position, Quaternion.Euler(0, StaticData.rotation, 0));
        if (PhotonNetwork.IsMasterClient)
        {
            size = StaticData.mazeSize;
            string tableString = StaticData.table[(int)PhotonNetwork.CurrentRoom.MaxPlayers - 2];
            int[,] tableData = Static_Methods.stringToTable(tableString, size);
            this.photonView.RPC("InstantiateMazeInScene", RpcTarget.All, tableString, size);
            InstansiateTrophies(2 * (int)PhotonNetwork.CurrentRoom.MaxPlayers + 1, tableData, "Trophy");
        }
    }

    public void InstansiateTrophies(int numOfThrophies, int[,] table, string prefab) // make it a recursive method?
    {
        int tablePointSize = (table.GetLength(0) - 1) / 2;
        if (numOfThrophies < (Math.Pow(tablePointSize, 2) / 3))
        {
            int initial_place = (((-size / 2)) * square_size);
            for (int i = 0; i < numOfThrophies; i++)
            {
                int row = UnityEngine.Random.Range(0, tablePointSize);
                int column = UnityEngine.Random.Range(0, tablePointSize);
                if (table[row, column] == 1)
                {
                    GameObject trophy = PhotonNetwork.Instantiate(prefab, new Vector3(initial_place + (row * 2 * square_size), 0.4f, initial_place + (column * 2 * square_size)), Quaternion.identity);
                    trophy.transform.localScale = new Vector3(5, 5, 5);
                    table[row, column] = 2;
                }
                else
                {
                    i--;
                }
            }
        }
    }

    public void AddPointTo(int indexPlayer)
    {
        if (PhotonNetwork.IsMasterClient && indexPlayer >= 0){
            ExitGames.Client.Photon.Hashtable playerProperties = PhotonNetwork.PlayerList[indexPlayer].CustomProperties;
            playerProperties["Point"] = (int)playerProperties["Point"] + 1;
            PhotonNetwork.PlayerList[indexPlayer].SetCustomProperties(playerProperties);
            this.photonView.RPC("UpdateLeaderboardRPC", RpcTarget.All, PhotonNetwork.PlayerList[indexPlayer] as object);
        }
    }

    [PunRPC]
    public void UpdateLeaderboardRPC(Player player){
        LeaderboardPanel.GetComponent<Leaderboard>().UpdateOtherPoints(player);
    }

    [PunRPC]
    public void InstantiateMazeInScene(string tableString, int tableSize)
    {
        Static_Methods.size = tableSize;
        int[,] table = Static_Methods.stringToTable(tableString, tableSize);
        Static_Methods.InstantiateMaze(table);
    }
}
