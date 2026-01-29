using Mirror;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 6f;
    public float sprintSpeed = 10f;
    public float acceleration = 10f;

    [Header("Jump (Normal)")]
    public float jumpForce = 7f;

    [Header("Under Water Jump")]
    public float waterLevelY = 0f;          // poziom wody
    public float waterJumpForce = 2.5f;     // S£ABSZY „SKOK” POD WOD¥
    public float waterGravityMultiplier = 0.4f;

    [Header("Gravity")]
    public float gravity = -20f;
    public float groundStickForce = -2f;

    [Header("Camera")]
    public Transform cameraTransform;
    public float mouseSensitivity = 2f;

    private CharacterController controller;
    private Vector3 velocity;
    private float xRotation;

    private Vector2 smoothInput;
    private Vector2 smoothVelocity;

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

        HandleCamera();
        HandleMovement();
        HandleVertical();
    }

    // ================= CAMERA =================
    void HandleCamera()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }

    // ================= HORIZONTAL =================
    void HandleMovement()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");

        Vector2 target = new Vector2(x, z).normalized;

        smoothInput = Vector2.SmoothDamp(
            smoothInput,
            target,
            ref smoothVelocity,
            1f / acceleration
        );

        float speed = Input.GetKey(KeyCode.LeftShift) ? sprintSpeed : walkSpeed;

        Vector3 move =
            transform.right * smoothInput.x +
            transform.forward * smoothInput.y;

        controller.Move(move * speed * Time.deltaTime);
    }

    // ================= VERTICAL LOGIC =================
    void HandleVertical()
    {
        bool underWater = transform.position.y < waterLevelY;

        float currentGravity = gravity;
        if (underWater)
            currentGravity *= waterGravityMultiplier;

        velocity.y += currentGravity * Time.deltaTime;

        // NORMALNY SKOK (nad wod¹)
        if (!underWater && controller.isGrounded && Input.GetKeyDown(KeyCode.Space))
        {
            velocity.y = jumpForce;
        }

        // SKOK POD WOD¥ (bez ziemi, bez WASD)
        if (underWater && Input.GetKey(KeyCode.Space))
        {
            velocity.y = waterJumpForce;
        }

        // PRZYKLEJ DO ZIEMI (tylko nad wod¹)
        if (!underWater && controller.isGrounded && velocity.y < 0)
            velocity.y = groundStickForce;

        controller.Move(Vector3.up * velocity.y * Time.deltaTime);
    }
}
