using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoubleJump : MonoBehaviour
{

    
  

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            other.GetComponent<PlayerControllerFPV>().doubleJump = true;
            Destroy(gameObject); // destroying the pickup
        }
    }
}
