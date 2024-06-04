using System;
using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : NetworkBehaviour
{
    [SerializeField] float moveSpeed = 10f;
    [SerializeField] float sprintMultiplier = 2f;
    [SerializeField] float jumpForce = 5f;
    [SerializeField] CinemachineVirtualCamera virtualCamera;
    [SerializeField] private PlayerInputControls playerInputControls;
    [Range(0, 20)]
    [SerializeField] private float maxAirSpeed = 1;
    int ownerPriority = 15;
    bool isGrounded = true;
    Vector2 moveInput;
    Rigidbody rb;
    PlayerInput input;

    public static PlayerMovement LocalInstance { get; private set; }


    private void Awake()
    {
        playerInputControls.onMove += Move;
    }

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
        else
        {
            virtualCamera.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        if (!IsOwner) { return; }
        if(input.Player.Jump.ReadValue<float>() > 0 && isGrounded)
        { 
            Jump(); 
            isGrounded = false;
        }
    }

    private void Move(Vector2 moveDelta)
    {
        bool isSprintHeld = input.Player.Sprint.activeControl != null;

        // Calculate the desired movement direction
        Vector3 moveDirection = new Vector3(moveDelta.x, 0f, moveDelta.y);
        moveDirection = transform.TransformDirection(moveDirection);
        moveDirection *= moveSpeed * (isSprintHeld ? sprintMultiplier : 1f);

        // Calculate the new position
        Vector3 newPosition = rb.position + Time.deltaTime *
            new Vector3 (moveDirection.x * (isGrounded ? 1f : maxAirSpeed), moveDirection.y, moveDirection.z * (isGrounded ? 1f : maxAirSpeed));

        // Move the Rigidbody to the new position
        rb.MovePosition(newPosition);
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