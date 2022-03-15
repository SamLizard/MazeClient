using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;

public class Leaderboard : MonoBehaviourPunCallbacks
{
    private GameObject LeaderboardPanel;
    private GameObject Spawning;
    public float[] textLenght = new float[4] { 0.45f, 0.3f, 0.225f, 0.18f}; // 2, 3, 4, 5 players
    public float percentMargin = 0.03f;
    public float betweenTwoTextBoxes = 0.02f; 
    private const float anchorPanelDefault = 0.176f;
    public float anchorPanel;
    private Color[] colorTable = new Color[5] { new Color(0, 1, 0, 1), new Color(1, 0.92f, 0.016f, 1), new Color(1, 0, 0, 1), new Color(1, 0, 1, 1), new Color(0, 0, 0, 0) };
    [SerializeField] GameObject textBoxePrefab;
    private GameObject[] textBoxes;
    private GameObject[] PointTextBoxes;
    private int restantTrophies;
    void Start()
    {
        LeaderboardPanel = GameObject.FindWithTag("LeaderboardPanel");
        Spawning = GameObject.FindWithTag("Spawning");
        restantTrophies = PlayerPrefs.GetInt("Trophies");
        Debug.Log("27restantTrophies = " + restantTrophies);
        InstantiateLeaderboard();
    }

    public void InstantiateLeaderboard(){
        int numOfPlayers = (int)PhotonNetwork.CurrentRoom.CustomProperties["MaxPlayersPerRoom"];
        anchorPanel = anchorPanelDefault * numOfPlayers / 2;
        
        LeaderboardPanel.GetComponent<RectTransform>().anchorMin = new Vector2(0.02f, 0.97f - anchorPanel);
        LeaderboardPanel.GetComponent<RectTransform>().anchorMax = new Vector2(0.2f, 0.97f);
        
        betweenTwoTextBoxes = (float)((1 - (2 * percentMargin) - (numOfPlayers * textLenght[numOfPlayers - 2])) / (numOfPlayers - 1));
        Debug.Log("Leaderboard script 52:\nbetweenTwoTextBoxes: " + betweenTwoTextBoxes);
        textBoxes = new GameObject[numOfPlayers];
        PointTextBoxes = new GameObject[numOfPlayers];
        SetTextBoxes(numOfPlayers, 0.15f, 0.75f, textBoxes);
        SetTextBoxes(numOfPlayers, 0.8f, 0.95f, PointTextBoxes);
        SetPlayerNamesAndPoints(numOfPlayers);
    }

    public GameObject[] SetTextBoxes(int numOfPlayers, float anchorXMin, float anchorXMax, GameObject[] boxesTable)
    {
        Debug.Log("SetTextBoxes. Variables: \nNumOfPlayers: " + numOfPlayers + "\nAnchorXMin:  " + anchorXMin + "\nAnchorXMax: " + anchorXMax);
        for (int i = 0; i < numOfPlayers; i++)
        {
            GameObject textBox = Instantiate(textBoxePrefab, Vector3.zero, Quaternion.identity);
            textBox.transform.SetParent(LeaderboardPanel.transform);
            textBox.GetComponent<RectTransform>().anchorMax = new Vector2(anchorXMax, 1 - percentMargin - (i * (textLenght[numOfPlayers - 2] + betweenTwoTextBoxes)));
            textBox.GetComponent<RectTransform>().anchorMin = new Vector2(anchorXMin, 1 - percentMargin - (i * (textLenght[numOfPlayers - 2] + betweenTwoTextBoxes)) - textLenght[numOfPlayers - 2]);
            textBox.GetComponent<RectTransform>().localPosition = new Vector3(0, 0, 0);
            textBox.GetComponent<RectTransform>().sizeDelta = new Vector2(textLenght[numOfPlayers - 2], anchorXMax - anchorXMin);    
            textBox.GetComponent<RectTransform>().anchoredPosition3D = Vector3.zero;
            Debug.Log("New Text Box created. AnchorMin: " + textBox.GetComponent<RectTransform>().anchorMin + " AnchorMax: " + textBox.GetComponent<RectTransform>().anchorMax + "\nAt index: " + i + "\nOffsetMin: " + textBox.GetComponent<RectTransform>().offsetMin + "\nLocalPosition: " + textBox.GetComponent<RectTransform>().localPosition + "\nPosition: " + textBox.transform.position);
            boxesTable[i] = textBox;
        }
        return boxesTable;
    }

