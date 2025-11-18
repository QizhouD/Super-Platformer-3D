using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControl : MonoBehaviour
{
    public GameObject mainCamera;
    public float xSpeed = 200f;
    public float ySpeed = 300f;
    public float minY = -30f;
    public float maxY = 90f;
    public bool isMouseReverse = false;
    private float reverse;
    private float xRotation;
    private float yRotation;

    public float forwardSpeed = 10f;
    public float backwardSpeed = 5f;
    public float sideSpeed = 5f;
    private Vector3 verticalMovement;
    private Vector3 horizontalMovement;

    public float jumpForce;
    private bool isGround;

    private bool doubleJump = true;
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
    void Update()
    {
        Jump();
        if (isGround && !Input.GetButton("Jump"))
            {
                doubleJump = false;
            }
    }

    void FixedUpdate()
    {
        CameraFixedUpdate();
        Move();
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag.Equals("Ground"))
        {
            isGround = true;
        }
     }

    void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.tag.Equals("Ground"))
        {
            isGround = false;
        }
    }

    void CameraFixedUpdate()
    {
        xRotation += Input.GetAxisRaw("Mouse Y") * xSpeed * Time.deltaTime * reverse;
        xRotation = Mathf.Clamp(xRotation, -1 * maxY, -1 * minY);
        yRotation -= Input.GetAxisRaw("Mouse X") * ySpeed * Time.deltaTime * reverse;
        transform.localRotation = Quaternion.Euler(0f, yRotation, 0f);
        mainCamera.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
    }

    void Move()
    {
        if (isGround)
        {
            if (Input.GetAxis("Vertical") > 0)
            {
                verticalMovement = transform.forward * Input.GetAxis("Vertical") * forwardSpeed * Time.deltaTime;
            }
            else
            {
                verticalMovement = transform.forward * Input.GetAxis("Vertical") * backwardSpeed * Time.deltaTime;
            }
            if (Input.GetAxis("Horizontal") != 0)
            {
                verticalMovement = verticalMovement / 2f;
            }
            horizontalMovement = transform.right * Input.GetAxis("Horizontal") * sideSpeed * Time.deltaTime;
            GetComponent<Rigidbody>().MovePosition(transform.position + verticalMovement + horizontalMovement);
        }
        else
        {
            GetComponent<Rigidbody>().MovePosition(transform.position + verticalMovement + horizontalMovement);
        }
    }

    void Jump()
    {
        if (isGround || doubleJump)
        {
            if (Input.GetButtonDown("Jump"))
            {
                GetComponent<Rigidbody>().AddForce(transform.up * jumpForce);

                doubleJump = !doubleJump;
            }
        }
    }

    void Crouch()
    {

    }

    void Hitted()
    {

    }

    void Cure()
    {

    }
}
