using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController: MonoBehaviour
{
    public GameObject Camera;
    public float xSpeed = 100f;
    public float ySpeed = 100f;
    public float minY = -30f;
    public float maxY = 90f;
    public bool isMouseReverse = false;
    private float reverse;
    private float xRotation;
    private float yRotation;

    // Start is called before the first frame update
    void Start()
    {
        if (isMouseReverse)
        {
            reverse = 1f;
        }
        else
        {
            reverse = -1f;
        }
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        xRotation += Input.GetAxisRaw("Mouse Y") * xSpeed * Time.deltaTime * reverse;
        xRotation = Mathf.Clamp(xRotation, -1 * maxY, -1 * minY);
        yRotation -= Input.GetAxisRaw("Mouse X") * ySpeed * Time.deltaTime * reverse;
        transform.rotation = Quaternion.Euler(0f, yRotation, 0f);
        Camera.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        //transform.localRotation = Quaternion.Euler(xRotation, yRotation, 0f);
    }
}