    public void SetPlayerNamesAndPoints(int numOfPlayers)
    {
        for (int i = 0; i < numOfPlayers; i++)
        {
            textBoxes[i].GetComponent<Text>().text = PhotonNetwork.PlayerList[i].NickName;
            textBoxes[i].GetComponent<Text>().color = colorTable[i];
            PointTextBoxes[i].GetComponent<Text>().text = PhotonNetwork.PlayerList[i].CustomProperties["Point"].ToString();
        }
    }

    // override
    public override void OnPlayerPropertiesUpdate (Player target, ExitGames.Client.Photon.Hashtable changedProps)
    {
        Debug.Log("OnPlayerPropertiesUpdate. Player: " + target.NickName + " changed properties: " + changedProps);
        if (changedProps.ContainsKey("Point"))
        {
            Debug.Log("Player " + target.NickName + " has changed his points to " + target.CustomProperties["Point"]);
            if (target.NickName == textBoxes[(int)target.CustomProperties["Index"]].GetComponent<Text>().text)
            {
                PointTextBoxes[(int)target.CustomProperties["Index"]].GetComponent<Text>().text = target.CustomProperties["Point"].ToString();
            }
            Spawning.GetComponent<spawning>().UpdateLeaderboard(target);
            CheckGameEnd();
        }
    }

    public void UpdateOtherPoints(Player player){
        Debug.Log("Player " + player.NickName + " has changed his points to " + player.CustomProperties["Point"]);
        if (player.NickName == textBoxes[(int)player.CustomProperties["Index"]].GetComponent<Text>().text) // check if it is not me
        {
            PointTextBoxes[(int)player.CustomProperties["Index"]].GetComponent<Text>().text = player.CustomProperties["Point"].ToString();
            restantTrophies--;
            Debug.Log("96restantTrophies: " + restantTrophies + "\nplayer.NickName: " + player.NickName);
            CheckGameEnd();
        }
    }

    // override on player left room
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        // add the player to the list of players
        StaticData.playerPoints.Add("B" + otherPlayer.NickName, (int)otherPlayer.CustomProperties["Point"]); // B - for black. It means he quit the game before the end

