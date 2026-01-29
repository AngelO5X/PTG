using Mirror;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 6f;
    public float sprintSpeed = 10f;
    public float acceleration = 10f;

    [Header("Jump")]
    public float jumpForce = 7f;

    [Header("Water Physics")]
    public float waterLevelY = 0f;
    public float waterJumpForce = 2.5f;
    public float waterGravityMultiplier = 0.4f;

    [Header("Gravity")]
    public float gravity = -20f;
    public float groundStickForce = -2f;

    [Header("Camera")]
    public Transform cameraTransform;
    public float mouseSensitivity = 2f;
    public bool useVR = false;
    public Transform vrCamera;

    private CharacterController controller;
    private Vector3 velocity;
    private float xRotation;
    private Vector2 smoothInput;
    private Vector2 smoothVelocity;

    void Start()
    {
        controller = GetComponent<CharacterController>();

        // WA¯NE: Nie wy³¹czamy ju¿ controllera! Ma byæ w³¹czony od razu.
        if (!isLocalPlayer)
        {
            if (cameraTransform != null) cameraTransform.gameObject.SetActive(false);
            return;
        }

        if (!useVR)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    void Update()
    {
        if (!isLocalPlayer) return;

        HandleCamera();
        HandleMovement();
        HandleVertical();
    }

    void HandleCamera()
    {
        if (useVR && vrCamera != null)
        {
            Vector3 headForward = vrCamera.forward;
            headForward.y = 0f;
            if (headForward.sqrMagnitude > 0.001f)
                transform.rotation = Quaternion.LookRotation(headForward);
        }
        else
        {
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, -90f, 90f);

            cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
            transform.Rotate(Vector3.up * mouseX);
        }
    }

    void HandleMovement()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");

        Vector2 target = new Vector2(x, z).normalized;
        smoothInput = Vector2.SmoothDamp(smoothInput, target, ref smoothVelocity, 1f / acceleration);

        float speed = Input.GetKey(KeyCode.LeftShift) ? sprintSpeed : walkSpeed;

        Vector3 forward = cameraTransform.forward;
        Vector3 right = cameraTransform.right;
        forward.y = 0f;
        right.y = 0f;
        forward.Normalize();
        right.Normalize();

        Vector3 move = forward * smoothInput.y + right * smoothInput.x;
        controller.Move(move * speed * Time.deltaTime);
    }

    void HandleVertical()
    {
        bool underWater = transform.position.y < waterLevelY;
        float currentGravity = gravity;
        if (underWater) currentGravity *= waterGravityMultiplier;

        // Resetowanie prêdkoœci, gdy stoimy na ziemi
        if (!underWater && controller.isGrounded && velocity.y < 0)
        {
            velocity.y = groundStickForce;
        }

        velocity.y += currentGravity * Time.deltaTime;

        if (!underWater && controller.isGrounded && Input.GetKeyDown(KeyCode.Space))
            velocity.y = jumpForce;
        else if (underWater && Input.GetKey(KeyCode.Space))
            velocity.y = waterJumpForce;

        controller.Move(Vector3.up * velocity.y * Time.deltaTime);
    }
}