using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerAnimatorController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private PlayerMovement playerMovement;

    [Header("Emotes")]
    [SerializeField] private KeyCode pointKey = KeyCode.Z;
    [SerializeField] private KeyCode dialogueKey = KeyCode.X;

    [Header("Movement")]
    [SerializeField] private KeyCode runKey = KeyCode.LeftShift; // must match PlayerMovement
    [SerializeField] private float moveThreshold = 0.1f;

    [Header("Air")]
    [Tooltip("Upward speed above which the player counts as jumping up.")]
    [SerializeField] private float riseThreshold = 0.5f;
    [Tooltip("Time off the ground before counting as airborne. Stops small bumps and stairs triggering Falling.")]
    [SerializeField] private float airborneDelay = 0.1f;
    [Tooltip("Minimum time in the air for the landing animation to play.")]
    [SerializeField] private float minAirTimeForLanding = 0.25f;
    [SerializeField] private float landDuration = 0.4f;

    private enum PlayerAnimation
    {
        Idle,
        Walk,
        Run,
        Point,
        Dialogue,
        JumpUp,
        Falling,
        Land
    }

    // Animator bool parameter names (Idle has no parameter, all bools off = Idle)
    private static readonly string[] parameterNames =
    {
        "Walk", "Run", "Point", "Dialogue", "JumpUp", "Falling", "Land"
    };

    private CharacterController characterController;
    private PlayerAnimation current = PlayerAnimation.Idle;
    private PlayerAnimation emote = PlayerAnimation.Idle; // Idle = no emote playing

    private float airTime;
    private float landTimer;
    private bool wasAirborne;
    private bool airborne;
    private bool moving;
    private float verticalVelocity;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();

        if (playerMovement == null)
        {
            playerMovement = GetComponent<PlayerMovement>();
        }

        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        if (animator == null)
        {
            Debug.LogError($"{name}: No Animator found. Assign one in the inspector.");
            return;
        }

        // The CharacterController moves the player, not the animation
        animator.applyRootMotion = false;
    }

    // LateUpdate so this reads the movement PlayerMovement did this frame
    private void LateUpdate()
    {
        if (animator == null)
            return;

        UpdateMovementState();
        UpdateAirState();
        UpdateEmote();

        Apply(ChooseAnimation());
    }

    private void UpdateMovementState()
    {
        Vector3 velocity = characterController.velocity;
        verticalVelocity = velocity.y;

        float horizontalSpeed = new Vector2(velocity.x, velocity.z).magnitude;
        moving = horizontalSpeed > moveThreshold;
    }

    private void UpdateAirState()
    {
        bool grounded = characterController.isGrounded;

        if (!grounded)
        {
            airTime += Time.deltaTime;
        }

        airborne = !grounded && (airTime > airborneDelay || verticalVelocity > riseThreshold);

        if (airborne)
        {
            wasAirborne = true;
            landTimer = 0f;
        }
        else if (wasAirborne)
        {
            // Just touched down
            wasAirborne = false;

            if (airTime >= minAirTimeForLanding)
            {
                landTimer = landDuration;
            }
        }

        if (grounded)
        {
            airTime = 0f;
        }

        if (landTimer > 0f)
        {
            landTimer -= Time.deltaTime;
        }
    }

    private void UpdateEmote()
    {
        bool canAct = playerMovement == null || playerMovement.canMove;

        // Any of these cancels the emote
        if (!canAct || airborne || moving)
        {
            emote = PlayerAnimation.Idle;
            return;
        }

        // Don't start an emote in the middle of a landing
        if (landTimer > 0f)
            return;

        if (Input.GetKeyDown(pointKey))
        {
            ToggleEmote(PlayerAnimation.Point);
        }
        else if (Input.GetKeyDown(dialogueKey))
        {
            ToggleEmote(PlayerAnimation.Dialogue);
        }
    }

    // Pressing the key of the emote that is playing stops it, the other key switches to it
    private void ToggleEmote(PlayerAnimation newEmote)
    {
        emote = emote == newEmote ? PlayerAnimation.Idle : newEmote;
    }

    private PlayerAnimation ChooseAnimation()
    {
        if (airborne)
        {
            return verticalVelocity > riseThreshold ? PlayerAnimation.JumpUp : PlayerAnimation.Falling;
        }

        if (landTimer > 0f)
            return PlayerAnimation.Land;

        if (emote != PlayerAnimation.Idle)
            return emote;

        if (moving)
            return Input.GetKey(runKey) ? PlayerAnimation.Run : PlayerAnimation.Walk;

        return PlayerAnimation.Idle;
    }

    // Turns every bool off, then turns on the one for the chosen animation
    private void Apply(PlayerAnimation next)
    {
        if (next == current)
            return;

        current = next;

        string active = next == PlayerAnimation.Idle ? null : next.ToString();

        foreach (string parameter in parameterNames)
        {
            animator.SetBool(parameter, parameter == active);
        }
    }
}