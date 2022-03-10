using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class TrophyBehavior : MonoBehaviourPun
{
    void OnTriggerEnter(Collider col)
    {
        if (col.gameObject.CompareTag("Player"))
        {
	        // PhotonView photonView = col.GetComponent<PhotonView>();
            // punrpc target master to add a point to the player that enter the trigger
            // Debug.Log("Player enter the trigger. PhotonNetwork.LocalPlayer.ActorNumber: " + PhotonNetwork.LocalPlayer.GetPlayerNumber());
            Debug.Log("Player enter the trigger. Index of the player is: " + col.gameObject.GetComponent<FIMSpace.Basics.FBasic_FheelekController>().playerIndex);
            this.photonView.RPC("AddPointTo", RpcTarget.MasterClient, col.gameObject.GetComponent<FIMSpace.Basics.FBasic_FheelekController>().playerIndex);
            // this.photonView.RPC("AddPointTo", RpcTarget.MasterClient, col.gameObject.GetComponent<spawning>().playerIndex);
            Destroy(gameObject);
        }
    }
}
