using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VerticalPlatform : MonoBehaviour
{
    //To check the positions of where the platform will move
    public GameObject verticalPlatform;
    public GameObject nextPlatform;
    private Vector3 originalPos;    
    private Vector3 targetPos;

    //To make the platform wait
    private float time;
    public float timeDelay;
    private bool isMovingUp;
    private bool isMovingDown;

    //To set how fast it will move
    public float speed;


    // Start is called before the first frame update
    void Start()
    {
        originalPos = verticalPlatform.transform.position;
        targetPos = new Vector3(originalPos.x, nextPlatform.transform.position.y, originalPos.z);

        time = 0f;
        isMovingUp = false;
        isMovingDown = false;
    }
  

    // Update is called once per frame
    void Update()
    {
        SetMove();
    }

    //If the platform is at the target or original position it waits
    private void SetMove()
    {
        //if the platform is at the original position it waits
        if (transform.position.y == originalPos.y)
        {            
            isMovingDown = false;
            Countdown();
            if (time >= timeDelay)
            {

                isMovingUp = true;
                time = 0f;
            }
        }
        //if the platform is at the target position it waits
        else if (transform.position.y == targetPos.y)
        {
            isMovingUp = false;
            Countdown();
            if (time >= timeDelay)
            {

                isMovingDown = true;
                time = 0f;
             }

        }


        if (isMovingUp)
        {
            MoveUp();
        }
        else if (isMovingDown)
        {
            MoveDown();
        }
    }

    void MoveUp()
    {
        transform.position = Vector3.MoveTowards(transform.position, targetPos, speed * Time.deltaTime);
    }

    void MoveDown()
    {             
        transform.position = Vector3.MoveTowards(transform.position, originalPos, speed * Time.deltaTime);            
    }

    void Countdown()
    {
        time += 1f * Time.deltaTime;
    }
}
