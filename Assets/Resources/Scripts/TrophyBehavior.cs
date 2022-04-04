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
            Debug.Log("Player enter the trigger. Index of the player is: " + col.gameObject.GetComponent<FIMSpace.Basics.FBasic_FheelekController>().playerIndex);
            Destroy(gameObject);
        }
    }
}
