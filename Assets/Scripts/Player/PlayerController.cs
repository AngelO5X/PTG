using Mirror;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 6f;
    public float sprintSpeed = 10f;
    public float acceleration = 10f;

    [Header("Jump Settings")]
    public float jumpHeight = 1.5f;
    public float gravity = -20f;

    [Header("Camera Settings")]
    public Transform cameraTransform;
    public float mouseSensitivity = 2f;
    public float maxRotationPerFrame = 10f;

    private CharacterController controller;
    private Vector3 velocity;
    private float xRotation;

    private Vector2 currentInputVector;
    private Vector2 smoothInputVelocity;

    void Start()
    {
        controller = GetComponent<CharacterController>();

        if (!isLocalPlayer)
        {
            cameraTransform.gameObject.SetActive(false);
            return;
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        if (!isLocalPlayer) return;

        if (PauseMenu_.GameIsPaused)
        {
            if (Cursor.lockState != CursorLockMode.None)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            return;
        }

        HandleCamera();
        HandleMovement();
        HandleGravity();
    }

    // ================= CAMERA =================
    void HandleCamera()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        mouseX = Mathf.Clamp(mouseX, -maxRotationPerFrame, maxRotationPerFrame);
        mouseY = Mathf.Clamp(mouseY, -maxRotationPerFrame, maxRotationPerFrame);

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }

    // ================= MOVEMENT =================
    void HandleMovement()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");

        Vector2 targetInput = new Vector2(x, z).normalized;
        currentInputVector = Vector2.SmoothDamp(
            currentInputVector,
            targetInput,
            ref smoothInputVelocity,
            1f / acceleration
        );

        float speed = Input.GetKey(KeyCode.LeftShift) ? sprintSpeed : walkSpeed;

        CmdMove(currentInputVector, speed);
    }

    [Command]
    void CmdMove(Vector2 input, float speed)
    {
        Vector3 move =
            transform.right * input.x +
            transform.forward * input.y;

        controller.Move(move * speed * Time.deltaTime);
    }

    // ================= GRAVITY =================
    void HandleGravity()
    {
        if (controller.isGrounded && velocity.y < 0)
            velocity.y = -2f;

        if (Input.GetKeyDown(KeyCode.Space) && controller.isGrounded)
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
}
