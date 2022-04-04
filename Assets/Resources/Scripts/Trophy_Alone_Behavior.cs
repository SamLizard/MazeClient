using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trophy_Alone_Behavior : MonoBehaviour
{
    bool alreadyIn = false;
    void OnTriggerEnter(Collider col)
    {
        if (col.gameObject.CompareTag("Player") && !alreadyIn)
        {
            alreadyIn = true;
            GameObject.FindWithTag("Client").GetComponent<ClientScript>().TrophyTaken();
            Destroy(gameObject);
        }
    }
}
