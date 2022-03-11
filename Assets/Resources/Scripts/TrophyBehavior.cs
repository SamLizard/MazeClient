using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class TrophyBehavior : MonoBehaviourPun
{
    bool alreadyIn = false;
    void OnTriggerEnter(Collider col)
    {

        if (col.gameObject.CompareTag("Player") && !alreadyIn)
        {
            alreadyIn = true;
            GameObject.FindWithTag("Spawning").GetComponent<spawning>().AddPointTo(col.gameObject.GetComponent<FIMSpace.Basics.FBasic_FheelekController>().playerIndex);
            // this.photonView.Owner.GetComponent<spawning>().AddPoint(col.gameObject.GetComponent<FIMSpace.Basics.FBasic_FheelekController>().playerIndex);
	        // PhotonView photonView = col.GetComponent<PhotonView>();
            // punrpc target master to add a point to the player that enter the trigger
            // Debug.Log("Player enter the trigger. PhotonNetwork.LocalPlayer.ActorNumber: " + PhotonNetwork.LocalPlayer.GetPlayerNumber());
            Debug.Log("Player enter the trigger. Index of the player is: " + col.gameObject.GetComponent<FIMSpace.Basics.FBasic_FheelekController>().playerIndex);
            // target master to add a point to the player that enter the trigger
            // photonView.RPC("AddPointTo", RpcTarget.All, col.gameObject.GetComponent<FIMSpace.Basics.FBasic_FheelekController>().playerIndex);
            // this.photonView.RPC("AddPointTo", RpcTarget., col.gameObject.GetComponent<FIMSpace.Basics.FBasic_FheelekController>().playerIndex); // RpcTarget.MasterClient
            // this.photonView.RPC("AddPointTo", RpcTarget.MasterClient, col.gameObject.GetComponent<spawning>().playerIndex);
            Destroy(gameObject);
        }
    }
}
