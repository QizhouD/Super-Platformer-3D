using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HorizontalPlatform : MonoBehaviour
{
    //To check the positions of where the platform will move
    public GameObject nextPlatform;
    private Vector3 originalPos;
    public Vector3 targetPos;
    public Vector3 nextSize;

    //To make the platform wait
    private float time;
    public float timeDelay;
    private bool isMovingForward;
    private bool isMovingBackward;

    //To set how fast it will move
    public float speed;


    // Start is called before the first frame update
    void Start()
    {
        originalPos = transform.position;
        targetPos = new Vector3(nextPlatform.transform.position.x, originalPos.y, nextPlatform.transform.position.z);
        CheckDirection();

        time = 0f;
        isMovingForward = false;
        isMovingBackward = false;
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
        if (transform.position.x == originalPos.x && transform.position.z == originalPos.z)
        {
            isMovingBackward = false;
            Countdown();
            if (time >= timeDelay)
            {
                isMovingForward = true;
                time = 0f;
            }
        }
        //if the platform is at the target position it waits
        else if (transform.position.x == targetPos.x && transform.position.z == targetPos.z)
        {
            isMovingForward = false;
            Countdown();
            if (time >= timeDelay)
            {
                isMovingBackward = true;
                time = 0f;
            }
        }

        if (isMovingForward)
        {
            MoveForward();
        }
        else if (isMovingBackward)
        {
            MoveBackward();
        }
    }

    void MoveForward()
    {
        transform.position = Vector3.MoveTowards(transform.position, targetPos, speed * Time.deltaTime);
    }

    void MoveBackward()
    {
        transform.position = Vector3.MoveTowards(transform.position, originalPos, speed * Time.deltaTime);
    }

    void Countdown()
    {
        time += 1f * Time.deltaTime;
    }

    //This checks which direction the platform is moving towards
    void CheckDirection()
    {
        //Get size of nextPlatform
        nextSize = nextPlatform.GetComponent<Collider>().bounds.size;
        Vector3 tempPos = targetPos;


        //Check direction on x
        if (originalPos.x >= nextPlatform.transform.position.x)
        {
            //Check direction on z
            if (originalPos.z >= nextPlatform.transform.position.z)
            {
                //Check if targetPos.x is closer or z
                if (originalPos.x - targetPos.x > originalPos.z - targetPos.z)
                {
                    //If x is closer the platform targets that side
                    Debug.Log("1");
                    targetPos.x = targetPos.x + nextSize.x / 2 + 5;
                }
                else if (originalPos.x - targetPos.x < originalPos.z - targetPos.z)
                {
                    Debug.Log("2");
                    targetPos.z = targetPos.z + nextSize.z / 2 + 5;
                }
            }
            else if (originalPos.z <= nextPlatform.transform.position.z)
            {
                if (originalPos.x - targetPos.x < targetPos.z - originalPos.z)
                {
                    Debug.Log("3");
                    targetPos.x = targetPos.x + nextSize.x / 2 + 5;
                }
                else if (originalPos.x - targetPos.x > targetPos.z - originalPos.z)
                {
                    Debug.Log("4");
                    targetPos.z = targetPos.z - nextSize.z / 2 - 5;
                }
            }
        }
        else if (originalPos.x <= nextPlatform.transform.position.x)
        {
            //Check direction on z
            if (originalPos.z >= nextPlatform.transform.position.z)
            {
                //Check if targetPos.x is closer or z
                if (targetPos.x - originalPos.x > originalPos.z - targetPos.z)
                {
                    //If x is closer the platform targets that side
                    Debug.Log("5");
                    targetPos.x = targetPos.x - nextSize.x / 2 - 5;
                }
                else if (targetPos.x - originalPos.x < originalPos.z - targetPos.z)
                {
                    Debug.Log("6");
                    targetPos.z = targetPos.z + nextSize.z / 2 + 5;
                }
                else
                {
                    targetPos.x = targetPos.x - nextSize.x / 2 - 5;
                }
            }
            else if (originalPos.z <= nextPlatform.transform.position.z)
            {
                if (targetPos.x - originalPos.x > targetPos.z - originalPos.z)
                {
                    Debug.Log("7");
                    targetPos.x = targetPos.x - nextSize.x / 2 - 5;
                }
                else if (targetPos.x - originalPos.x < targetPos.z - originalPos.z)
                {
                    Debug.Log("8");
                    targetPos.z = targetPos.z - nextSize.z / 2 - 5;
                }
            }
        }
    }
}
