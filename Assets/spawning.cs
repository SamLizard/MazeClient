using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System;
using Photon;
using Photon.Realtime;

public class spawning : MonoBehaviourPun
{
    [SerializeField] private GameObject data = null;
    [SerializeField] private GameObject playerPrefab = null;
    [SerializeField] public GameObject LeaderboardPanel;

    private readonly int square_size = 10;
    private int size;

    public GameObject Trophy;

    private int[] playersPoints;
    public int playerIndex = -1;
    public string[] playerNames;

    void Start()
    {
        // instantiate the leaderboard panel
        LeaderboardPanel = Instantiate(LeaderboardPanel, Vector3.zero, Quaternion.identity);
	    playerIndex = PlayerPrefs.GetInt("PlayerIndex");
        Debug.Log("Starting position of player: \nX: " + PlayerPrefs.GetInt("PositionX") + "\nY: " + PlayerPrefs.GetInt("PositionY"));
	    string playerPrefabName = string.Concat(playerPrefab.name, PlayerPrefs.GetString("ColorName"));
        Debug.Log("Player prefab name: " + playerPrefabName + "\nPlayerPrefs: " + PlayerPrefs.GetString("ColorName"));
        GameObject player = PhotonNetwork.Instantiate(playerPrefabName, new Vector3(PlayerPrefs.GetInt("PositionX"), 0, PlayerPrefs.GetInt("PositionY")), Quaternion.identity); // the y is the height (so it is z instead)
        player.transform.rotation = player.transform.rotation * Quaternion.Euler(0, PlayerPrefs.GetInt("RotationY"), 0);
        GameObject photonData = PhotonNetwork.Instantiate("roomData", Vector3.zero, Quaternion.identity);

        if (PhotonNetwork.IsMasterClient)
        {
            instantiatePlayerNameList();
            playersPoints = new int[(PlayerPrefs.GetInt("Size") - 1) / 12];
            for (int i = 0; i < playersPoints.Length; i++)
            {
                playersPoints[i] = 0;
            }
            int tableSize = PlayerPrefs.GetInt("Size");
            size = tableSize;
            string tableString = PlayerPrefs.GetString("Table" + PlayerPrefs.GetInt("MaxPlayersPerRoom").ToString());
            int[,] tableData = stringToTable(tableString);

            photonData.GetComponent<Data>().setTable(tableSize, tableString, PhotonNetwork.IsMasterClient);
            InstansiateTrophies(2 * PlayerPrefs.GetInt("MaxPlayersPerRoom") + 1, tableData);
        }
    }

    public void instantiatePlayerNameList(){
        playerNames = new string[PhotonNetwork.PlayerList.Length];
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            if (playerNames[(int)player.CustomProperties["Index"]] != "")
            {
                Debug.Log(player.NickName + " has the same index that " + playerNames[(int)player.CustomProperties["Index"]] + " has.");
            }
            else
            {
                playerNames[(int)player.CustomProperties["Index"]] = player.NickName;
            }
        }
    }

    public void AddPointTo(int indexPlayer)
    {
        if (PhotonNetwork.IsMasterClient){
            if (indexPlayer >= 0)
            {
	            playersPoints[indexPlayer]++;
                // add in Custom Properties
                ExitGames.Client.Photon.Hashtable playerProperties = PhotonNetwork.PlayerList[indexPlayer].CustomProperties;
                playerProperties["Point"] = (int)playerProperties["Point"] + 1;
                PhotonNetwork.PlayerList[indexPlayer].SetCustomProperties(playerProperties);
            }        
        }
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

    public void InstansiateTrophies(int numOfThrophies, int[,] table)
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
                    GameObject trophy = PhotonNetwork.Instantiate(Trophy.name, new Vector3(initial_place + (row * 2 * square_size), 0.4f, initial_place + (column * 2 * square_size)), Quaternion.identity);
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

    public void UpdateLeaderboard(Player player){
        this.photonView.RPC("UpdateLeaderboardRPC", RpcTarget.Others, player as object);
    }

    [PunRPC]
    public void UpdateLeaderboardRPC(Player player){
        LeaderboardPanel.GetComponent<Leaderboard>().UpdateOtherPoints(player);
    }
}
