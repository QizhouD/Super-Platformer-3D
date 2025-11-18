using UnityEngine;
using System.Collections;

public class SharkText : MonoBehaviour
{
    //    public Transform cube;
    bool isShowTip;
    public bool WindowShow = false;
    //    // Use this for initialization
    void Start()
    {
        isShowTip = false;
    }
    void OnMouseEnter()
    {
        isShowTip = true;
        //Debug.Log (cube.name);//Get the name of object

    }
    void OnMouseExit()
    {
        isShowTip = false;
    }

    void OnGUI()
    {
        if (isShowTip)
        {
            GUIStyle style1 = new GUIStyle();
            style1.fontSize = 30;
            style1.normal.textColor = Color.red;
            GUI.Label(new Rect(Input.mousePosition.x, Screen.height - Input.mousePosition.y, 400, 50), "Shark!", style1);

        }
        if (WindowShow)
            GUI.Window(0, new Rect(30, 30, 200, 100), MyWindow, "Shark!");
    }

    //GUI Function
    void MyWindow(int WindowID)
    {
        GUILayout.Label("Danger,avoid it!");
    }
    //On mouse Click
    void OnMouseDown()
    {
        Debug.Log("show");
        if (WindowShow)
            WindowShow = false;
        else
            WindowShow = true;
    }
}