using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;
using Photon.Realtime;
using System;
using UnityEngine.UI;

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
    private int playersPositionIndex = 0;
    // add a table with key = string, value = color of type Color. It start with the key : "G" and the value : Color.green
    private Dictionary<string, Color> colorList = new Dictionary<string, Color>() { { "G", Color.green }, { "R", Color.red }, { "P", new Color(1, 0, 1, 1) }, { "Y", Color.yellow }, { "B", Color.black}, { "W", Color.white} };

    private readonly Tuple<int, int>[] directions = new Tuple<int, int>[4] {
        new Tuple<int, int>(0, -1),
        new Tuple<int, int>(-1, 0),
        new Tuple<int, int>(0, 1),
        new Tuple<int, int>(1, 0)
    };

    private readonly Tuple<int, int>[] positions = new Tuple<int, int>[4] {
        new Tuple<int, int>(1, 1),
        new Tuple<int, int>(-1, -1),
        new Tuple<int, int>(-1, 1),
        new Tuple<int, int>(1, -1)
    };

    private int[] rotation = new int[4] {45, 225, 135, 315};
    private string[] colorName = new string[4] {"G", "Y", "R", "P"};

    private readonly int[][] RandomDirections = new int[24][] {
        new int[4] {0, 1, 2, 3 }, new int[4] {0, 1, 3, 2 }, new int[4] {0, 2, 1, 3 }, new int[4] {0, 2, 3, 1 }, new int[4] {0, 3, 2, 1 }, new int[4] {0, 3, 1, 2 },
        new int[4] {1, 0, 2, 3 }, new int[4] {1, 0, 3, 2 }, new int[4] {1, 2, 0, 3 }, new int[4] {1, 2, 3, 0 }, new int[4] {1, 3, 2, 0 }, new int[4] {1, 3, 0, 2 },
        new int[4] {2, 0, 1, 3 }, new int[4] {2, 0, 3, 1 }, new int[4] {2, 1, 3, 0 }, new int[4] {2, 1, 0, 3 }, new int[4] {2, 3, 0, 1 }, new int[4] {2, 3, 1, 0 },
        new int[4] {3, 0, 2, 1 }, new int[4] {3, 0, 1, 2 }, new int[4] {3, 1, 2, 0 }, new int[4] {3, 1, 0, 2 }, new int[4] {3, 2, 1, 0 }, new int[4] {3, 2, 0, 1 },
    };

    private readonly int square_size = 10;
    public int size;

    private void Awake() => PhotonNetwork.AutomaticallySyncScene = true;

    private async void Start()
    {
        if (StaticData.firstTimeMenu)
        {
            StaticData.firstTimeMenu = false;
            beforeStarting();
        }
        else
        {
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
            if ((int)PhotonNetwork.LocalPlayer.CustomProperties["Point"] == (int)list[0].Value) // (int)PhotonNetwork.PlayerList[PlayerPrefs.GetInt("Index")].CustomProperties["Point"] is not working. (giving the master one)
            {
                Canvas.transform.Find("Panel_Winning").gameObject.transform.Find("Status_Winning_Text").gameObject.GetComponent<Text>().text = "You won";
            }
            else
            {
                Canvas.transform.Find("Panel_Winning").gameObject.transform.Find("Status_Winning_Text").gameObject.GetComponent<Text>().text = "Game Over";
            }
            PhotonNetwork.LeaveRoom();
        }
    }

    private void beforeStarting(){
        for (int i = 2; i <= 5; i++)
        {
            string nameOfTable = "Table" + i;
            PlayerPrefs.SetString(nameOfTable, "");
        }
    }

    public void ActivateMenu(){
        // make the cursor visible
        Cursor.visible = true;
        findOpponentPanel.SetActive(true);
        // waitingStatusPanel.SetActive(false);
        Canvas.transform.Find("Panel_Winning").gameObject.SetActive(false);
        // set image active
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
        // change the text of the textbox that show the number of players that joined
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
        else{
            Debug.Log(otherPlayer.NickName + " is not the master: " + otherPlayer.CustomProperties["Master"]);
        }
    }

    public int calculateMazeSize()
    {
        return 12 * MaxPlayersPerRoom + 1;
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log("No clients are waiting  for an opponent, creating a new room");

        PhotonNetwork.CreateRoom(null, new RoomOptions { MaxPlayers = (byte)MaxPlayersPerRoom }); // enter as master
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
        PlayerPrefs.SetInt("Size", size);
        int[,] table = CreateMazeList();
        string tableString = TableToString(table);
        Debug.Log("Table already generated. \nSize:" + size + "\n" + tableString);
        PlayerPrefs.SetString("Table" + MaxPlayersPerRoom.ToString(), tableString);
    }

    private int[,] CreateMazeList()
    {
        int[,] table = InitialBoard(size); // integrate this in the Point class
        Point[,] tablePoint = BuildListOfPoint(size);
        int length = tablePoint.GetLength(0);
        int PointRow = UnityEngine.Random.Range(0, length);
        int PointColumn = UnityEngine.Random.Range(0, length);
        int actualDirection;
        Compartiment lastCompartiment = tablePoint[length - 1, length - 1].getCompartiment();

        table = ConnectEveryPoint(table, tablePoint); // connect every point one time
        while (!lastCompartiment.isEqual())
        {
            actualDirection = -1;
            while (actualDirection == -1)
            {
                PointRow = UnityEngine.Random.Range(0, length);
                PointColumn = UnityEngine.Random.Range(0, length);
                if (tablePoint[PointRow, PointColumn].hasRemainingConnections())
                {
                    actualDirection = ChoosePointDirection(tablePoint[PointRow, PointColumn].getDirections(), tablePoint, PointRow, PointColumn);
                }
            }
            UpdatePointTable(tablePoint, actualDirection, PointRow, PointColumn);
            table = UpdateTable(table, actualDirection, PointRow, PointColumn);
        }
        return table;
    }

    private int[,] InitialBoard(int size)
    {
        int[,] table = new int[size + 1 - size % 2, size + 1 - size % 2];
        for (int i = 0; i < size; i += 2)
        {
            for (int j = 0; j < size; j += 2)
            {
                table[i, j] = 1;
            }
        }
        return table;
    }

    private int[,] UpdateTable(int[,] table, int actualDirection, int row, int column)
    {
        row *= 2;
        column *= 2;
        table[row, column] = 1;
        (int x, int y) = directions[actualDirection];
        table[row + x, column + y] = 1;
        table[row + 2 * x, column + 2 * y] = 1;
        return table;
    }

    private Point[,] BuildListOfPoint(int size)
    {
        int tableLength = (int)Math.Ceiling(size / 2.0);
        Point[,] table = new Point[tableLength, tableLength];
        for (int i = 0; i < tableLength; i++)
        {
            for (int j = 0; j < tableLength; j++)
            {
                table[i, j] = new Point(false, i * tableLength + j + 1, i, j, tableLength);
            }
        }
        return table;
    }

    public int[,] ConnectEveryPoint(int[,] table, Point[,] tablePoint)
    {
        int tableLength = tablePoint.GetLength(0);
        for (int row = 0; row < tableLength; row++)
        {
            for (int column = 0; column < tableLength; column++)
            {
                if (!tablePoint[row, column].getConnection())
                {
                    int actualDirection = ChoosePointDirection(tablePoint[row, column].getDirections(), tablePoint, row, column);
                    UpdatePointTable(tablePoint, actualDirection, row, column);
                    table = UpdateTable(table, actualDirection, row, column);
                }
            }
        }
        return table;
    }

    private int ChoosePointDirection(bool[] pointDirections, Point[,] tablePoint, int row, int column)
    {
        int directionIndex = UnityEngine.Random.Range(0, 23);
        foreach (int direction in RandomDirections[directionIndex])
        {
            (int x, int y) = directions[direction];
            if (tablePoint[row, column].canConnect(direction) && tablePoint[row + x, column + y].hasRemainingConnections())
            {
                return direction;
            }
        }
        return -1;
    }

    public void UpdatePointCompartiment(Point actualPoint, Point pointedPoint, Point[,] tablePoint)
    {
        int biggestCompartimentValue = Math.Max(actualPoint.getCompartiment().getValue(), pointedPoint.getCompartiment().getValue());
        if (actualPoint.getCompartiment().getValue() == biggestCompartimentValue)
        {
            Point actualPointToTransfer = actualPoint;
            actualPoint = pointedPoint;
            pointedPoint = actualPointToTransfer;
        }
        if (!actualPoint.getConnection() && !pointedPoint.getConnection())
        {
            actualPoint.setCompartiment(pointedPoint.getCompartiment());
            pointedPoint.getCompartiment().addCount(1);
        }
        else
        {
            Compartiment actualPointLastCompartiment = ConnectToLastCompartimentOf(actualPoint.getCompartiment(), 0);
            Compartiment pointedPointLastCompartiment = ConnectToLastCompartimentOf(pointedPoint.getCompartiment(), 0);
            ConnectCompartiments(actualPointLastCompartiment, pointedPointLastCompartiment);
        }
    }

    private void UpdatePointTable(Point[,] tablePoint, int actualDirection, int row, int column)
    {
        Point actualPoint = tablePoint[row, column];
        (int x, int y) = directions[actualDirection];
        Point pointedPoint = tablePoint[row + x, column + y];
        UpdatePointCompartiment(actualPoint, pointedPoint, tablePoint);
        actualPoint.setConnection();
        pointedPoint.setConnection();
        actualPoint.setDirections(actualDirection);
        pointedPoint.setDirections(InverseDirection(actualDirection));
    }

    private int InverseDirection(int actualDirection)
    {
        return (actualDirection + 2) % 4;
    }

    public Compartiment ConnectToLastCompartimentOf(Compartiment actualCompartiment, int count)
    {
        if (actualCompartiment.getNext() != null)
        {
            Compartiment lastCompartiment = ConnectToLastCompartimentOf(actualCompartiment.getNext(), actualCompartiment.getCount());
            actualCompartiment.setNext(lastCompartiment);
            actualCompartiment.addCount(-count);
            return lastCompartiment;
        }
        return actualCompartiment;
    }

    public void ConnectCompartiments(Compartiment first, Compartiment second)
    {
        if (first.getValue() > second.getValue())
        {
            first.addCount(second.getCount());
            second.setNext(first);
        }
        else
        {
            if (second.getValue() > first.getValue())
            {
                second.addCount(first.getCount());
                first.setNext(second);
            }
        }
    }

    private string TableToString(int[,] table)
    {
        string tableString = "";
        for (int i = 0; i < table.GetLength(0); i++)
        {
            for (int j = 0; j < table.GetLength(0); j++)
            {
                tableString += table[i, j].ToString();
            }
        }
        return tableString;
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
            (int x, int y) = positions[i];
            if (i < 4){
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
            waitingStatusText.text = "Waiting for opponent. " + playerCount + " / " + MaxPlayersPerRoom + " players.";
        }
        else
        {
            waitingStatusText.text = "Opponent found";
            Debug.Log("Mach is ready to begin");
        }
        if (PhotonNetwork.IsMasterClient)
        {
            Debug.Log("I am the master");
            PlayerPrefs.SetInt("MaxPlayersPerRoom", MaxPlayersPerRoom);
            Debug.Log("Starting GenerateMaze Coroutine.");
            StartCoroutine(GenerateMaze());

            playersPosition = new int[MaxPlayersPerRoom, 2];
            playersPosition = GeneratePlayerPositions(playersPosition);
            // PlayerPrefs.SetInt("PositionX", playersPosition[playersPositionIndex, 0]);
            // PlayerPrefs.SetInt("PositionY", playersPosition[playersPositionIndex, 1]);
            // PlayerPrefs.SetInt("RotationY", rotation[playersPositionIndex]);
            PlayerPrefs.SetString("ColorName", colorName[playersPositionIndex]);

            // add in roomoptions customproperties the maxplayersperroom using setCustomproperties
            PhotonNetwork.CurrentRoom.SetCustomProperties(new ExitGames.Client.Photon.Hashtable() { { "MaxPlayersPerRoom", MaxPlayersPerRoom } });
            PhotonNetwork.LocalPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable() { { "ColorName", colorName[playersPositionIndex] }, { "Index", playersPositionIndex}, { "Point", 0 }, {"Master", true} });
            playersPositionIndex = 1;

            // add a textBox that let him change the number of players in the room. Pay attention if he put a smaller number than the actual number of players that are connected (playerCount). dont forget to update the playerprefs and the maxplayersperroom.
            // nothing to do here.
            
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
            if (playersInInput >= playerCount && playersInInput <= 5 && playersInInput >= 2){
                MaxPlayersPerRoom = playersInInput;
                // change the maxplayersperroom in the room properties and PlayerPrefs
                PhotonNetwork.CurrentRoom.SetCustomProperties(new ExitGames.Client.Photon.Hashtable() { { "MaxPlayersPerRoom", MaxPlayersPerRoom } });
                PlayerPrefs.SetInt("MaxPlayersPerRoom", MaxPlayersPerRoom);
                // change the text of textbox that contain the number of trophies
                masterPanel.transform.Find("RoomInformations").Find("Number of trophies").GetComponent<Text>().text = "Number of trophies: " + (2 * MaxPlayersPerRoom + 1);
                PhotonNetwork.CurrentRoom.MaxPlayers = (byte)MaxPlayersPerRoom;
                // actualize the text of the textBox
                masterPanel.transform.Find("RoomInformations").Find("Max Players").Find("TextBox").GetComponent<Text>().text = "Max Players: " + MaxPlayersPerRoom;
                // if is equal to the actual number of players that are connected, start the game
                string nameOfTable = "Table" + MaxPlayersPerRoom.ToString();
                if (PlayerPrefs.GetString(nameOfTable) == "" && MaxPlayersPerRoom != defaultMaxPlayers){
                    Debug.Log("Generating a Maze when the max players equals: " + MaxPlayersPerRoom);
                    StartCoroutine(GenerateMaze());
                }
                else{
                    Debug.Log("I didn't entered in the if because: " + (bool)(PlayerPrefs.GetString(nameOfTable) == "") + " and " + (bool)(MaxPlayersPerRoom != defaultMaxPlayers));
                    Debug.Log("The table is already: " + PlayerPrefs.GetString(nameOfTable));
                }
                if (playersInInput == playerCount){
                    PhotonNetwork.CurrentRoom.IsOpen = false;

                    waitingStatusText.text = "Opponent found";
                    Debug.Log("Mach is ready to begin");

                    if (PhotonNetwork.IsMasterClient) // it has to be alredy the master
                    {
                        double timeOfTimer = 3; //in seconds
                        if (PlayerPrefs.GetString(nameOfTable) == "") // make a string of table for each num of players. it check if the one that is equal to maxplayersperroom is not empty.
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
        // change the text in playersJoinedTextBox
        PlayersJoinedTextBox.GetComponent<Text>().text = "Players that alredy joined: " + PhotonNetwork.CurrentRoom.PlayerCount;
        // add custom properties to the player
        newPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable() { { "ColorName", colorName[playersPositionIndex] }, { "Index", playersPositionIndex}, { "Point", 0 }, {"Master", newPlayer.IsMasterClient} });
        Debug.Log("Photon_Menu - Giving index:" + playersPositionIndex + " to " + newPlayer.NickName);
        this.photonView.RPC("ChangePlayerVariables", newPlayer, playersPositionIndex); 
        playersPositionIndex++;
        
        if (PhotonNetwork.CurrentRoom.PlayerCount == MaxPlayersPerRoom)
        {
            PhotonNetwork.CurrentRoom.IsOpen = false;

            waitingStatusText.text = "Opponent found";
            Debug.Log("Mach is ready to begin");

            if (PhotonNetwork.IsMasterClient)
            {
                double timeOfTimer = 3; //in seconds
                if (PlayerPrefs.GetString("Table") == "")
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
                    playersPosition = GeneratePlayerPositions(new int[MaxPlayersPerRoom, 2]);
                }
                // go over all players
                foreach (Player player in PhotonNetwork.PlayerList)
                {
                    // send the player position and rotation
                    // get the index in player custom properties
                    int index;
                    if (player.CustomProperties["Index"] == null){
                        Debug.Log("The index is null");
                        index = playersPositionIndex - 1;
                    }
                    else{   
                        index = (int)player.CustomProperties["Index"];
                    }
                    this.photonView.RPC("ChangePlayerPosition", player, playersPosition[index, 0], playersPosition[index, 1], rotation[index % 4]);
                }

                StartCoroutine(StartTimer(timeToStart));
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
        PlayerPrefs.SetInt("PlayerIndex", index);
	    if (index <= 4){
	        PlayerPrefs.SetString("ColorName", colorName[index]);
	    }
	    else {
	        PlayerPrefs.SetString("ColorName", "W");
	    }
    }

    [PunRPC]
    public void ChangePlayerPosition(int placeX, int placeY, int rotation){
        PlayerPrefs.SetInt("PositionX", placeX);
        PlayerPrefs.SetInt("PositionY", placeY);
        PlayerPrefs.SetInt("RotationY", rotation);
        PlayerPrefs.SetInt("Trophies", 2 * (int)PhotonNetwork.CurrentRoom.CustomProperties["MaxPlayersPerRoom"] + 1);
    }
}