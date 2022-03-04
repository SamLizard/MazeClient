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

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log("No clients are waiting  for an opponent, creating a new room");

        PhotonNetwork.CreateRoom(null, new RoomOptions{ MaxPlayers = MaxPlayersPerRoom }); // enter as master
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
            Debug.Log("I am the master");
            StartCoroutine(InstantiateData());
        }
    }

    public DateTime Truncate(this DateTime dateTime, TimeSpan timeSpan)
    {
        if (timeSpan == TimeSpan.Zero) return dateTime; // Or could throw an ArgumentException
        if (dateTime == DateTime.MinValue || dateTime == DateTime.MaxValue) return dateTime; // do not modify "guard" values
        return dateTime.AddTicks(-(dateTime.Ticks % timeSpan.Ticks));
    }

    public DateTime CalculateStartingTime(double timeOfTimer)
    {
        DateTime actualTime = DateTime.Now;
        actualTime.AddSeconds(timeOfTimer);
        return Truncate(actualTime, TimeSpan.FromSeconds(1));
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
                this.photonView.RPC("StartTimer", RpcTarget.Others, timeToStart);
                // pun rpc evryone
            }

            // start a timer of 5 secondes, and then load the game
            // if master, send the time you want the game to start. the client need a punRPC methode that just wait until the time tha is written. it also change the panel timer, and start it
            PhotonNetwork.LoadLevel("Game");
        }
    }

    [PunRPC]
    public void StartTimer(DateTime timeToStart)
    {
        Debug.Log("TimeToStart: " + timeToStart);
        // hide the panels that are active, set active the timer panel, while localtime < timeToStart, do nothing.
        // after the while:
        PhotonNetwork.LoadLevel("Game");
    }
}
