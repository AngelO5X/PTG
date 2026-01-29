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
    public float waterLevelY = 0f;
    public float waterJumpForce = 2.5f;
    public float waterGravityMultiplier = 0.4f;

    [Header("Gravity")]
    public float gravity = -20f;
    public float groundStickForce = -2f;

    [Header("Camera")]
    public Transform cameraTransform;
    public float mouseSensitivity = 2f;

    [Header("VR")]
    public bool useVR = false;          //  PRZE£¥CZNIK VR
    public Transform vrCamera;          //  ta sama kamera (Tracked Pose Driver)

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

    // ================= CAMERA =================
    void HandleCamera()
    {
        if (useVR)
            HandleVRCamera();
        else
            HandleMouseCamera();
    }

    // ======== MYSZ ========
    void HandleMouseCamera()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }

    // ======== VR (HEAD ONLY) ========
    void HandleVRCamera()
    {
        if (vrCamera == null) return;

        // kierunek patrzenia g³owy
        Vector3 headForward = vrCamera.forward;

        // rzut na p³aszczyznê poziom¹ (ignorujemy góra/dó³)
        headForward.y = 0f;

        if (headForward.sqrMagnitude < 0.0001f)
            return;

        Quaternion targetRotation = Quaternion.LookRotation(headForward);

        // obracamy TYLKO cia³o (yaw)
        transform.rotation = targetRotation;
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

        // KIERUNKI WZGLÊDEM KAMERY (VR + NON-VR)
        Vector3 forward = cameraTransform.forward;
        Vector3 right = cameraTransform.right;

        // obliczenia róchu
        forward.y = 0f;
        right.y = 0f;

        forward.Normalize();
        right.Normalize();

        Vector3 move =
            forward * smoothInput.y +
            right * smoothInput.x;

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

        // NORMALNY SKOK
        if (!underWater && controller.isGrounded && Input.GetKeyDown(KeyCode.Space))
        {
            velocity.y = jumpForce;
        }

        // SKOK POD WOD¥ (bez pod³o¿a)
        if (underWater && Input.GetKey(KeyCode.Space))
        {
            velocity.y = waterJumpForce;
        }

        // PRZYKLEJ DO ZIEMI
        if (!underWater && controller.isGrounded && velocity.y < 0)
            velocity.y = groundStickForce;

        controller.Move(Vector3.up * velocity.y * Time.deltaTime);
    }
}
