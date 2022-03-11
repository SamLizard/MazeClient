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
    float[] textLenght = new float[4] { 0.45f, 0.3f, 0.225f, 0.18f}; // 2, 3, 4, 5 players
    float percentMargin = 0.03f;
    float betweenTwoTextBoxes = 0.02f; 
    float anchorPanel = 0.176f;
    private Color[] colorTable = new Color[5] { new Color(0, 1, 0, 1), new Color(1, 0.92f, 0.016f, 1), new Color(1, 0, 0, 1), new Color(1, 0, 1, 1), new Color(0, 0, 0, 0) };
    [SerializeField] GameObject textBoxePrefab;
    private GameObject[] textBoxes;
    private GameObject[] PointTextBoxes;
    void Start()
    {
        LeaderboardPanel = GameObject.FindWithTag("LeaderboardPanel");
        Spawning = GameObject.FindWithTag("Spawning");
        
        int numOfPlayers = (int)PhotonNetwork.CurrentRoom.CustomProperties["MaxPlayersPerRoom"];
        anchorPanel = anchorPanel * numOfPlayers / 2;

        LeaderboardPanel.GetComponent<RectTransform>().anchorMin = new Vector2(0.02f, 0.97f - anchorPanel);
        LeaderboardPanel.GetComponent<RectTransform>().anchorMax = new Vector2(0.2f, 0.97f);

        betweenTwoTextBoxes = (float)((1 - (2 * percentMargin) - (numOfPlayers * textLenght[numOfPlayers - 2])) / (numOfPlayers - 1));
        Debug.Log("Leaderboard script 35:\nbetweenTwoTextBoxes: " + betweenTwoTextBoxes);

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
        }
    }

    public void UpdateOtherPoints(Player player){
        Debug.Log("Player " + player.NickName + " has changed his points to " + player.CustomProperties["Point"]);
        if (player.NickName == textBoxes[(int)player.CustomProperties["Index"]].GetComponent<Text>().text)
        {
            PointTextBoxes[(int)player.CustomProperties["Index"]].GetComponent<Text>().text = player.CustomProperties["Point"].ToString();
        }
    }
}
