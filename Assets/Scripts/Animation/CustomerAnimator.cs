using UnityEngine;
using UnityEngine.AI;

// Drives the customer's animator from what the customer is doing.
[DefaultExecutionOrder(-50)]
[RequireComponent(typeof(NavMeshAgent))]
public class CustomerAnimator : MonoBehaviour
{
    [SerializeField] private CustomerModelPicker modelPicker;

    [Tooltip("Optional. Overrides the controller on the model. Only works if all models use a Humanoid rig.")]
    [SerializeField] private RuntimeAnimatorController controllerOverride;

    [SerializeField] private float speedDampTime = 0.1f;

    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private static readonly int SittingHash = Animator.StringToHash("Sitting");
    private static readonly int EatingHash = Animator.StringToHash("Eating");

    private Animator animator;
    private NavMeshAgent agent;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();

        if (modelPicker == null)
        {
            modelPicker = GetComponent<CustomerModelPicker>();
        }

        if (modelPicker != null)
        {
            animator = modelPicker.Animator;
        }

        if (animator == null)
        {
            Debug.LogError($"{name}: No Animator found. Check the CustomerModelPicker.");
            return;
        }

        if (controllerOverride != null)
        {
            animator.runtimeAnimatorController = controllerOverride;
        }

        // The NavMeshAgent moves the customer, not the animation
        animator.applyRootMotion = false;
    }

    private void Update()
    {
        if (animator == null)
            return;

        animator.SetFloat(SpeedHash, agent.velocity.magnitude, speedDampTime, Time.deltaTime);
    }

    public void SetSitting(bool value)
    {
        if (animator != null)
            animator.SetBool(SittingHash, value);
    }

    public void SetEating(bool value)
    {
        if (animator != null)
            animator.SetBool(EatingHash, value);
    }
}