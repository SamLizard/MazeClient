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
    [SerializeField] GameObject roomName;
    [SerializeField] private GameObject masterPanel = null;
    [SerializeField] private GameObject Canvas = null;
    public GameObject PlayersJoinedTextBox = null;
    private bool isConnecting = false;
    private const string GameVersion = "0.2";
    private const int defaultMaxPlayers = 2;
    private int[,] playersPosition;
    private readonly double minTime = 4;
    private bool isInTimer = false;
    [SerializeField] private GameObject lockPrefab;
    private Dictionary<string, Color> colorList = new Dictionary<string, Color>() { { "G", Color.green }, { "R", Color.red }, { "P", new Color(1, 0, 1, 1) }, { "Y", Color.yellow }, { "B", Color.black}, { "W", Color.white} };
    private readonly Tuple<int, int>[] positions = new Tuple<int, int>[4] {
        new Tuple<int, int>(1, 1),
        new Tuple<int, int>(-1, -1),
        new Tuple<int, int>(-1, 1),
        new Tuple<int, int>(1, -1)
    };
    private int[] rotation = new int[4] {45, 225, 315, 135};
    private string[] colorName = new string[5] {"G", "Y", "R", "P", "W"};
    private readonly int square_size = 10;
    public int size;

    private void Awake() => PhotonNetwork.AutomaticallySyncScene = true;

    void Update()
    {
        if (Canvas.transform.Find("Panel_Winning").gameObject.activeInHierarchy && Cursor.visible == false)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            Debug.Log("Cursor was not visible.");
        }
    }

    private void Start()
    {
        StaticData.CleanTable();
        Canvas.transform.Find("Panel_FindOpponent").transform.Find("InputFieldRoomName").GetComponent<InputField>().text = StaticData.myRoomName;
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
            for (int i = 0; i < list.Count; i++)
            {
                addTextBox(list[i].Key.Substring(2), list[i].Value.ToString(), i, false, colorList[list[i].Key[0].ToString()], "Finnish_Background", Canvas.transform.Find("Panel_Winning").gameObject, true);
            }
            if ((int)PhotonNetwork.LocalPlayer.CustomProperties["Point"] == (int)list[0].Value) 
            {
                Canvas.transform.Find("Panel_Winning").gameObject.transform.Find("Status_Winning_Text").gameObject.GetComponent<Text>().text = "You won";
                Canvas.transform.Find("Panel_Winning").gameObject.transform.Find("Status_Winning_Text").gameObject.GetComponent<Text>().color = Color.green;
            }
            else
            {
                Canvas.transform.Find("Panel_Winning").gameObject.transform.Find("Status_Winning_Text").gameObject.GetComponent<Text>().text = "Game Over";
                Canvas.transform.Find("Panel_Winning").gameObject.transform.Find("Status_Winning_Text").gameObject.GetComponent<Text>().color = Color.red;
            }
            StaticData.playerPoints.Clear();
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
            PhotonNetwork.PhotonServerSettings.AppSettings.AppVersion = GameVersion;
            PhotonNetwork.ConnectUsingSettings();
        }
    }

    private void SetRoomNameText(){
        roomName.GetComponent<TextMeshProUGUI>().text = "Room Name: " + PhotonNetwork.CurrentRoom.Name;
        roomName.SetActive(true);
    }

    public void CreateRoomAlone()
    {
        SceneManager.LoadScene("GameAlone");
    }

    public override void OnConnectedToMaster()
    {
        if (isConnecting)
        {
            PhotonNetwork.JoinRandomRoom();
        }
        else if (StaticData.creatingMyRoom && findOpponentPanel.activeInHierarchy){
            PhotonNetwork.JoinOrCreateRoom(StaticData.myRoomName, new RoomOptions { MaxPlayers = (byte)defaultMaxPlayers }, TypedLobby.Default);
            StaticData.creatingMyRoom = false;
        }
    }

    public override void OnDisconnected(Photon.Realtime.DisconnectCause cause)
    {
        waitingStatusPanel.SetActive(false);
        roomName.SetActive(false);
        findOpponentPanel.SetActive(true);
    }

    IEnumerator Alert(string textToDisplay, int secondsToWait){
        Canvas.transform.Find("TextAlert").gameObject.SetActive(true);
        Canvas.transform.Find("TextAlert").gameObject.GetComponent<TextMeshProUGUI>().text = textToDisplay;
        yield return new WaitForSeconds(secondsToWait);
        Canvas.transform.Find("TextAlert").gameObject.SetActive(false);
    }

    public override void OnPlayerLeftRoom(Player otherPlayer){
        if (isInTimer){
            isInTimer = false;
            StartCoroutine(Alert("Player " + otherPlayer.NickName + " left the room.", 2));
        }
        if (PhotonNetwork.IsMasterClient && otherPlayer.CustomProperties["Master"] != null && (bool)otherPlayer.CustomProperties["Master"] && !Canvas.transform.Find("Panel_Winning").gameObject.activeInHierarchy){
            findOpponentPanel.SetActive(false);
            ActivateMasterMode();
            ActivateLockedLock(Canvas.transform.Find("masterPanel").transform.Find("RoomInformations").transform.Find("Lock_Parent").transform.Find("Lock").transform, !PhotonNetwork.CurrentRoom.IsVisible);
        }
        else if(PhotonNetwork.IsMasterClient && Canvas.transform.Find("masterPanel").gameObject.activeInHierarchy){
            PlayersJoinedTextBox.GetComponent<Text>().text = "Players that already joined: " + PhotonNetwork.CurrentRoom.PlayerCount.ToString();
        }
        else if (!PhotonNetwork.IsMasterClient){ 
            waitingStatusText.text = "Waiting for opponents.\n" + PhotonNetwork.CurrentRoom.PlayerCount + " / " + PhotonNetwork.CurrentRoom.MaxPlayers + " players.";
        }
    }

    public void ActivateMasterMode(){
        masterPanel.SetActive(true);
        waitingStatusPanel.SetActive(false);
        findOpponentPanel.SetActive(false);
        PhotonNetwork.LocalPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable() { {"Master", true} });
        StartCoroutine(GenerateMaze());
        playersPosition = new int[PhotonNetwork.CurrentRoom.MaxPlayers, 2];
        playersPosition = GeneratePlayerPositions(playersPosition);
        CreateMasterBoxes(PhotonNetwork.CurrentRoom.PlayerCount);
    }

    public int calculateMazeSize(){
        return 12 * PhotonNetwork.CurrentRoom.MaxPlayers + 1;
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log("No clients are waiting for an opponent, creating a new room.");
        PhotonNetwork.CreateRoom(null, new RoomOptions { MaxPlayers = (byte)defaultMaxPlayers });
    }

    public void CreateNewRoom(){
        waitingStatusPanel.SetActive(true);
        waitingStatusText.text = "Creating room...";
        if (!PhotonNetwork.IsConnected)
        {
            PhotonNetwork.PhotonServerSettings.AppSettings.AppVersion = GameVersion;
            PhotonNetwork.ConnectUsingSettings();
        }
        else{
            PhotonNetwork.JoinOrCreateRoom(StaticData.myRoomName, new RoomOptions { MaxPlayers = (byte)defaultMaxPlayers }, TypedLobby.Default);
        }
        StaticData.creatingMyRoom = true;
    }

    public void ChangeLock(){
        Transform Lock;
        if(masterPanel.activeInHierarchy){
            Lock = Canvas.transform.Find("masterPanel").transform.Find("RoomInformations").transform.Find("Lock_Parent").transform.Find("Lock").transform;
            PhotonNetwork.CurrentRoom.IsVisible = !PhotonNetwork.CurrentRoom.IsVisible;
        }
        else{
            Lock = Canvas.transform.Find("Panel_FindOpponent").transform.Find("Button_CreateNewRoom").transform.Find("Lock").transform;
        }
        if (Lock.Find("Locked_Lock").gameObject.activeInHierarchy){
            ActivateLockedLock(Lock, false);
        }
        else{
            ActivateLockedLock(Lock, true);
        }
    }

    public void ActivateLockedLock(Transform Lock, bool activateIt){
        Lock.Find("Locked_Lock").gameObject.SetActive(activateIt);
        Lock.Find("Unlocked_Lock").gameObject.SetActive(!activateIt);
    }

    IEnumerator GenerateMaze()
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            yield break;
        }
        size = calculateMazeSize();
        StaticData.mazeSize = size;
        int[,] table = Static_Methods.CreateMazeList(size);
        string tableString = Static_Methods.TableToString(table);
        StaticData.PutStringInTable(tableString, PhotonNetwork.CurrentRoom.MaxPlayers);
    }

    private int[,] GeneratePlayerPositions(int[,] playersPositionToActualize){
        size = calculateMazeSize();
        if (!PhotonNetwork.IsMasterClient){
            return playersPositionToActualize;
        }
        int startingPosition = ((size - (size % 2)) / 2) * -square_size;
        int placeX = startingPosition;
        int placeY = startingPosition;
        for (int i = 0; i < PhotonNetwork.CurrentRoom.MaxPlayers; i++)
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
        Debug.Log("Client successfully joined a room with name: " + PhotonNetwork.CurrentRoom.Name);
        SetRoomNameText();
        int playerCount = PhotonNetwork.CurrentRoom.PlayerCount;

        if (playerCount == PhotonNetwork.CurrentRoom.MaxPlayers)
        {
            waitingStatusText.text = "Opponent found";
            Debug.Log("Mach is ready to begin");        }
        if (!PhotonNetwork.IsMasterClient)
        {
            waitingStatusText.text = "Waiting for Players\n" + playerCount + " / " + PhotonNetwork.CurrentRoom.MaxPlayers;
        }
        else
        {
            if (Canvas.transform.Find("Panel_FindOpponent").transform.Find("Button_CreateNewRoom").transform.Find("Lock").transform.Find("Locked_Lock").gameObject.activeInHierarchy){
                PhotonNetwork.CurrentRoom.IsVisible = false;
            }
            ActivateMasterMode();
        }
    }

    private void CreateLockInMaster(int index){
        GameObject Lock_Parent = Instantiate(new GameObject("Lock_Parent"), masterPanel.transform.Find("RoomInformations").transform);
        Lock_Parent.AddComponent<RectTransform>();
        PlaceBox(Lock_Parent, index, "Lock_Parent");
        GameObject LockText = addTextBox("Is room visible", "", 0, false, new Color(0, 0, 0.5f), "Lock_Parent", masterPanel.transform.Find("RoomInformations").gameObject, false);
        LockText.transform.SetParent(Lock_Parent.transform);
        SetObjectPosition(LockText, 0, 0.65f);
        GameObject Lock = Instantiate(lockPrefab, Lock_Parent.transform);
        Lock.name = "Lock";
        Lock.transform.localPosition = new Vector3(0, 0, 0);
        SetObjectPosition(Lock, 0.66f, 1);
        Lock.GetComponent<Button>().onClick.AddListener(delegate { ChangeLock(); });
        if (PhotonNetwork.CurrentRoom.IsVisible){
            ActivateLockedLock(Lock.transform, false);
            return;
        }
        ActivateLockedLock(Lock.transform, true);
    }

    private void SetObjectPosition(GameObject Object, float minX, float maxX){
        Object.GetComponent<RectTransform>().anchorMin = new Vector2(minX, 0);
        Object.GetComponent<RectTransform>().anchorMax = new Vector2(maxX, 1);
        Object.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 0);
        Object.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);
    }

    public void CreateMasterBoxes(int playerCount){
        addTextBox("Max Players", PhotonNetwork.CurrentRoom.MaxPlayers.ToString(), 0, true, new Color(0, 0, 0.5f), "RoomInformations", masterPanel, false);
        addTextBox("Number of trophies", Static_Methods.CalculateTrophies((int)PhotonNetwork.CurrentRoom.MaxPlayers).ToString(), 1, false, new Color(0, 0, 0.5f), "RoomInformations", masterPanel, false);
        PlayersJoinedTextBox = addTextBox("Players that alredy joined", playerCount.ToString(), 2, false, new Color(0, 0, 0.5f), "RoomInformations", masterPanel, false);
        CreateLockInMaster(3);
    }

    public GameObject addTextBox(string textToDisplay, string variableToShow, int index, bool allowInput, Color color, string parent, GameObject panel, bool enableOutline)
    {
        enableOutline = enableOutline && color != Color.black;
        if (allowInput){
            return AddTextBoxAndInput(textToDisplay, variableToShow, index, color, parent, panel, enableOutline);
        }
        GameObject textBox = Instantiate(textBoxPrefab, Vector3.zero, Quaternion.identity);
        textBox.transform.SetParent(panel.transform.Find(parent));
        
        PlaceBox(textBox, index, textToDisplay);
        TextBoxParameters(textBox, textToDisplay, variableToShow, color, enableOutline);
        return textBox;
    }

    public GameObject AddTextBoxAndInput(string textToDisplay, string variableToShow, int index, Color color, string parentStr, GameObject panel, bool enableOutline){
        GameObject parent = Instantiate(TextBoxAndField, Vector3.zero, Quaternion.identity);
        parent.transform.SetParent(panel.transform.Find(parentStr));
        
        GameObject textBox = parent.transform.Find("TextBox").gameObject;
        GameObject inputField = parent.transform.Find("InputField").gameObject;
        
        PlaceBox(parent, index, textToDisplay);
        TextBoxParameters(textBox, textToDisplay, variableToShow, color, enableOutline);
        
        inputField.transform.Find("Text").GetComponent<Text>().color = color;
        inputField.GetComponent<InputField>().text = variableToShow;
        // activate the inputfield on value changed, chose the function oninputChanged that is in the script Photon_Menu in the Canvas_Menu in scene
        inputField.GetComponent<InputField>().onValueChanged.AddListener(delegate { onInputChanged(); });
        return parent;
    }

    public void PlaceBox(GameObject Box, int index, string textToDisplay){ // and rename the box
        Box.GetComponent<RectTransform>().anchorMin = new Vector2(0.2f, 0.67f - (0.13f * index));
        Box.GetComponent<RectTransform>().anchorMax = new Vector2(0.8f, 0.75f - (0.13f * index));
        Box.GetComponent<RectTransform>().localPosition = new Vector3(0, 0, 0);
        Box.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 0f);    
        Box.GetComponent<RectTransform>().anchoredPosition3D = Vector3.zero;
        Box.name = textToDisplay;
    }

    public void TextBoxParameters(GameObject Box, string textToDisplay, string variableToShow, Color color, bool enableOutline){
        Box.GetComponent<Text>().text = textToDisplay + ": " + variableToShow;
        Box.GetComponent<Text>().color = color;
        Box.GetComponent<Outline>().enabled = enableOutline;
        Box.GetComponent<Text>().resizeTextForBestFit = true;
        Box.GetComponent<Text>().resizeTextMinSize = 5;
        Box.GetComponent<Text>().resizeTextMaxSize = 200;
    }

    public void onInputChanged(){
        GameObject inputFieldMaxPlayers = masterPanel.transform.Find("RoomInformations").Find("Max Players").Find("InputField").GetComponent<InputField>().gameObject;
        CheckChangingMaxPlayers(inputFieldMaxPlayers);
    }

    public void CheckChangingMaxPlayers(GameObject inputFieldMaxPlayers){
        if (int.TryParse(inputFieldMaxPlayers.GetComponent<InputField>().text, out int playersInInput)){
            int playerCount = PhotonNetwork.CurrentRoom.PlayerCount;
            if (playersInInput >= playerCount && playersInInput <= 5 && playersInInput >= 2) {
                PhotonNetwork.CurrentRoom.MaxPlayers = (byte)playersInInput;
                masterPanel.transform.Find("RoomInformations").Find("Number of trophies").GetComponent<Text>().text = "Number of trophies: " + Static_Methods.CalculateTrophies((int)PhotonNetwork.CurrentRoom.MaxPlayers).ToString();
                masterPanel.transform.Find("RoomInformations").Find("Max Players").Find("TextBox").GetComponent<Text>().text = "Max Players: " + playersInInput;
                this.photonView.RPC("UpdateMaxPlayers", RpcTarget.Others);
                playersPosition = new int[playersInInput, 2];
                playersPosition = GeneratePlayerPositions(playersPosition);
                
                if (StaticData.IsEmptyAt(playersInInput)) {
                    StartCoroutine(GenerateMaze());
                }
                else
                {
                    if (!StaticData.IsEmptyAt(playersInInput)) 
                    {
                        size = calculateMazeSize();
                        Static_Methods.size = size;
                        StaticData.mazeSize = size;
                    }
                }
                if (playersInInput == playerCount){
                    PhotonNetwork.CurrentRoom.IsOpen = false;
                    waitingStatusText.text = "Opponent found";
                    double timeOfTimer = minTime;
                    if (StaticData.IsEmptyAt(playersInInput))
                    {
                        timeOfTimer += 2;
                    }
                    DateTime timeToStart = CalculateStartingTime(timeOfTimer);
                    this.photonView.RPC("StartClientTimer", RpcTarget.Others, timeToStart.ToString("dd/MM/yyyy HH:mm:ss"));
                    StartCoroutine(StartTimer(timeToStart));
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
        int timeLeft = fromTime.Second - DateTime.Now.Second;
        return timeLeft < 0 ? timeLeft + 60 : timeLeft;
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        if (!PhotonNetwork.IsMasterClient){
            waitingStatusText.text = "Waiting for Players\n" + PhotonNetwork.CurrentRoom.PlayerCount + " / " + PhotonNetwork.CurrentRoom.MaxPlayers;
            return;
        }
        PlayersJoinedTextBox.GetComponent<Text>().text = "Players that alredy joined: " + PhotonNetwork.CurrentRoom.PlayerCount;
        if (PhotonNetwork.CurrentRoom.PlayerCount == PhotonNetwork.CurrentRoom.MaxPlayers)
        {
            PhotonNetwork.CurrentRoom.IsOpen = false;
            waitingStatusText.text = "Opponent found";
            double timeOfTimer = minTime;
            if (StaticData.IsEmptyAt(PhotonNetwork.CurrentRoom.MaxPlayers))
            {
                timeOfTimer += 2;
            }
            DateTime timeToStart = CalculateStartingTime(timeOfTimer);
            this.photonView.RPC("StartClientTimer", RpcTarget.Others, timeToStart.ToString("dd/MM/yyyy HH:mm:ss"));
            if (PhotonNetwork.CurrentRoom.MaxPlayers != defaultMaxPlayers)
            {
                playersPosition = new int[PhotonNetwork.CurrentRoom.MaxPlayers, 2];
                playersPosition = GeneratePlayerPositions(playersPosition);
            }
            StartCoroutine(StartTimer(timeToStart));
        }
    }

    public void sendRPCtoClients(){
        int i = 0;
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            player.SetCustomProperties(new ExitGames.Client.Photon.Hashtable() { { "ColorName", colorName[i] }, { "Index", i }, { "Point", 0 }, {"Master", player.IsMasterClient} });
            this.photonView.RPC("ChangePlayerPosition", player, playersPosition[i, 0], playersPosition[i, 1], rotation[i % 4], Static_Methods.CalculateTrophies((int)PhotonNetwork.CurrentRoom.MaxPlayers));
            i++;
        }
    }

    IEnumerator StartTimer(DateTime timeToStart)
    {
        Debug.Log("Time to start: " + timeToStart + ". So, time left: " + timeLeft(timeToStart));
        isInTimer = true;
        int restantSeconds = timeLeft(timeToStart);
        int secondsToWait = restantSeconds;
        waitingStatusText.text = "Game Start in " + restantSeconds + " seconds";
        if (PhotonNetwork.IsMasterClient){
            waitingStatusPanel.SetActive(true);
            waitingStatusPanel.GetComponent<Image>().color = new Color(1, 1, 1, 0);
            waitingStatusText.GetComponent<RectTransform>().anchoredPosition = new Vector2(200, 0);
        }
        bool rpcSent = false;
        for (int i = 0; i < secondsToWait; i++)
        {
            yield return new WaitUntil(() => (timeLeft(timeToStart) < restantSeconds));
            restantSeconds = timeLeft(timeToStart);
            waitingStatusText.text = "Game Start in " + restantSeconds + " seconds";
            if (!isInTimer){
                if (PhotonNetwork.IsMasterClient){
                    waitingStatusText.text = "";
                }
                else{
                    waitingStatusText.text = "Waiting for Players\n" + PhotonNetwork.CurrentRoom.PlayerCount + " / " + PhotonNetwork.CurrentRoom.MaxPlayers;
                }
                yield break;
            }
            if (DateTime.Now > timeToStart)
            {
                waitingStatusText.text = "Game Start in 0 seconds";
                break;
            }
            if ((restantSeconds <= (minTime - 3) || (restantSeconds <= 2)) && PhotonNetwork.IsMasterClient && !rpcSent){
                rpcSent = true;
                sendRPCtoClients();
            }
        }
        yield return new WaitUntil(() => (PhotonNetwork.PlayerList[0].CustomProperties["Index"] != null));
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
    public void ChangePlayerPosition(int placeX, int placeY, int rotation, int trophies){
        StaticData.position = new Vector3(placeX, 0.5f, placeY);
        StaticData.rotation = rotation;
        StaticData.trophiesInGame = trophies;
    }

    [PunRPC]
    public void UpdateMaxPlayers(){
        if (!PhotonNetwork.IsMasterClient){
            waitingStatusText.text = "Waiting for Players\n" + PhotonNetwork.CurrentRoom.PlayerCount + " / " + PhotonNetwork.CurrentRoom.MaxPlayers.ToString();
        }
    }
}