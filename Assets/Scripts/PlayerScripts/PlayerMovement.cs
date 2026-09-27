using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private Camera playerCamera;
    [SerializeField] private float walkSpeed = 6f;
    [SerializeField] private float runSpeed = 12f;
    [SerializeField] private float jumpPower = 7f;
    [SerializeField] private float gravity = 10f;
    [SerializeField] private float lookSpeed = 2f;
    [SerializeField] private float lookXLimit = 45f;
    // [SerializeField] private float defaultHeight = 2f;
    // [SerializeField] private float crouchHeight = 1f;
    // [SerializeField] private float crouchSpeed = 3f;
    
    [Header("Recoil")]
    [SerializeField] private float recoilSnappiness = 8f;  // how fast the kick snaps in
    [SerializeField] private float recoilReturnSpeed = 4f; // how fast it settles back

    private Vector3 moveDirection = Vector3.zero;
    private float rotationX = 0;
    private CharacterController characterController;
    
    private float recoilCurrent; 
    private float recoilTarget;

    public bool canMove = true;
    
    public static PlayerMovement Instance;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // Converts directions into world space
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);

        bool isRunning = Input.GetKey(KeyCode.LeftShift);
        float curSpeedX = canMove ? (isRunning ? runSpeed : walkSpeed) * Input.GetAxis("Vertical") : 0;
        float curSpeedY = canMove ? (isRunning ? runSpeed : walkSpeed) * Input.GetAxis("Horizontal") : 0;
        float movementDirectionY = moveDirection.y;
        moveDirection = (forward * curSpeedX) + (right * curSpeedY);

        // Jumping
        if (Input.GetButton("Jump") && canMove && characterController.isGrounded)
        {
            moveDirection.y = jumpPower;
        }
        else
        {
            moveDirection.y = movementDirectionY;
        }

        // Gravity
        if (!characterController.isGrounded)
        {
            moveDirection.y -= gravity * Time.deltaTime;
        }

        // Crouching
        // if (Input.GetKey(KeyCode.R) && canMove)
        // {
        //     characterController.height = crouchHeight;
        //     walkSpeed = crouchSpeed;
        //     runSpeed = crouchSpeed;
        //
        // }
        // else
        // {
        //     characterController.height = defaultHeight;
        //     walkSpeed = 6f;
        //     runSpeed = 12f;
        // }

        // Move Player
        characterController.Move(moveDirection * Time.deltaTime);
        
        // Recoil decay (runs every frame regardless of canMove, so a kick still settles even if input is frozen)
        recoilTarget = Mathf.Lerp(recoilTarget, 0f, recoilReturnSpeed * Time.deltaTime);
        recoilCurrent = Mathf.Lerp(recoilCurrent, recoilTarget, recoilSnappiness * Time.deltaTime);
        
        // Mouse Look
        if (canMove)
        {
            rotationX += -Input.GetAxis("Mouse Y") * lookSpeed;
            rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
            playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
            transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * lookSpeed, 0);
        }
        
        // Single writer to camera localRotation: mouse look + recoil combined here
        playerCamera.transform.localRotation = Quaternion.Euler(rotationX - recoilCurrent, 0, 0);
    }
    
    // Adds an upward camera kick (in degrees). Call from weapon scripts on fire.
    public void AddRecoil(float angle)
    {
        recoilTarget += angle;
    }
}