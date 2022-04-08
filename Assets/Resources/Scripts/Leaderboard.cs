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
    public float[] textLenght; // 2, 3, 4, 5 players { 0.45f, 0.3f, 0.225f, 0.18f}
    public float percentMargin = 0.03f;
    public float betweenTwoTextBoxes = 0.02f; 
    private const float anchorPanelDefault = 0.176f;
    public float anchorPanel;
    private Color[] colorTable = new Color[5] { new Color(0, 1, 0, 1), new Color(1, 0.92f, 0.016f, 1), new Color(1, 0, 0, 1), new Color(1, 0, 1, 1), new Color(1, 1, 1, 1) };
    [SerializeField] GameObject textBoxePrefab;
    private GameObject[] textBoxes;
    private GameObject[] PointTextBoxes;
    private int restantTrophies;
    // make a dictionary with in key: constantIndex; value: playerIndexOfPhotonPlayerList
    private Dictionary<int, int> constantIndexDictionary = new Dictionary<int, int>();
    void Start()
    {
        // add all the players to the dictionary
        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        {
            constantIndexDictionary.Add(i, i);
        }
        LeaderboardPanel = GameObject.FindWithTag("LeaderboardPanel");
        Spawning = GameObject.FindWithTag("Spawning");
        restantTrophies = StaticData.trophiesInGame;
        Debug.Log("27restantTrophies = " + restantTrophies);
        textLenght = new float[4] { 0.45f, 0.3f, 0.225f, 0.18f}; // { 0.225f, 0.15f, 0.1125f, 0.09f };
        InstantiateLeaderboard();
    }

    public void InstantiateLeaderboard(){
        int numOfPlayers = (int)PhotonNetwork.CurrentRoom.CustomProperties["MaxPlayersPerRoom"];
        anchorPanel = anchorPanelDefault * numOfPlayers / 2;
        
        LeaderboardPanel.GetComponent<RectTransform>().anchorMin = new Vector2(0.02f, 0.97f - anchorPanel);
        LeaderboardPanel.GetComponent<RectTransform>().anchorMax = new Vector2(0.2f, 0.97f);
        
        betweenTwoTextBoxes = (float)((1 - (2 * percentMargin) - (numOfPlayers * textLenght[numOfPlayers - 2])) / (numOfPlayers - 1));
        Debug.Log("Leaderboard script 52:\nbetweenTwoTextBoxes: " + betweenTwoTextBoxes + "\npercentMargin: " + percentMargin + "\ntextLenght: " + textLenght[numOfPlayers - 2] + "\nnumOfPlayers: " + numOfPlayers);
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
            textBox.GetComponent<RectTransform>().anchorMax = new Vector2(anchorXMax, 1 - percentMargin - (i * (textLenght[numOfPlayers - 2] + betweenTwoTextBoxes)) - textLenght[numOfPlayers - 2] * 0.3f); // pay attention when there is three players.   - (textLenght[numOfPlayers - 2] * (5 - numOfPlayers)) / 10
            textBox.GetComponent<RectTransform>().anchorMin = new Vector2(anchorXMin, 1 - percentMargin - (i * (textLenght[numOfPlayers - 2] + betweenTwoTextBoxes)) - textLenght[numOfPlayers - 2] * 0.7f); //  + (textLenght[numOfPlayers - 2] * (5 - numOfPlayers)) / 10
            textBox.GetComponent<RectTransform>().localPosition = new Vector3(0, 0, 0);
            textBox.GetComponent<RectTransform>().sizeDelta = new Vector2(textLenght[numOfPlayers - 2] * 0.4f, anchorXMax - anchorXMin);    //  - (textLenght[numOfPlayers - 2] * (5 - numOfPlayers)) / 5 
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
            textBoxes[i].GetComponent<Text>().color = colorTable[(int)PhotonNetwork.PlayerList[i].CustomProperties["Index"]];
            PointTextBoxes[i].GetComponent<Text>().text = PhotonNetwork.PlayerList[i].CustomProperties["Point"].ToString();
        }
    }

    public void UpdateOtherPoints(Player player){
        Debug.Log("Player " + player.NickName + " has changed his points to " + player.CustomProperties["Point"]);
        if (player.NickName == textBoxes[constantIndexDictionary[(int)player.CustomProperties["Index"]]].GetComponent<Text>().text) // add something that check if it is not me?
        {
            PointTextBoxes[constantIndexDictionary[(int)player.CustomProperties["Index"]]].GetComponent<Text>().text = player.CustomProperties["Point"].ToString();
            restantTrophies--;
            Debug.Log("96restantTrophies: " + restantTrophies + "\nplayer.NickName: " + player.NickName);
            CheckGameEnd();
        }
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        StaticData.playerPoints.Add("B" + otherPlayer.NickName, (int)otherPlayer.CustomProperties["Point"]); // B - for black. It means he quit the game before the end
        foreach (Transform child in LeaderboardPanel.transform)
        {
            Destroy(child.gameObject);
        }
        if (PhotonNetwork.PlayerList.Length <= 1)
        {
            // set the player that didn't quit the game to the winner. (give him the restant trophies)
            foreach (Player player in PhotonNetwork.PlayerList)
            {
                if (player.ActorNumber != otherPlayer.ActorNumber)
                {
                    player.CustomProperties["Point"] = (int)player.CustomProperties["Point"] + restantTrophies;
                }
            }
            FinishGame();
            return;
        }
        // change room properties variable max players
        // PhotonNetwork.CurrentRoom.MaxPlayers = (byte)PhotonNetwork.PlayerList.Length;
        PhotonNetwork.CurrentRoom.CustomProperties["MaxPlayersPerRoom"] = PhotonNetwork.PlayerList.Length;
        if (otherPlayer.NickName == textBoxes[constantIndexDictionary[(int)otherPlayer.CustomProperties["Index"]]].GetComponent<Text>().text)
        {
            // go over the player list and change their index in custom properties
            for (int i = (int)otherPlayer.CustomProperties["Index"]; i < PhotonNetwork.PlayerList.Length; i++)
            {
                // putting a new index in the player custom properties
                // ExitGames.Client.Photon.Hashtable playerCustomProperties = PhotonNetwork.PlayerList[i].CustomProperties;
                // playerCustomProperties["Index"] = i;
                // PhotonNetwork.PlayerList[i].SetCustomProperties(playerCustomProperties);
                
                // PhotonNetwork.PlayerList[i].CustomProperties["Index"] = i;
                constantIndexDictionary[(int)PhotonNetwork.PlayerList[i].CustomProperties["Index"]] = i;
            }
            for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
            {
                Debug.Log("Leaderboard29: " + PhotonNetwork.PlayerList[constantIndexDictionary[i]].CustomProperties["Point"]);
            }
            InstantiateLeaderboard();
            CheckGameEnd();
        }
        foreach (GameObject otherPlayerGameObject in GameObject.FindGameObjectsWithTag("Player"))
        {
            if (otherPlayerGameObject.GetComponent<PhotonView>().Owner.ActorNumber == otherPlayer.ActorNumber)
            {
                Destroy(otherPlayerGameObject);
            }
        }
    }
    public void CheckGameEnd()
    {
        if (PhotonNetwork.PlayerList.Length == 1 || restantTrophies == 0)
        {
            FinishGame();
            return;
        }
        int firstPlayerPoints = 0;
        int secondPlayerPoints = 0;
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            if ((int)player.CustomProperties["Point"] > firstPlayerPoints)
            {
                secondPlayerPoints = firstPlayerPoints;
                firstPlayerPoints = (int)player.CustomProperties["Point"];
            }
            else if ((int)player.CustomProperties["Point"] > secondPlayerPoints)
            {
                secondPlayerPoints = (int)player.CustomProperties["Point"];
            }
        }
        if (firstPlayerPoints > restantTrophies + secondPlayerPoints)
        {
            FinishGame();
        }
        return;
    }

    public void FinishGame()
    {
        SaveData();
        if (PhotonNetwork.IsMasterClient){
            PhotonNetwork.LoadLevel("Lobby");
        }
    }

    public void SaveData(){
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            StaticData.playerPoints.Add(player.CustomProperties["ColorName"] + player.NickName, (int)player.CustomProperties["Point"]);
        }
    }
}