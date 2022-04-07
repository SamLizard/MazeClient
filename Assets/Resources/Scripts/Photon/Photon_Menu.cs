using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;
using Photon.Realtime;
using System;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Photon_Menu : MonoBehaviourPunCallbacks
{
    [SerializeField] private GameObject findOpponentPanel = null;
    [SerializeField] private GameObject waitingStatusPanel = null;
    [SerializeField] private TextMeshProUGUI waitingStatusText = null;
    [SerializeField] GameObject textBoxPrefab;
    [SerializeField] GameObject TextBoxAndField;
    [SerializeField] private GameObject masterPanel = null;
    [SerializeField] private GameObject Canvas = null;
    public GameObject PlayersJoinedTextBox = null;
    private bool isConnecting = false;
    private const string GameVersion = "0.1";
    public int MaxPlayersPerRoom = 2;
    private const int defaultMaxPlayers = 2;
    private int[,] playersPosition;
    // private int playersPositionIndex = 0;
    private Dictionary<string, Color> colorList = new Dictionary<string, Color>() { { "G", Color.green }, { "R", Color.red }, { "P", new Color(1, 0, 1, 1) }, { "Y", Color.yellow }, { "B", Color.black}, { "W", Color.white} };
    private readonly Tuple<int, int>[] positions = new Tuple<int, int>[4] {
        new Tuple<int, int>(1, 1),
        new Tuple<int, int>(-1, -1),
        new Tuple<int, int>(-1, 1),
        new Tuple<int, int>(1, -1)
    };
    private int[] rotation = new int[4] {45, 225, 315, 135};
    private string[] colorName = new string[4] {"G", "Y", "R", "P"};
    private readonly int square_size = 10;
    public int size;

    private void Awake() => PhotonNetwork.AutomaticallySyncScene = true;

    void Update()
    {
        if (Canvas.transform.Find("Panel_Winning").gameObject.active && Cursor.visible == false)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            Debug.Log("Cursor was not visible.");
        }
    }

    private void Start()
    {
        StaticData.CleanTable();
        if (StaticData.firstTimeMenu)
        {
            StaticData.firstTimeMenu = false;
            if (StaticData.commingFromAloneMode)
            {
                Canvas.transform.Find("Panel_NameInput").gameObject.SetActive(false);
                Canvas.transform.Find("Panel_FindOpponent").gameObject.SetActive(true);
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            findOpponentPanel.SetActive(false);
            Canvas.transform.Find("Image").gameObject.SetActive(false);
            Canvas.transform.Find("Panel_NameInput").gameObject.SetActive(false);
            Canvas.transform.Find("Panel_Winning").gameObject.SetActive(true);
            List<KeyValuePair<string, int>> list = new List<KeyValuePair<string, int>>(StaticData.Classement());
            // add a textBox for each list element
            int i = 0;
            foreach (KeyValuePair<string, int> pair in list)
            {
                // add a text box with the name and the points
                addTextBox(pair.Key.Substring(1), pair.Value, i, false, colorList[pair.Key[0].ToString()], "Finnish_Background", Canvas.transform.Find("Panel_Winning").gameObject);
                i++;
            }
            if ((int)PhotonNetwork.LocalPlayer.CustomProperties["Point"] == (int)list[0].Value) 
            {
                Canvas.transform.Find("Panel_Winning").gameObject.transform.Find("Status_Winning_Text").gameObject.GetComponent<Text>().text = "You won";
            }
            else
            {
                Canvas.transform.Find("Panel_Winning").gameObject.transform.Find("Status_Winning_Text").gameObject.GetComponent<Text>().text = "Game Over";
            }
            StaticData.CleanList();
            PhotonNetwork.LeaveRoom();
        }
    }

    public void ActivateMenu(){
        Cursor.visible = true;
        findOpponentPanel.SetActive(true);
        Canvas.transform.Find("Panel_Winning").gameObject.SetActive(false);
        Canvas.transform.Find("Image").gameObject.SetActive(true);
    }

    public void FindOpponent()
    {
        isConnecting = true;

        findOpponentPanel.SetActive(false);
        waitingStatusPanel.SetActive(true);

        waitingStatusText.text = "Searching...";

        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.JoinRandomRoom();
        }
        else
        {
            PhotonNetwork.GameVersion = GameVersion;
            PhotonNetwork.ConnectUsingSettings();
        }
    }

    public void CreateNewRoom()
    {
        SceneManager.LoadScene("GameAlone");
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to Master");
        if (isConnecting)
        {
            PhotonNetwork.JoinRandomRoom();
        }
    }

    public override void OnDisconnected(Photon.Realtime.DisconnectCause cause)
    {
        waitingStatusPanel.SetActive(false);
        findOpponentPanel.SetActive(true);
        // send message to the other players that you disconnected
        Debug.Log($"Disctonnected due to: {cause}");
    }

    public override void OnPlayerLeftRoom(Player otherPlayer){
        Debug.Log($"{otherPlayer.NickName} left the room");
        if (otherPlayer.IsInactive){
            Debug.Log($"{otherPlayer.NickName} is inactive");
        }
        // was otherplayer the master? use custom property
        if (PhotonNetwork.IsMasterClient && otherPlayer.CustomProperties["Master"] != null && (bool)otherPlayer.CustomProperties["Master"]){
            Debug.Log(otherPlayer.NickName + " is the master: " + otherPlayer.CustomProperties["Master"]);
            // activate the master panel
            masterPanel.SetActive(true);
            // desactivate the panel waitingStatusPanel and the findOpponentPanel
            waitingStatusPanel.SetActive(false);
            findOpponentPanel.SetActive(false);
            // create the master panel items
            GameObject maxPlayersTextBox = addTextBox("Max Players", MaxPlayersPerRoom, 0, true, new Color(0, 0, 0.5f), "RoomInformations", masterPanel);
            GameObject trophiesTextBox = addTextBox("Number of trophies", 2 * MaxPlayersPerRoom + 1, 1, false, new Color(0, 0, 0.5f), "RoomInformations", masterPanel);
            PlayersJoinedTextBox = addTextBox("Players that alredy joined", PhotonNetwork.CurrentRoom.PlayerCount, 2, false, new Color(0, 0, 0.5f), "RoomInformations", masterPanel);
        }
        else if(PhotonNetwork.IsMasterClient){
            PlayersJoinedTextBox.GetComponent<Text>().text = "Players that already joined: " + PhotonNetwork.CurrentRoom.PlayerCount.ToString();
        }
        else{
            waitingStatusText.text = "Waiting for opponents.\n" + PhotonNetwork.CurrentRoom.PlayerCount + " / " + MaxPlayersPerRoom + " players.";
        }
    }

    public int calculateMazeSize()
    {
        Debug.Log("172CalculateMazeSize. MaxPlayersPerRoom is: " + MaxPlayersPerRoom + ".");
        return 12 * MaxPlayersPerRoom + 1;
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log("No clients are waiting  for an opponent, creating a new room\n" + message);

        PhotonNetwork.CreateRoom(null, new RoomOptions { MaxPlayers = (byte)MaxPlayersPerRoom });
    }

    IEnumerator GenerateMaze()
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            Debug.Log("You are not the Master");
            yield break;
        }
        Debug.Log("Master generate maze coroutine started");
        size = calculateMazeSize();
        StaticData.mazeSize = size;
        Static_Methods.size = size;
        int[,] table = Static_Methods.CreateMazeList();
        string tableString = Static_Methods.TableToString(table);
        Debug.Log("Table already generated. \nSize:" + size + "\n" + tableString);
        StaticData.PutStringInTable(tableString, MaxPlayersPerRoom);
    }

    private int[,] GeneratePlayerPositions(int[,] playersPositionToActualize){
        if (!PhotonNetwork.IsMasterClient){
            return playersPositionToActualize;
        }
        int startingPosition = ((size - (size % 2)) / 2) * -square_size;
        int placeX = startingPosition;
        int placeY = startingPosition;
        for (int i = 0; i < MaxPlayersPerRoom; i++)
        {
            if (i < 4){
                (int x, int y) = positions[i];
                placeX = x * startingPosition;
                placeY = y * startingPosition;
            }
            else{
                placeX = startingPosition + (UnityEngine.Random.Range(0, ((size - (size % 2)) / 2) * square_size));
                placeY = startingPosition + (UnityEngine.Random.Range(0, ((size - (size % 2)) / 2) * square_size));
            }
            playersPositionToActualize[i, 0] = placeX;
            playersPositionToActualize[i, 1] = placeY;
        }
        return playersPositionToActualize;
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Client successfully joined a room");

        int playerCount = PhotonNetwork.CurrentRoom.PlayerCount;

        if (playerCount != MaxPlayersPerRoom)
        {
            waitingStatusText.text = "Waiting for opponents. " + playerCount + " / " + MaxPlayersPerRoom + " players.";
        }
        else
        {
            waitingStatusText.text = "Opponent found";
            Debug.Log("Mach is ready to begin");
        }
        if (!PhotonNetwork.IsMasterClient)
        {
            waitingStatusText.text = "Waiting for Players\n" + playerCount + " / " + PhotonNetwork.CurrentRoom.CustomProperties["MaxPlayersPerRoom"];
        }
        if (PhotonNetwork.IsMasterClient)
        {
            Debug.Log("I am the master");
            Debug.Log("Starting GenerateMaze Coroutine.");
            StartCoroutine(GenerateMaze());
            size = calculateMazeSize();
            playersPosition = new int[MaxPlayersPerRoom, 2];
            playersPosition = GeneratePlayerPositions(playersPosition);
            StaticData.colorName = colorName[0]; 
            // add in roomoptions customproperties the maxplayersperroom using setCustomproperties
            PhotonNetwork.CurrentRoom.SetCustomProperties(new ExitGames.Client.Photon.Hashtable() { { "MaxPlayersPerRoom", MaxPlayersPerRoom } });
            PhotonNetwork.LocalPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable() { { "ColorName", colorName[0] }, { "Index", 0}, { "Point", 0 }, {"Master", true} });
            // playersPositionIndex = 1;
            
            masterPanel.SetActive(true);
            waitingStatusPanel.SetActive(false);
            GameObject maxPlayersTextBox = addTextBox("Max Players", MaxPlayersPerRoom, 0, true, new Color(0, 0, 0.5f), "RoomInformations", masterPanel);
            GameObject trophiesTextBox = addTextBox("Number of trophies", 2 * MaxPlayersPerRoom + 1, 1, false, new Color(0, 0, 0.5f), "RoomInformations", masterPanel);
            PlayersJoinedTextBox = addTextBox("Players that alredy joined", playerCount, 2, false, new Color(0, 0, 0.5f), "RoomInformations", masterPanel);
        }
    }

    public GameObject addTextBox(string textToDisplay, int variableToShow, int index, bool allowInput, Color color, string parent, GameObject panel) // add floats: minX, minY, maxX, maxY, between
    {
        if (allowInput){
            return AddTextBoxAndInput(textToDisplay, variableToShow, index, color, parent, panel);
        }
        // add a textBox with a parent with the name "RoomInformations"
        GameObject textBox = Instantiate(textBoxPrefab, Vector3.zero, Quaternion.identity);
        // put his parent to the gameObject with the name "RoomInformations" that is a child of masterpanel
        textBox.transform.SetParent(panel.transform.Find(parent));
        // change the name of the textBox
        textBox.name = textToDisplay;
        // textBox.transform.SetParent(masterPanel.transform.findinchild("RoomInformations"));
        textBox.GetComponent<Text>().text = textToDisplay + ": " + variableToShow;
        // set anchors
        textBox.GetComponent<RectTransform>().anchorMin = new Vector2(0.2f, 0.67f - (0.13f * index));
        textBox.GetComponent<RectTransform>().anchorMax = new Vector2(0.8f, 0.75f - (0.13f * index));
        // position to zero
        textBox.GetComponent<RectTransform>().localPosition = new Vector3(0, 0, 0);
        textBox.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 0f);    
        textBox.GetComponent<RectTransform>().anchoredPosition3D = Vector3.zero;
        // put his color to dark blue
        textBox.GetComponent<Text>().color = color;
        // activate bestFit
        textBox.GetComponent<Text>().resizeTextForBestFit = true;
        textBox.GetComponent<Text>().resizeTextMinSize = 5;
        textBox.GetComponent<Text>().resizeTextMaxSize = 200;
        return textBox;
    }

    public GameObject AddTextBoxAndInput(string textToDisplay, int variableToShow, int index, Color color, string parentStr, GameObject panel){ // parentStr is the parent of parent
        GameObject parent = Instantiate(TextBoxAndField, Vector3.zero, Quaternion.identity);
        // change the name of parent to textToDisplay
        parent.name = textToDisplay;
        parent.transform.SetParent(panel.transform.Find(parentStr));

        GameObject textBox = parent.transform.Find("TextBox").gameObject;
        GameObject inputField = parent.transform.Find("InputField").gameObject;

        parent.GetComponent<RectTransform>().anchorMin = new Vector2(0.2f, 0.67f - (0.13f * index));
        parent.GetComponent<RectTransform>().anchorMax = new Vector2(0.8f, 0.75f - (0.13f * index));
        parent.GetComponent<RectTransform>().localPosition = new Vector3(0, 0, 0);
        parent.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 0f);    
        parent.GetComponent<RectTransform>().anchoredPosition3D = Vector3.zero;

        // set the color of the textBox and inputField to dark blue
        textBox.GetComponent<Text>().color = color;
        // set the color of the inputfield child named text to dark blue
        inputField.transform.Find("Text").GetComponent<Text>().color = color;
        // activate bestFit of the textBox
        textBox.GetComponent<Text>().resizeTextForBestFit = true;
        textBox.GetComponent<Text>().resizeTextMinSize = 5;
        textBox.GetComponent<Text>().resizeTextMaxSize = 200;

        // set the text of the textBox
        textBox.GetComponent<Text>().text = textToDisplay + ": " + variableToShow;

        // set the text of the inputField
        inputField.GetComponent<InputField>().text = variableToShow.ToString();
        // activate the inputfield on value changed, chose the function oninputChanged that is in the script Photon_Menu in the Canvas_Menu in scene
        inputField.GetComponent<InputField>().onValueChanged.AddListener(delegate { onInputChanged(); });

        return parent;
    }

    public void onInputChanged(){
        GameObject inputFieldMaxPlayers = masterPanel.transform.Find("RoomInformations").Find("Max Players").Find("InputField").GetComponent<InputField>().gameObject;
        Debug.Log("Input of Max Players changed to: " + inputFieldMaxPlayers.GetComponent<InputField>().text);
        CheckChangingMaxPlayers(inputFieldMaxPlayers);
    }

    public void CheckChangingMaxPlayers(GameObject inputFieldMaxPlayers){
        if (int.TryParse(inputFieldMaxPlayers.GetComponent<InputField>().text, out int playersInInput)){
            int playerCount = PhotonNetwork.CurrentRoom.PlayerCount;
            if (playersInInput >= playerCount && playersInInput <= 5 && playersInInput >= 2) {
                MaxPlayersPerRoom = playersInInput;
                // change the maxplayersperroom in the room properties
                PhotonNetwork.CurrentRoom.SetCustomProperties(new ExitGames.Client.Photon.Hashtable() { { "MaxPlayersPerRoom", MaxPlayersPerRoom } });
                // change the text of textbox that contain the number of trophies
                masterPanel.transform.Find("RoomInformations").Find("Number of trophies").GetComponent<Text>().text = "Number of trophies: " + (2 * MaxPlayersPerRoom + 1); //#// put it in a method
                // say to photon that it can let players enter until the new maxplayers is reached
                PhotonNetwork.CurrentRoom.MaxPlayers = (byte)MaxPlayersPerRoom;
                // actualize the text of the textBox
                masterPanel.transform.Find("RoomInformations").Find("Max Players").Find("TextBox").GetComponent<Text>().text = "Max Players: " + MaxPlayersPerRoom;
                // if is equal to the actual number of players that are connected, start the game
                // string nameOfTable = "Table" + MaxPlayersPerRoom.ToString();
                
                size = calculateMazeSize();
                playersPosition = new int[MaxPlayersPerRoom, 2];
                playersPosition = GeneratePlayerPositions(playersPosition);
                
                if (StaticData.IsEmptyAt(MaxPlayersPerRoom) && MaxPlayersPerRoom != defaultMaxPlayers) { 
                    Debug.Log("Generating a Maze when the max players equals: " + MaxPlayersPerRoom);
                    StartCoroutine(GenerateMaze());
                }
                else
                {
                    if (!StaticData.IsEmptyAt(MaxPlayersPerRoom)) //  && MaxPlayersPerRoom == defaultMaxPlayers
                    {
                        size = calculateMazeSize();
                        Static_Methods.size = size;
                        StaticData.mazeSize = size;
                    }
                }
                if (playersInInput == playerCount){
                    Debug.Log("##Closing game. Max players equals: " + MaxPlayersPerRoom + " and player count equals: " + playerCount);
                    PhotonNetwork.CurrentRoom.IsOpen = false;

                    waitingStatusText.text = "Opponent found";
                    Debug.Log("Mach is ready to begin");

                    if (PhotonNetwork.IsMasterClient) // it has to be alredy the master
                    {
                        double timeOfTimer = 3; //in seconds
                        if (StaticData.IsEmptyAt(MaxPlayersPerRoom))
                        {
                            timeOfTimer += 2;
                        }
                        DateTime timeToStart = CalculateStartingTime(timeOfTimer);
                        // calculate the time (take the local seconds, add the timeOfTimer, around it to the highest)
                        Debug.Log("Master - TimeToStart: " + timeToStart.ToString("dd/MM/yyyy HH:mm:ss"));

                        // pun rpc everyone else
                        this.photonView.RPC("StartClientTimer", RpcTarget.Others, timeToStart.ToString("dd/MM/yyyy HH:mm:ss"));

                        StartCoroutine(StartTimer(timeToStart));
                    }
                }
            }
        }
    }

    public DateTime Truncate(DateTime dateTime, TimeSpan timeSpan)
    {
        if (timeSpan == TimeSpan.Zero) return dateTime; // Or could throw an ArgumentException
        if (dateTime == DateTime.MinValue || dateTime == DateTime.MaxValue) return dateTime; // do not modify "guard" values
        return dateTime.AddTicks(-(dateTime.Ticks % timeSpan.Ticks));
    }

    public DateTime CalculateStartingTime(double timeOfTimer)
    {
        DateTime timeToStart = DateTime.Now.AddSeconds(timeOfTimer);
        return Truncate(timeToStart, TimeSpan.FromSeconds(1));
    }

    public int timeLeft(DateTime fromTime)
    {
        return fromTime.Second - DateTime.Now.Second;
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        if (!PhotonNetwork.IsMasterClient){
            Debug.Log("A Player entered the room, but I am not the master");
            return;
        }
        // change the text in playersJoinedTextBox
        PlayersJoinedTextBox.GetComponent<Text>().text = "Players that alredy joined: " + PhotonNetwork.CurrentRoom.PlayerCount;
        // add custom properties to the player
        newPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable() { { "ColorName", colorName[PhotonNetwork.CurrentRoom.PlayerCount - 1] }, { "Index", PhotonNetwork.CurrentRoom.PlayerCount - 1}, { "Point", 0 }, {"Master", newPlayer.IsMasterClient} });
        Debug.Log("Photon_Menu - Giving index:" + ((int)PhotonNetwork.CurrentRoom.PlayerCount - 1).ToString() + " to " + newPlayer.NickName);
        this.photonView.RPC("ChangePlayerVariables", newPlayer, PhotonNetwork.CurrentRoom.PlayerCount - 1); 
        // playersPositionIndex++;
        
        if (PhotonNetwork.CurrentRoom.PlayerCount == MaxPlayersPerRoom)
        {
            Debug.Log("##Photon_Menu - Max players reached: " + MaxPlayersPerRoom + " - Starting the game with " + PhotonNetwork.CurrentRoom.PlayerCount + " players.");
            PhotonNetwork.CurrentRoom.IsOpen = false;

            waitingStatusText.text = "Opponent found";
            Debug.Log("Mach is ready to begin");

            if (PhotonNetwork.IsMasterClient)
            {
                double timeOfTimer = 3; //in seconds
                if (StaticData.IsEmptyAt(MaxPlayersPerRoom))
                {
                    timeOfTimer += 2;
                }
                DateTime timeToStart = CalculateStartingTime(timeOfTimer);
                // calculate the time (take the local seconds, add the timeOfTimer, around it to the highest)
                Debug.Log("Master - TimeToStart: " + timeToStart.ToString("dd/MM/yyyy HH:mm:ss"));

                // pun rpc everyone else
                this.photonView.RPC("StartClientTimer", RpcTarget.Others, timeToStart.ToString("dd/MM/yyyy HH:mm:ss"));

                // remaking the table of playerPosition if the maxPlayers is changed
                if (MaxPlayersPerRoom != defaultMaxPlayers)
                {
                    size = calculateMazeSize();
                    playersPosition = new int[MaxPlayersPerRoom, 2];
                    playersPosition = GeneratePlayerPositions(playersPosition);
                }
                StartCoroutine(StartTimer(timeToStart));
            }
        }
    }

    public void sendRPCtoClients(){
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            if (player.CustomProperties["Index"] == null){
                this.photonView.RPC("ChangePlayerPosition", player, playersPosition[(int)PhotonNetwork.CurrentRoom.CustomProperties["MaxPlayersPerRoom"] - 1, 0], playersPosition[(int)PhotonNetwork.CurrentRoom.CustomProperties["MaxPlayersPerRoom"] - 1, 1], rotation[((int)PhotonNetwork.CurrentRoom.CustomProperties["MaxPlayersPerRoom"] - 1) % 4]);
            }
            else{  
                Debug.Log("Index is :" + player.CustomProperties["Index"].ToString() + ".\n PlayersPosition length is: " + playersPosition.GetLength(0));
                this.photonView.RPC("ChangePlayerPosition", player, playersPosition[(int)player.CustomProperties["Index"], 0], playersPosition[(int)player.CustomProperties["Index"], 1], rotation[(int)player.CustomProperties["Index"] % 4]);
            }
        }
    }

    IEnumerator StartTimer(DateTime timeToStart)
    {
        int restantSeconds = timeLeft(timeToStart);
        waitingStatusText.text = "Game Start in " + restantSeconds + " seconds";
        for (int i = 0; i < restantSeconds; i++)
        {
            yield return new WaitUntil(() => (timeLeft(timeToStart) < restantSeconds));
            restantSeconds = timeLeft(timeToStart);
            waitingStatusText.text = "Game Start in " + restantSeconds + " seconds";
            if (DateTime.Now > timeToStart)
            {
                Debug.Log("Master - Actual time: " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + "\nTimeToStart: " + timeToStart.ToString("dd/MM/yyyy HH:mm:ss"));
                waitingStatusText.text = "Game Start in 0 seconds";
                break;
            }
            if (restantSeconds <= 2 && PhotonNetwork.IsMasterClient){
                sendRPCtoClients();
            }
        }
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.LoadLevel("Game");
        }
    }

    [PunRPC]
    public void StartClientTimer(String timeString)
    {
        System.Globalization.CultureInfo culture = new System.Globalization.CultureInfo("fr-FR");
        DateTime timeToStart = Convert.ToDateTime(timeString, culture);
        StartCoroutine(StartTimer(timeToStart));
    }

    [PunRPC]
    public void ChangePlayerVariables(int index)  
    {
        Debug.Log("Photon_Menu - Index given is: " + index);
	    if (index <= 4){
	        StaticData.colorName = colorName[index];
	    }
	    else {
	        StaticData.colorName = "W";
	    }
    }

    [PunRPC]
    public void ChangePlayerPosition(int placeX, int placeY, int rotation){
        StaticData.position = new Vector3(placeX, 0.5f, placeY);
        StaticData.rotation = rotation;
        StaticData.trophiesInGame = 2 * (int)PhotonNetwork.CurrentRoom.CustomProperties["MaxPlayersPerRoom"] + 1; //#// change it to check from a method so I just have to change the method to change it everywhere
    }
}