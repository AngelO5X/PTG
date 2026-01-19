using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // --- SETTINGS ---
    [Header("Movement Settings")]
    public float walkSpeed = 6f;
    public float sprintSpeed = 10f;
    public float acceleration = 10f;

    [Header("Jump Settings")]
    public float jumpHeight = 1.5f;
    public float gravity = -20f;
    public float jumpBufferTime = 0.2f;
    public float coyoteTime = 0.2f;

    [Header("Camera Settings")]
    public Transform cameraTransform;
    public float mouseSensitivity = 2f;
    public float maxRotationPerFrame = 10f;

    // --- INTERNAL STATE ---
    private CharacterController controller;
    private Vector3 velocity;
    private float xRotation = 0f;

    private float jumpBufferCounter;
    private float coyoteTimeCounter;

    private Vector2 currentInputVector;
    private Vector2 smoothInputVelocity;

    // --- INITIALIZATION ---
    void Start()
    {
        controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // --- MAIN LOOP ---
    void Update()
    {
        HandleCamera();
        HandleMovement();
        HandleGravityAndJump();
    }

    // --- CAMERA LOGIC ---
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

    // --- MOVEMENT LOGIC ---
    void HandleMovement()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");

        Vector2 targetInput = new Vector2(x, z).normalized;
        currentInputVector = Vector2.SmoothDamp(currentInputVector, targetInput, ref smoothInputVelocity, 1f / acceleration);

        float currentSpeed = Input.GetKey(KeyCode.LeftShift) ? sprintSpeed : walkSpeed;

        Vector3 move = transform.right * currentInputVector.x + transform.forward * currentInputVector.y;
        controller.Move(move * currentSpeed * Time.deltaTime);
    }

    // --- PHYSICS & JUMP LOGIC ---
    void HandleGravityAndJump()
    {
        // Coyote Time
        if (controller.isGrounded)
        {
            coyoteTimeCounter = coyoteTime;
            if (velocity.y < 0) velocity.y = -2f;
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
        }

        // Jump Buffer
        if (Input.GetKeyDown(KeyCode.Space))
        {
            jumpBufferCounter = jumpBufferTime;
        }
        else
        {
            jumpBufferCounter -= Time.deltaTime;
        }

        // Jump Execution
        if (jumpBufferCounter > 0f && coyoteTimeCounter > 0f)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            jumpBufferCounter = 0f;
            coyoteTimeCounter = 0f;
        }

        // Gravity
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
}