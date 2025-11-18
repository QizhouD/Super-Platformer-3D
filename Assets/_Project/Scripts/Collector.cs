using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Collector : MonoBehaviour
{
    public int points = 0;

    public int maxPoints;

    //Counting the amount of Waste objects
    void Start()
    {
        maxPoints = GameObject.FindGameObjectsWithTag("Waste").Length;
        Debug.Log(maxPoints);
    }

    private void OnGUI()
    {
        GUI.Label(new Rect(10, 10, 300, 20), "Waste Collected: " + points);
    }

    void Update()
    {
        // If there are no more pickups load the next scene
        if (points >= maxPoints)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
            //Debug.Log("You beat this level");
        }
    }
}
