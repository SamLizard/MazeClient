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
    [SerializeField] public GameObject[] squares;
    [SerializeField] public GameObject LeaderboardPanel;

    private readonly int square_size = 10;
    private int size;

    public GameObject Trophy;

    private readonly Tuple<int, int>[] directions = new Tuple<int, int>[4] {
        new Tuple<int, int>(0, -1),
        new Tuple<int, int>(-1, 0),
        new Tuple<int, int>(0, 1),
        new Tuple<int, int>(1, 0)
    };

    private int[] playersPoints;
    public int playerIndex = -1;
    public string[] playerNames;

    void Start()
    {
        // instantiate the leaderboard panel
        LeaderboardPanel = Instantiate(LeaderboardPanel, Vector3.zero, Quaternion.identity);
	    playerIndex = PlayerPrefs.GetInt("PlayerIndex");
        // Color color;
        // ColorUtility.TryParseHtmlString("#" + PlayerPrefs.GetString("Color"), out color);
        Debug.Log("Starting position of player: \nX: " + PlayerPrefs.GetInt("PositionX") + "\nY: " + PlayerPrefs.GetInt("PositionY"));
	    string playerPrefabName = string.Concat(playerPrefab.name, PlayerPrefs.GetString("ColorName"));
        Debug.Log("Player prefab name: " + playerPrefabName + "\nPlayerPrefs: " + PlayerPrefs.GetString("ColorName"));
        GameObject player = PhotonNetwork.Instantiate(playerPrefabName, new Vector3(PlayerPrefs.GetInt("PositionX"), 0, PlayerPrefs.GetInt("PositionY")), Quaternion.identity); // the y is the height (so it is z instead)
        player.transform.rotation = player.transform.rotation * Quaternion.Euler(0, PlayerPrefs.GetInt("RotationY"), 0);
        // player.GetComponent<MeshRenderer>().sharedMaterial.SetColor("_Color", color);
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
            string tableString = PlayerPrefs.GetString("Table");
            int[,] tableData = stringToTable(tableString);

            photonData.GetComponent<Data>().setTable(tableSize, tableString, PhotonNetwork.IsMasterClient);
            InstansiateTrophies(2 * PlayerPrefs.GetInt("MaxPlayersPerRoom") + 1, tableData);
            InstantiateMaze(tableData);
        }
    }

    // instantiate leaderboard. use room.maxPlayers. use void OnPlayerPropertiesUpdate	(Player targetPlayer, Hashtable changedProps) to update. add a photonview?

    public void instantiatePlayerNameList(){
        // index : 0        1       2       3       4
        // name  : sam      sam11   samuel  Sa      Sam01                               // to display
        // Color : G        Y       R       P       W       // use CustomProperties     // use index to set the color in the leaderboard
        // Points: 2        1       3       4       1       // use CustomProperties     // to display // use void OnPlayerPropertiesUpdate	(Player targetPlayer, Hashtable changedProps)	
        playerNames = new string[PhotonNetwork.PlayerList.Length];
        string str = "";
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            if (playerNames[(int)player.CustomProperties["Index"]] != "")
            {
                Debug.Log(player.NickName + " has the same index that " + playerNames[(int)player.CustomProperties["Index"]] + " has.");
            }
            else
            {
                playerNames[(int)player.CustomProperties["Index"]] = player.NickName;
                // str += player.ActorNumber.ToString() + ". " + player.NickName + ". Index: " + player.CustomProperties["Index"].ToString() + "\n";
            }
        }
        Debug.Log("Player names: \n" + str);
    }

    // [PunRPC]
    public void AddPointTo(int indexPlayer)
    {
        if (PhotonNetwork.IsMasterClient){
            if (indexPlayer >= 0)
            {
	            playersPoints[indexPlayer]++;
                // addin
                ExitGames.Client.Photon.Hashtable playerProperties = PhotonNetwork.PlayerList[indexPlayer].CustomProperties;
                playerProperties["Point"] = (int)playerProperties["Point"] + 1;
                PhotonNetwork.PlayerList[indexPlayer].SetCustomProperties(playerProperties);
            }        
            Debug.Log("Player number " + indexPlayer + " has " + playersPoints[indexPlayer] + " points.");
        }
        // Debug.Log(player.NickName);
        // Debug.Log("RPC AddPointTo in spawning. \nPhotonView owner id:" + photonView.Owner.identity);
        // int playerNumber = photonView.owner.GetPlayerNumber();
        
	    Debug.Log("Client see: Player number " + indexPlayer);
	    // call rpc target all clients. Thay add to the player the point
        
	    // Debug.Log("Player " + playerNumber + " has " + playersPoints[playerNumber] + " points");
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

    private bool IsOutOfBounds(int row, int column)
    {
        return row >= size
            || row < 0
            || column >= size
            || column < 0;
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

    private void InstantiateMaze(int[,] table)
    {
        for (int row = 0; row < table.GetLength(0); row++)
        {
            for (int column = 0; column < table.GetLength(0); column++)
            {
                if (table[row, column] == 0) { continue; }

                int[] num_of_item = new int[4] { 1, 1, 1, 1 };
                for (int i = 0; i < directions.Length; i++)
                {
                    (int x, int y) = directions[i];
                    if (!IsOutOfBounds(row + x, column + y) && table[row + x, column + y] != 0)
                    {
                        num_of_item[i] = 0;
                    }
                }
                InstantiateSquare(row, column, table.GetLength(0), num_of_item);
            }
        }
    }

    private void InstantiateSquare(int row, int column, int size, int[] walls)
    {
        int num_of_walls = 0;
        int adjacent_walls = 0;
        int prefab_place = 0;
        int rotation = 0;
        for (int i = 0; i < walls.Length; i++)
        {
            num_of_walls += walls[i];
            if ((i == 0 && walls[i] == 1 && walls[3] == 1) || (i > 0 && walls[i] == 1 && walls[i - 1] == 1))
            {
                adjacent_walls++;
            }
        }
        if (num_of_walls == 1)
        {
            prefab_place = 0;
            foreach (int possible_wall in walls)
            {
                if (possible_wall == 1)
                {
                    break;
                }
                rotation += 90;
            }
        }
        else if (num_of_walls == 2)
        {
            if (adjacent_walls == 0)
            {
                prefab_place = 2;
                if (walls[0] == 0)
                {
                    rotation = 90;
                }
            }
            else
            {
                int zeros_before_first_wall = 0;
                prefab_place = 1;
                foreach (int item in walls)
                {
                    if (item == 0)
                    {
                        zeros_before_first_wall += 1;
                    }
                    else
                    {
                        if (zeros_before_first_wall == 0)
                        {
                            rotation = 180;
                            if (walls[3] == 1)
                            {
                                rotation -= 90;
                            }
                            break;
                        }
                        else
                        {
                            rotation = 360 - (2 - zeros_before_first_wall) * 90;
                            break;
                        }
                    }
                }
            }
        }
        else if (num_of_walls == 3)
        {
            prefab_place = 3;
            rotation = -90;
            foreach (int item in walls)
            {
                if (item == 0)
                {
                    break;
                }
                rotation += 90;
            }
        }
        else
        {
            prefab_place = 4;
        }
        InstantiateMaze(row, column, rotation, prefab_place, size);
    }

    public void InstantiateMaze(int row, int column, int rotation, int prefab_place, int size)
    {
        int initial_place = ((-size / 2) * square_size) + (square_size / 2) - ((square_size / 2) * (size % 2));
        int place_x = initial_place + row * square_size;  // vertical = x
        int place_z = initial_place + column * square_size;  // horizontale = z
        GameObject square = Instantiate(squares[prefab_place], new Vector3(place_x, 0, place_z), Quaternion.identity);
        square.transform.SetParent(GameObject.FindWithTag("Map").transform);
        square.transform.Rotate(0, rotation, 0);
    }
}
