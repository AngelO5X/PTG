using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Ruch")]
    public float walkSpeed = 5f;
    public float sprintSpeed = 9f;
    public float jumpForce = 2f;
    public float gravity = -9.81f;

    [Header("Mysz")]
    [Range(0f, 5f)]
    public float mouseSensitivity = 1f; // 0 = brak obrotu, 1 = normalnie, 2 = 2x szybciej
    public float baseMouseSpeed = 100f;
    public Transform cameraTransform;

    private CharacterController controller;
    private Vector3 velocity;
    private float xRotation = 0f;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        Look();
        Move();
        JumpAndGravity();
    }

    void Move()
    {
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        float currentSpeed = Input.GetKey(KeyCode.LeftShift) ? sprintSpeed : walkSpeed;

        Vector3 move = transform.right * x + transform.forward * z;
        controller.Move(move * currentSpeed * Time.deltaTime);
    }

    void Look()
    {
        if (mouseSensitivity <= 0f)
            return; // brak obracania

        float mouseX = Input.GetAxis("Mouse X") * baseMouseSpeed * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * baseMouseSpeed * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }

    void JumpAndGravity()
    {
        if (controller.isGrounded)
        {
            if (velocity.y < 0)
                velocity.y = -2f;

            // Skakanie w miejscu lub w ruchu
            if (Input.GetKeyDown(KeyCode.Space))
                velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
}
