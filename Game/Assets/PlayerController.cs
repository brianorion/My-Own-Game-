using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField]
    private Camera playerCam;
    [SerializeField]
    private float playerSpeed;
    [SerializeField]
    private float rotationSensitivity;
    [SerializeField]
    private float maximumCameraRotation;
    [SerializeField]
    private float jumpForce = 5f;
    [SerializeField]
    private float crounchingRatio = 0.5f;
    [SerializeField]
    [Range(0.01f, 0.1f)]
    private float crounchingTransitionValue;
    [SerializeField]
    [Range(0f, 1f)]
    private float crounchingSpeedDebuffRatio = 0.8f;
    [SerializeField]
    private float pickUpDistance = 5f;

    private Rigidbody rb;
    private bool isGrounded = true;
    private bool isCrounching = false;
    private float originalYScale;

    private void Awake()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        originalYScale = transform.localScale.y;
    }

    private void FixedUpdate()
    {
        PlayerMovement();
    }

    private void PlayerMovement()
    {
        // Will have two components: rotation and position
        Move();
        Rotate();
    }

    private void Move()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 camF = playerCam.transform.forward;
        Vector3 camR = playerCam.transform.right;

        camF.y = 0f;
        camR.y = 0f;

        camF = camF.normalized;
        camR = camR.normalized;

        Vector3 velocity = camF * vertical + camR * horizontal;
        velocity = velocity.normalized * playerSpeed * Time.fixedDeltaTime;

        if (isCrounching)
        {
            velocity *= crounchingSpeedDebuffRatio;
        }

        rb.MovePosition(rb.position + velocity);

        Jump();
        Crounching();
    }

    private void Rotate()
    {
        // there are two rotations we need to modify: the rotation of the player and the rotation of the player.
        PlayerRotate();
        CameraRotate();
    }

    private void PlayerRotate()
    {
        float yRot = Input.GetAxis("Mouse X");

        Vector3 rotation = new Vector3(0f, yRot, 0f) * rotationSensitivity;

        rb.MoveRotation(rb.rotation * Quaternion.Euler(rotation));
    }

    private void CameraRotate()
    {
        float xRot = Input.GetAxis("Mouse Y");

        Vector3 rotation = new Vector3(xRot, 0f, 0f) * rotationSensitivity;

        playerCam.transform.Rotate(-rotation);
        float currentX = playerCam.transform.localEulerAngles.x;
        currentX = Mathf.Clamp(currentX, -maximumCameraRotation, maximumCameraRotation);

        playerCam.transform.localEulerAngles = new Vector3(currentX, 0f, 0f);
    }

    private void Jump()
    {
        if (isGrounded && Input.GetKeyDown(KeyCode.Space))
        {
            Vector3 jumpForceVector = new Vector3(0f, jumpForce, 0f);
            rb.AddForce(jumpForceVector, ForceMode.Impulse);
            isGrounded = false;
        }
    }

    private void Crounching()
    {
        // subject to change. Once we have a model this code here will change. 
        float yScale = transform.localScale.y;

        if (Input.GetKey(KeyCode.LeftControl))
        {
            if (yScale > crounchingRatio)
            {
                yScale -= crounchingTransitionValue;
                transform.localScale = new Vector3(1f, yScale, 1f);
            }
            isCrounching = true;
        }
        else
        {
            if (yScale < originalYScale)
            {
                yScale += crounchingTransitionValue;
                transform.localScale = new Vector3(1f, yScale, 1f);
            }
            isCrounching = false;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.tag == "ground")
        {
            isGrounded = true;
        }
    }
}