        // delete all leaderboard childs
        foreach (Transform child in LeaderboardPanel.transform)
        {
            Destroy(child.gameObject);
        }
        Debug.Log("OnPlayerLeftRoom. Player: " + otherPlayer.NickName + ".\nMax Players: " + PhotonNetwork.PlayerList.Length);
        if (PhotonNetwork.PlayerList.Length <= 1)
        {
            // set the player that didn't quit the game to the winner. (give him the restant trophies)
            foreach (Player player in PhotonNetwork.PlayerList)
            {
                if ((int)player.CustomProperties["Index"] != (int)otherPlayer.CustomProperties["Index"])
                {
                    // set a hashtable with the player custom properties of the player that won the game
                    ExitGames.Client.Photon.Hashtable props = player.CustomProperties;
                    props["Point"] = PlayerPrefs.GetInt("Trophies") - (int)otherPlayer.CustomProperties["Point"];
                    foreach (int value in StaticData.playerPoints.Values)
                    {
                        props["Point"] = (int)props["Point"] - value;
                    }
                    player.SetCustomProperties(props);
                }
            }
            FinishGame();
            // Debug.Log("OnPlayerLeftRoom. Player: " + otherPlayer.NickName + ".\nMax Players: " + PhotonNetwork.PlayerList.Length);
            // PhotonNetwork.LeaveRoom();
            // PhotonNetwork.LoadLevel("Lobby");
            return;
        }
        // change room properties variable max players
        PhotonNetwork.CurrentRoom.MaxPlayers = (byte)PhotonNetwork.PlayerList.Length;
        PhotonNetwork.CurrentRoom.CustomProperties["MaxPlayersPerRoom"] = PhotonNetwork.PlayerList.Length;
        if (otherPlayer.NickName == textBoxes[(int)otherPlayer.CustomProperties["Index"]].GetComponent<Text>().text)
        {
            // go over the player list and change their index in custom properties
            for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
            {
                // putting a new index in the player custom properties
                ExitGames.Client.Photon.Hashtable playerCustomProperties = PhotonNetwork.PlayerList[i].CustomProperties;
                playerCustomProperties["Index"] = i;
                PhotonNetwork.PlayerList[i].SetCustomProperties(playerCustomProperties);
            }
            InstantiateLeaderboard();
            CheckGameEnd();
        }
        // go over all player gameobject and delete otherplayer's one.
        foreach (GameObject otherPlayerGameObject in GameObject.FindGameObjectsWithTag("Player"))
        {
            if (otherPlayerGameObject.GetComponent<PhotonView>().Owner.NickName == otherPlayer.NickName)
            {
                Destroy(otherPlayerGameObject);
            }
        }
    }

    // check if the game is done
    public bool CheckGameEnd()
    {
        if (PhotonNetwork.PlayerList.Length == 1)
        {
            // call the finish game function
            FinishGame();
            return true;
        }
        // float maxPoints = (float)PlayerPrefs.GetInt("Trophies") / (float)PhotonNetwork.CurrentRoom.MaxPlayers; // recalculate the max points. there is a problem. ex - 3 players
        // float maxPoints is the max points that each player can have. Maxpoint is the 
        
        bool isFirstPlayerWinner = false;
        bool isTwoFirstEquals = false;
        // it is true if the player with the biggest number of points has more points than: restantTrophies - the trophies the second best player has
        // go over the list of players and put in two variables of type int the points of the first player and the second player
        int firstPlayerPoints = 0;
        int secondPlayerPoints = 0;
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            if ((int)player.CustomProperties["Point"] > firstPlayerPoints)
            {
                firstPlayerPoints = (int)player.CustomProperties["Point"];
            }
            else if ((int)player.CustomProperties["Point"] > secondPlayerPoints)
            {
                secondPlayerPoints = (int)player.CustomProperties["Point"];
            }
        }
        if (firstPlayerPoints >= restantTrophies + secondPlayerPoints)
        {
            if (firstPlayerPoints == secondPlayerPoints)
            {
                if (restantTrophies == 0){
                    isTwoFirstEquals = true;
                }
            }
            else
            {
                isFirstPlayerWinner = true;
            }
        }
        if (isFirstPlayerWinner || isTwoFirstEquals)
        {
            // call the finish game function
            Debug.Log("isFirstPlayerWinner: " + isFirstPlayerWinner + "\nisTwoFirstEquals: " + isTwoFirstEquals + "\nrestantTrophies: " + restantTrophies);
            FinishGame();
            return true;
        }
        return false;
    }

    // method: finish game
    public void FinishGame()
    {
        // call a method that save the data from the game: list of points, winner.
        SaveData();
        PhotonNetwork.LeaveRoom();
        PhotonNetwork.LoadLevel("Lobby");
    }

    // method that save the data from the game: list of points, winner.
    public void SaveData(){
        StaticData.firstTimeMenu = false;
        int maxTrophies = 0;
        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        {
            if (maxTrophies < (int)PhotonNetwork.PlayerList[i].CustomProperties["Point"])
            {
                maxTrophies = (int)PhotonNetwork.PlayerList[i].CustomProperties["Point"];
            }
            StaticData.maxTrophies = maxTrophies;
            StaticData.playerPoints.Add(PhotonNetwork.PlayerList[i].CustomProperties["ColorName"] + PhotonNetwork.PlayerList[i].NickName, (int)PhotonNetwork.PlayerList[i].CustomProperties["Point"]);
        }
    }

    // make this a methode if other script needs it:                     
    // Hashtable props = player.CustomProperties;
    // props["Point"] = PlayerPrefs.GetInt("Trophies") - (int)otherPlayer.CustomProperties["Point"];
    //player.SetCustomProperties(props);
}