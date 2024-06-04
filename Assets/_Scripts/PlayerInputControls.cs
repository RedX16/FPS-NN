using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputControls : NetworkBehaviour
{
    [SerializeField] private float minViewDistance = 25f;
    [SerializeField] private Transform playerBody;
    private float _xRotation = 0f;
    [Range(0, 100)]
    private float mouseSensitivity = 10f;
    private PlayerInput _input;
    private Coroutine _moveCoroutine;

    public Action<Vector2> onMove;

    // Start is called before the first frame update
    void Awake()
    {
        Cursor.lockState = CursorLockMode.Locked;
        _input  = new PlayerInput();

        _input.Player.Look.performed += PlayerLook;
        _input.Player.Move.started += StartPlayerMove;
        _input.Player.Move.canceled += StopPlayerMove;
    }

    private void OnEnable()
    {
        _input.Enable();
    }

    private void OnDisable()
    {
        _input.Disable();
    }

    private void PlayerLook(InputAction.CallbackContext context)
    {
        var lookDelta = context.ReadValue<Vector2>();
        var lookActionDelta = lookDelta * mouseSensitivity * Time.deltaTime;

        _xRotation -= lookActionDelta.y;
        _xRotation = Mathf.Clamp(_xRotation, -75f, 75f); // Clamping the vertical rotation

        playerBody.localRotation = Quaternion.Euler(_xRotation, playerBody.localEulerAngles.y + lookActionDelta.x, 0f);
        Vector3 rotate = new Vector3(lookActionDelta.y, lookActionDelta.x, 0);

        playerBody.Rotate(rotate);
    }

    private void StartPlayerMove(InputAction.CallbackContext obj)
    {
        _moveCoroutine = StartCoroutine(MoveCoroutine());
    }

    private void StopPlayerMove(InputAction.CallbackContext obj)
    {
        if (_moveCoroutine != null)
        {
            StopCoroutine(_moveCoroutine);
        }
    }

    private IEnumerator MoveCoroutine()
    {
        while (true)
        {
            var moveInput = _input.Player.Move.ReadValue<Vector2>();
            onMove?.Invoke(moveInput);
            Debug.Log($"Move Input PIC {moveInput}");
            yield return null;
        }

        yield return null;
        // ReSharper disable once IteratorNeverReturns
    }
}

    // Update is called once per frame
    /*void Update()
    {
        if(!IsOwner) return;

        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, minViewDistance);
        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        playerBody.Rotate(Vector3.up * mouseX);
    }*/