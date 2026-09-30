using UnityEngine;

public class ManagerAnimatorController : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private ManagerController managerController;

    [Tooltip("Animation to play on ARRIVAL at each position, in the same order as the positions.")]
    [SerializeField] private ManagerAnimations[] animations;
    [SerializeField] private bool loop;

    private int currentIndex = -1;
    private int lastAnimationFrame = -1;
    private bool wasWalking;

    // Animator bool parameter names, one per enum value (Idle has no parameter)
    private static readonly string[] parameterNames =
    {
        "Walk", "Sit", "Run", "Point", "Dialogue", "Land", "JumpUp", "Falling", "Eating"
    };

    private void Awake()
    {
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        if (managerController == null)
        {
            managerController = GetComponent<ManagerController>();
        }

        if (animator == null)
        {
            Debug.LogError($"{name}: No Animator found. Assign one in the inspector.");
        }

        if (managerController == null)
        {
            Debug.LogError($"{name}: No ManagerController found. Assign one in the inspector.");
        }
    }

    private void Update()
    {
        if (managerController == null)
            return;

        // Switch between the walk animation and the arrival animation automatically
        bool walking = managerController.IsWalking;

        if (walking != wasWalking)
        {
            wasWalking = walking;
            ApplyState();
        }
    }

    // Selects the animation for the next position. It plays once the NPC arrives.
    public void NextAnimation()
    {
        if (animator == null || animations == null || animations.Length == 0)
            return;

        // Ignore a second call in the same frame
        if (lastAnimationFrame == Time.frameCount)
            return;

        int nextIndex = currentIndex + 1;

        if (nextIndex >= animations.Length)
        {
            if (!loop)
                return;

            nextIndex = 0;
        }

        lastAnimationFrame = Time.frameCount;
        currentIndex = nextIndex;

        wasWalking = managerController != null && managerController.IsWalking;
        ApplyState();
    }

    // Call this if the dialogue restarts from the beginning
    public void ResetSequence()
    {
        currentIndex = -1;
    }

    private void ApplyState()
    {
        if (animator == null)
            return;

        if (wasWalking)
        {
            SetOnly("Walk");
            return;
        }

        if (currentIndex < 0 || currentIndex >= animations.Length)
        {
            SetOnly(null);
            return;
        }

        ManagerAnimations arrival = animations[currentIndex];
        SetOnly(arrival == ManagerAnimations.Idle ? null : arrival.ToString());
    }

    // Turns every bool off, then turns on the one given (null = all off = Idle)
    private void SetOnly(string activeParameter)
    {
        foreach (string parameter in parameterNames)
        {
            animator.SetBool(parameter, parameter == activeParameter);
        }
    }
}

// New values are added at the end so existing inspector arrays keep their values.
// Each name must match an Animator bool parameter (Idle = all bools off).
public enum ManagerAnimations
{
    Walk,
    Sit,
    Run,
    Point,
    Dialogue,
    Idle,
    Land,
    JumpUp,
    Falling,
    Eating
}