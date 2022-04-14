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
    private Dictionary<int, int> constantIndexDictionary = new Dictionary<int, int>();
    void Start()
    {
        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        {
            constantIndexDictionary.Add(i, i);
        }
        LeaderboardPanel = GameObject.FindWithTag("LeaderboardPanel");
        Spawning = GameObject.FindWithTag("Spawning");
        restantTrophies = StaticData.trophiesInGame;
        textLenght = new float[4] { 0.45f, 0.3f, 0.225f, 0.18f};
        InstantiateLeaderboard();
    }

    public void InstantiateLeaderboard(){
        int numOfPlayers = (int)PhotonNetwork.CurrentRoom.MaxPlayers;
        anchorPanel = anchorPanelDefault * numOfPlayers / 2;
        
        LeaderboardPanel.GetComponent<RectTransform>().anchorMin = new Vector2(0.02f, 0.97f - anchorPanel);
        LeaderboardPanel.GetComponent<RectTransform>().anchorMax = new Vector2(0.2f, 0.97f);
        
        betweenTwoTextBoxes = (float)((1 - (2 * percentMargin) - (numOfPlayers * textLenght[numOfPlayers - 2])) / (numOfPlayers - 1));
        textBoxes = new GameObject[numOfPlayers];
        PointTextBoxes = new GameObject[numOfPlayers];
        SetTextBoxes(numOfPlayers, 0.15f, 0.75f, textBoxes);
        SetTextBoxes(numOfPlayers, 0.8f, 0.95f, PointTextBoxes);
        SetPlayerNamesAndPoints(numOfPlayers);
    }

    public void SetTextBoxes(int numOfPlayers, float anchorXMin, float anchorXMax, GameObject[] boxesTable)
    {
        for (int i = 0; i < numOfPlayers; i++)
        {
            GameObject textBox = Instantiate(textBoxePrefab, Vector3.zero, Quaternion.identity);
            textBox.transform.SetParent(LeaderboardPanel.transform);
            textBox.GetComponent<RectTransform>().anchorMax = new Vector2(anchorXMax, 1 - percentMargin - (i * (textLenght[numOfPlayers - 2] + betweenTwoTextBoxes)) - textLenght[numOfPlayers - 2] * 0.3f);
            textBox.GetComponent<RectTransform>().anchorMin = new Vector2(anchorXMin, 1 - percentMargin - (i * (textLenght[numOfPlayers - 2] + betweenTwoTextBoxes)) - textLenght[numOfPlayers - 2] * 0.7f);
            textBox.GetComponent<RectTransform>().localPosition = new Vector3(0, 0, 0);
            textBox.GetComponent<RectTransform>().sizeDelta = new Vector2(textLenght[numOfPlayers - 2] * 0.4f, anchorXMax - anchorXMin);
            textBox.GetComponent<RectTransform>().anchoredPosition3D = Vector3.zero;
            boxesTable[i] = textBox;
        }
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
        if (player.NickName == textBoxes[constantIndexDictionary[(int)player.CustomProperties["Index"]]].GetComponent<Text>().text)
        {
            PointTextBoxes[constantIndexDictionary[(int)player.CustomProperties["Index"]]].GetComponent<Text>().text = player.CustomProperties["Point"].ToString();
            restantTrophies--;
            CheckGameEnd();
        }
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        StaticData.playerPoints.Add("B" + StaticData.playerPoints.Count.ToString() + otherPlayer.NickName, (int)otherPlayer.CustomProperties["Point"]); // B - for black. It means he quit the game before the end
        foreach (Transform child in LeaderboardPanel.transform)
        {
            Destroy(child.gameObject);
        }
        if (PhotonNetwork.PlayerList.Length <= 1)
        {
            PhotonNetwork.LocalPlayer.CustomProperties["Point"] = (int)PhotonNetwork.LocalPlayer.CustomProperties["Point"] + restantTrophies;
            FinishGame();
            return;
        }
        PhotonNetwork.CurrentRoom.MaxPlayers = (byte)PhotonNetwork.PlayerList.Length;
        if (otherPlayer.NickName == textBoxes[constantIndexDictionary[(int)otherPlayer.CustomProperties["Index"]]].GetComponent<Text>().text)
        {
            for (int i = (int)otherPlayer.CustomProperties["Index"]; i < PhotonNetwork.PlayerList.Length; i++)
            {
                constantIndexDictionary[(int)PhotonNetwork.PlayerList[i].CustomProperties["Index"]] = i;
            }
            InstantiateLeaderboard();
            CheckGameEnd();
        }
        foreach (GameObject otherPlayerGameObject in GameObject.FindGameObjectsWithTag("Player"))
        {
            if (otherPlayerGameObject.GetComponent<PhotonView>().Owner.ActorNumber == otherPlayer.ActorNumber)
                Destroy(otherPlayerGameObject);
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
            int playerPoints = (int)player.CustomProperties["Point"];
            if (playerPoints > firstPlayerPoints)
            {
                secondPlayerPoints = firstPlayerPoints;
                firstPlayerPoints = playerPoints;
            }
            else if (playerPoints > secondPlayerPoints)
            {
                secondPlayerPoints = playerPoints;
            }
        }
        if (firstPlayerPoints > restantTrophies + secondPlayerPoints)
            FinishGame();
    }

    public void FinishGame()
    {
        SaveData();
        if (PhotonNetwork.IsMasterClient)
            PhotonNetwork.LoadLevel("Lobby");
    }

    public void SaveData(){
        foreach (Player player in PhotonNetwork.PlayerList)
            StaticData.playerPoints.Add(player.CustomProperties["ColorName"] + StaticData.playerPoints.Count.ToString() + player.NickName, (int)player.CustomProperties["Point"]);
    }
}