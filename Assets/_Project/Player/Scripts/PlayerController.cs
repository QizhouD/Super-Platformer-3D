using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public CharacterController controller;
    public float forwardSpeed;
    public float rightSpeed;
    public float jumpSpeed;
    public float gravity;
    public float sprintMagnification;
    private bool isSprint;
    public float crochMagnification;
    private bool isCroch;
    private float magnification;
    private Vector2 moveValue;
    private float verticalVelocity;
    private Vector3 horizontalVelocity;
    public Vector3 originalPos;
    public string stealthTag;
    private int onAirCount;
    public int jumpDelay;
    private Vector3 lastLocalPosition;

    public bool doubleJump;

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        GameObject hitObject = hit.gameObject;
        //Debug.Log(hitObject.tag);
        if (hitObject.tag == stealthTag && isCroch)
        {
            this.gameObject.tag = "stealthPlayer";
        }
        else
        {
            this.gameObject.tag = "Player";
        }
        if (hitObject.tag == "Enemy") {
            controller.transform.position = originalPos;
        }
        if (hitObject.tag == "Ground")
        {
            this.transform.SetParent(hitObject.transform, true);
            lastLocalPosition = transform.parent.position;
            //this.parent = hitObject;
        }
    }

    void OnMove(InputValue value)
    {
        moveValue = value.Get<Vector2>();
        if(moveValue.y < 0)
        {
            moveValue = moveValue / 2;
        }
    }

    void OnJump(InputValue value)
    {
        if(onAirCount < jumpDelay || doubleJump )
        {
            verticalVelocity = jumpSpeed;
            if (doubleJump)
            {
                Vector3 forward = transform.TransformDirection(Vector3.forward);
                Vector3 right = transform.TransformDirection(Vector3.right);
                Vector3 movement = transform.right * moveValue.x / 1.5f * rightSpeed + transform.forward * moveValue.y * forwardSpeed / 1.5f;
                controller.Move(movement * Time.deltaTime + transform.up * verticalVelocity * Time.deltaTime);
                horizontalVelocity = new Vector3(controller.velocity.x, 0, controller.velocity.z);
            }
            else
            {
                controller.Move(Vector3.up * jumpSpeed * Time.deltaTime);
            }
            doubleJump = !doubleJump;
        }
        isCroch = false;
    }

    void OnSprint(InputValue value)
    {
        isSprint = !isSprint;
        if(isSprint)
        {
            isCroch = false;
        }
    }

    void OnCroch(InputValue value)
    {
        isCroch = !isCroch;
        if(isCroch)
        {
            isSprint = false;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<CharacterController>();
        magnification = 1;
        isCroch = false;
        isSprint = false;
        originalPos = controller.transform.position;
        onAirCount = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (controller.isGrounded && !Input.GetButton("Jump"))
        {
            doubleJump = false;
        }
    }

    void FixedUpdate()
    {
        if (isCroch)
        {
            magnification = crochMagnification;
            controller.height = 1;
        }
        else
        {
            controller.height = 2;
        }
        if(isSprint)
        {
            magnification = sprintMagnification;
        }
        if(controller.isGrounded)
        {
            onAirCount = 0;
            Vector3 forward = transform.TransformDirection(Vector3.forward);
            Vector3 right = transform.TransformDirection(Vector3.right);
            Vector3 parentMovement = new(0, 0, 0);
            if (transform.parent != null)
            {
                parentMovement = transform.parent.position - lastLocalPosition;
                //Debug.Log(parentMovement);
            }
            Vector3 movement = right * moveValue.x * rightSpeed + forward * moveValue.y * forwardSpeed;
            controller.Move(movement * magnification * Time.deltaTime + parentMovement);
            magnification = 1;
            horizontalVelocity = new Vector3(controller.velocity.x, 0, controller.velocity.z);
            if (horizontalVelocity == Vector3.zero)
            {
                isSprint = false;
            }
        }
        else
        {
            this.transform.parent = null;
            onAirCount++;
            //Debug.Log("in here");
            this.gameObject.tag = "Player";
            verticalVelocity -= gravity;
            if (true)
            {
                if(horizontalVelocity == new Vector3(0, 0, 0))
                {
                    Vector3 forward = transform.TransformDirection(Vector3.forward);
                    Vector3 right = transform.TransformDirection(Vector3.right);
                    Vector3 movement = transform.right * moveValue.x / 2.0f * rightSpeed + transform.forward * moveValue.y * forwardSpeed / 2.0f;
                    controller.Move(movement * Time.deltaTime + transform.up * verticalVelocity * Time.deltaTime);
                    horizontalVelocity = new Vector3(controller.velocity.x, 0, controller.velocity.z);
                }
                else
                {
                    controller.Move(horizontalVelocity * Time.deltaTime + Vector3.up * verticalVelocity * Time.deltaTime);
                }
            }
            else
            {
                Vector3 forward = transform.TransformDirection(Vector3.forward);
                Vector3 right = transform.TransformDirection(Vector3.right);
                horizontalVelocity = Vector3.RotateTowards(horizontalVelocity, transform.right, moveValue.x * 1.0f, 0.0f);
                controller.Move(horizontalVelocity * Time.deltaTime + transform.up * verticalVelocity * Time.deltaTime);
            }
        }
        
        if (controller.transform.position.y <= -20)
        {
            Debug.Log("teleport");
            Debug.Log("old position is" + controller.transform.position);
            Debug.Log(controller.transform.position.y);
            controller.transform.position = originalPos;
            Debug.Log("current position is" + controller.transform.position);
        }
        //Debug.Log(controller.isGrounded);
        if (transform.parent != null)
        {
            lastLocalPosition = transform.parent.position;
        }
    }

    void SetPosition(Vector3 position)
    {
        Debug.Log("teleport");
        Debug.Log("old position is" + controller.transform.position);
        Debug.Log(controller.transform.position.y);
        controller.transform.position = position;
        Debug.Log("current position is" + controller.transform.position);
    }
}
