using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : NetworkBehaviour
{
    [SerializeField] float moveSpeed = 10f;
    [SerializeField] float sprintMultiplyer = 2f;
    [SerializeField] float jumpForce = 5f;
    [SerializeField] CinemachineVirtualCamera virtualCamera;
    int ownerPriority = 15;
    bool isGrounded = true;
    Vector2 moveInput;
    Rigidbody rb;
    PlayerInput input;
    public static PlayerMovement LocalInstance { get; private set; }

    private void Start()
    {
        input = new PlayerInput();
        rb = GetComponent<Rigidbody>();
        input.Enable();
    }

    public override void OnNetworkSpawn()
    {
        if(IsOwner)
        {
            LocalInstance = this;
            virtualCamera.Priority = ownerPriority;
        }
    }

    private void Update()
    {
        if (!IsOwner) { return; }
        Move();
        if(input.Player.Jump.ReadValue<float>() > 0 && isGrounded) 
        { 
            Jump(); 
            isGrounded = false;
        }
    }

    void Move()
    {
        bool isSprintHeld = (input.Player.Sprint.activeControl != null) ? true : false;

        if(!isSprintHeld)
        {
            moveInput = input.Player.Move.ReadValue<Vector2>();
            Vector3 playerVelocity = new Vector3(moveInput.x * moveSpeed, rb.velocity.y, moveInput.y * moveSpeed);
            rb.velocity = transform.TransformDirection(playerVelocity);
        }
        else
        {
            moveInput = input.Player.Move.ReadValue<Vector2>();
            Vector3 playerVelocity = new Vector3(moveInput.x * moveSpeed, rb.velocity.y, moveInput.y * moveSpeed * sprintMultiplyer);
            rb.velocity = transform.TransformDirection(playerVelocity);
        }

    }

    void Jump()
    {
        rb.AddForce(new Vector3(0,jumpForce,0), ForceMode.Impulse);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Ground")
        {
            isGrounded = true;
        }
    }
}
