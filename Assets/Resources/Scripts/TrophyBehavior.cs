using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrophyBehavior : MonoBehaviour
{
    void OnTriggerEnter(Collider col)
    {
        if (col.gameObject.CompareTag("Player"))
        {
            Destroy(gameObject);
        }
    }
}
