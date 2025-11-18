using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class Tutorial : MonoBehaviour
{
    public void Play()
    {
        SceneManager.LoadScene("Level_Tutorial");
    }
 
    public void Quit()
    {
        Application.Quit();
    }
}

