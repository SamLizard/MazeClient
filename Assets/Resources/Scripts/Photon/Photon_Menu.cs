using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;
using Photon.Realtime;
using System;

public class Photon_Menu : MonoBehaviourPunCallbacks
{
    [SerializeField] private GameObject findOpponentPanel = null;
    [SerializeField] private GameObject waitingStatusPanel = null;
    [SerializeField] private TextMeshProUGUI waitingStatusText = null;

    private bool isConnecting = false;

    private const string GameVersion = "0.1";
    private const int MaxPlayersPerRoom = 2;

    private int[,] playersPosition;
    private int playersPositionIndex = 0;

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

    private int[] rotation = new int[4] {45, 135, 225, 315};

    private readonly int[][] RandomDirections = new int[24][] {
        new int[4] {0, 1, 2, 3 }, new int[4] {0, 1, 3, 2 }, new int[4] {0, 2, 1, 3 }, new int[4] {0, 2, 3, 1 }, new int[4] {0, 3, 2, 1 }, new int[4] {0, 3, 1, 2 },
        new int[4] {1, 0, 2, 3 }, new int[4] {1, 0, 3, 2 }, new int[4] {1, 2, 0, 3 }, new int[4] {1, 2, 3, 0 }, new int[4] {1, 3, 2, 0 }, new int[4] {1, 3, 0, 2 },
        new int[4] {2, 0, 1, 3 }, new int[4] {2, 0, 3, 1 }, new int[4] {2, 1, 3, 0 }, new int[4] {2, 1, 0, 3 }, new int[4] {2, 3, 0, 1 }, new int[4] {2, 3, 1, 0 },
        new int[4] {3, 0, 2, 1 }, new int[4] {3, 0, 1, 2 }, new int[4] {3, 1, 2, 0 }, new int[4] {3, 1, 0, 2 }, new int[4] {3, 2, 1, 0 }, new int[4] {3, 2, 0, 1 },
    };

    private readonly int square_size = 10;
    public int size;

    private void Awake() => PhotonNetwork.AutomaticallySyncScene = true;

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

        Debug.Log($"Disctonnected due to: {cause}");
    }

    public int calculateMazeSize()
    {
        return 12 * MaxPlayersPerRoom + 1;
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log("No clients are waiting  for an opponent, creating a new room");

        PhotonNetwork.CreateRoom(null, new RoomOptions { MaxPlayers = MaxPlayersPerRoom }); // enter as master
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
        PlayerPrefs.SetString("Table", tableString);
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
            waitingStatusText.text = "Waiting for opponent";
            Debug.Log("Client is waiting for an opponent");
        }
        else
        {
            waitingStatusText.text = "Opponent found";
            Debug.Log("Mach is ready to begin");
        }
        if (PhotonNetwork.IsMasterClient)
        {
            //System.Globalization.CultureInfo culture = new System.Globalization.CultureInfo("fr-FR");
            //Debug.Log(Convert.ToDateTime(StringDateTime(DateTime.Now), culture)); 
            Debug.Log("I am the master");
            PlayerPrefs.SetInt("MaxPlayersPerRoom", MaxPlayersPerRoom);
            Debug.Log("Starting GenerateMaze Coroutine.");
            StartCoroutine(GenerateMaze());

            playersPosition = new int[MaxPlayersPerRoom, 2];
            playersPosition = GeneratePlayerPositions(playersPosition);
            PlayerPrefs.SetInt("PositionX", playersPosition[playersPositionIndex, 0]);
            PlayerPrefs.SetInt("PositionY", playersPosition[playersPositionIndex, 1]);
            PlayerPrefs.SetInt("RotationY", rotation[playersPositionIndex]);
            playersPositionIndex = 1;
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

                // start a timer of 5 secondes, and then load the game
                // if master, send the time you want the game to start. the client need a punRPC methode that just wait until the time tha is written. it also change the panel timer, and start it
                //int restantSeconds = timeLeft(timeToStart);
                //waitingStatusText.text = "Game Start in " + restantSeconds + " seconds";
                // Debug.Log("WaitingStatusText is: " + waitingStatusText.text);
                //while (true)
                //{
                //    if (timeLeft(timeToStart) < restantSeconds)
                //    {
                //        restantSeconds = timeLeft(timeToStart);
                //        waitingStatusText.text = "Game Start in " + restantSeconds + " seconds";
                //        // Debug.Log("WaitingStatusText is: " + waitingStatusText.text);
                //    }
                //    if (DateTime.Now > timeToStart)
                //    {
                //        Debug.Log("Master - Actual time: " + DateTime.Now.ToString() + "\nTimeToStart: " + timeToStart.ToString());
                //        waitingStatusText.text = "Game Start in 0 seconds";
                //        break;
                //    }
                //}
                //PhotonNetwork.LoadLevel("Game");
                StartCoroutine(StartTimer(timeToStart));
            }
        }
        this.photonView.RPC("ChangePlayerPosition", newPlayer, playersPosition[playersPositionIndex, 0], playersPosition[playersPositionIndex, 1], rotation[playersPositionIndex % 4]);
        playersPositionIndex++;
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

        //if (!PhotonNetwork.IsMasterClient)
        //{
        //    Debug.Log("[PunRPC] Actual time: " + DateTime.Now.ToString() + "\nTimeToStart: " + timeString);
        //    PlayerPrefs.SetString("TimerTime", timeString);
        //    GameObject timer = Instantiate(timerPrefab, Vector3.zero, Quaternion.identity);
        //}

        //Debug.Log("TimeToStart: " + timeToStart.ToString());
        //int restantSeconds = timeLeft(timeToStart);
        //waitingStatusText.text = "Game Start in " + restantSeconds + " seconds";
        //while (true)
        //{
        //    if (timeLeft(timeToStart) < restantSeconds)
        //    {
        //        restantSeconds = timeLeft(timeToStart);
        //        waitingStatusText.text = "Game Start in " + restantSeconds + " seconds";
        //        // Debug.Log("WaitingStatusText is: " + waitingStatusText.text);
        //    }
        //    if (DateTime.Now > timeToStart)
        //    {
        //        Debug.Log("Actual time: " + DateTime.Now.ToString() + "\nTimeToStart: " + timeToStart.ToString());
        //        break;
        //    }
        //}
        //Debug.Log("For is Done");
        //// hide the panels that are active, set active the timer panel, while localtime < timeToStart, do nothing.
        //// after the while:
        //PhotonNetwork.LoadLevel("Game");
    }

    [PunRPC]
    public void ChangePlayerPosition(int placeX, int placeY, int rotation)
    {
        PlayerPrefs.SetInt("PositionX", placeX);
        PlayerPrefs.SetInt("PositionY", placeY);
        PlayerPrefs.SetInt("RotationY", rotation);
    }
}
