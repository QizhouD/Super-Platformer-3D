using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VanishingPlatform : MonoBehaviour
{
    [SerializeField] string playerTag = "Player"; // 
    [SerializeField] float disappearTime = 3;
    [SerializeField] bool canReset;
    [SerializeField] float resetTime;

    Animator vanishAnim;

    
    private void Start()
    {
        vanishAnim = GetComponent<Animator>();
        vanishAnim.SetFloat("DisappearTime", 1 / disappearTime);
    }


    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.tag == playerTag)
        {
            vanishAnim.SetBool("Trigger", true);
        }
    }

    IEnumerator Reset()
    {
        yield return new WaitForSeconds(resetTime);
        vanishAnim.SetBool("Trigger", false);
    }


    public void TriggerReset()
    {
        if (canReset )
        {
            StartCoroutine(Reset());
        }
    }

   


}
