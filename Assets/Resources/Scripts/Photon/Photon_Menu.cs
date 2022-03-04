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
        return 12 * MaxPlayersPerRoom;
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log("No clients are waiting  for an opponent, creating a new room");

        PhotonNetwork.CreateRoom(null, new RoomOptions { MaxPlayers = MaxPlayersPerRoom }); // enter as master
                                                                                            // start coroutine that build the maze, and put it in string in the playerprefs
    }

    IEnumerator InstantiateData()
    {
        Debug.Log("Coroutine \"InstantiateData\" started.");
        PlayerPrefs.SetInt("Size", 3);
        PlayerPrefs.SetString("Table", "101111001");
        // PhotonNetwork.Instantiate("roomData", Vector3.zero, Quaternion.identity);
        yield return new WaitForSeconds(0);
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
            StartCoroutine(InstantiateData());
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
        DateTime actualTime = DateTime.Now;
        DateTime timeToStart = actualTime.AddSeconds(timeOfTimer);
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
}
