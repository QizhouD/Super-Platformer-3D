using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NucliarWaste : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            other.GetComponent<Collector>().points++; //adding 1 point per nuclear waste
            Destroy(gameObject); // destroying the pickup
        }
    }
}
