using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : NetworkBehaviour
{
    [Header("Movement")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float sprintSpeed = 8f;
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private float gravity = -20f;

    [Header("Mouse Look")]
    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField] private Transform cameraHolder;

    [Header("Recoil Recovery")]
    [SerializeField] private float recoilRecoverySpeed = 8f;

    private CharacterController _cc;
    private Vector3 _velocity;
    private float _xRotation;

    // Recoil offsets — added on top of mouse input, decay over time
    private float _recoilX;
    private float _recoilY;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;

        Cursor.lockState = CursorLockMode.Locked;

        if (Camera.main != null)
        {
            Camera.main.transform.SetParent(cameraHolder);
            Camera.main.transform.localPosition = Vector3.zero;
            Camera.main.transform.localRotation = Quaternion.identity;
        }
    }

    void Awake() => _cc = GetComponent<CharacterController>();

    void Update()
    {
        if (!IsOwner) return; // CRITICAL — prevents controlling other players

        HandleMouseLook();
        HandleMovement();
        DecayRecoil();
    }

    void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        // Combine mouse input with active recoil offset
        _xRotation -= mouseY;
        _xRotation += _recoilY;          // recoil pushes view up
        _xRotation = Mathf.Clamp(_xRotation, -80f, 80f);

        cameraHolder.localRotation = Quaternion.Euler(_xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * (mouseX + _recoilX));

        // Consume the recoil this frame so it doesn't keep re-adding
        _recoilY = 0f;
        _recoilX = 0f;
    }

    void HandleMovement()
    {
        bool isGrounded = _cc.isGrounded;
        if (isGrounded && _velocity.y < 0) _velocity.y = -2f;

        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");
        bool sprinting = Input.GetKey(KeyCode.LeftShift);

        float speed = sprinting ? sprintSpeed : walkSpeed;
        Vector3 move = transform.right * x + transform.forward * z;
        _cc.Move(move * speed * Time.deltaTime);

        if (Input.GetButtonDown("Jump") && isGrounded)
            _velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);

        _velocity.y += gravity * Time.deltaTime;
        _cc.Move(_velocity * Time.deltaTime);
    }

    private float _pendingRecoilX, _pendingRecoilY;

    // Called by WeaponController each time a shot is fired
    public void AddRecoil(Vector2 recoil)
    {
        // Apply instantly as a kick (felt immediately)
        _recoilY += recoil.y;
        _recoilX += recoil.x;
    }

    void DecayRecoil()
    {
        // Smoothly returns view toward center between shots
        _pendingRecoilX = Mathf.Lerp(_pendingRecoilX, 0f, Time.deltaTime * recoilRecoverySpeed);
        _pendingRecoilY = Mathf.Lerp(_pendingRecoilY, 0f, Time.deltaTime * recoilRecoverySpeed);
    }
}