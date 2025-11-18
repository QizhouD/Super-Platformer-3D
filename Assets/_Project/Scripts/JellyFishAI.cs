using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JellyFishAI : MonoBehaviour
{
    public GameObject jellyFish;
    public float speed;
    public Vector3 originalPos;
    public int height;

    public void Start()
    {
        originalPos = jellyFish.transform.position;
    }

    public void Update()
    {
        float y = Mathf.PingPong(Time.time * speed, 1) * height - height/2;
        jellyFish.transform.position = new Vector3(originalPos.x, originalPos.y + y, originalPos.z);
    }
}
