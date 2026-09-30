using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class ManagerController : MonoBehaviour
{
    [SerializeField] private Transform[] positions;
    [SerializeField] private bool loop;

    [Header("Facing")]
    [SerializeField] private float turnSpeed = 180f; // degrees per second
    [SerializeField] private float arrivalTolerance = 0.1f;

    private NavMeshAgent navMeshAgent;
    private int currentIndex = -1;
    private int lastMoveFrame = -1;
    private bool hasArrived = true;

    // True from the moment a move starts until the NPC has arrived
    public bool IsWalking => !hasArrived;

    private void Awake()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        if (currentIndex < 0)
            return;

        if (!hasArrived && HasReachedDestination())
        {
            hasArrived = true;
        }

        if (hasArrived)
        {
            FacePositionDirection();
        }
        
        if (hasArrived && currentIndex == positions.Length - 1)
        {
            Destroy(this.gameObject);
        }
    }

    public void NextPosition()
    {
        if (positions == null || positions.Length == 0)
            return;

        // Ignore a second call in the same frame
        if (lastMoveFrame == Time.frameCount)
        {
            Debug.LogWarning("NextPosition called twice in one frame - ignoring duplicate.\n" + StackTraceUtility.ExtractStackTrace());
            return;
        }

        int nextIndex = currentIndex + 1;

        if (nextIndex >= positions.Length)
        {
            if (!loop)
                return;

            nextIndex = 0;
        }

        lastMoveFrame = Time.frameCount;
        currentIndex = nextIndex;
        hasArrived = false;
        navMeshAgent.SetDestination(positions[currentIndex].position);
    }

    private bool HasReachedDestination()
    {
        if (navMeshAgent.pathPending)
            return false;

        if (navMeshAgent.remainingDistance > navMeshAgent.stoppingDistance + arrivalTolerance)
            return false;

        // Wait until the agent has actually stopped moving
        return !navMeshAgent.hasPath || navMeshAgent.velocity.sqrMagnitude < 0.01f;
    }

    private void FacePositionDirection()
    {
        Transform target = positions[currentIndex];

        // Flatten to the horizontal plane so the NPC stays upright
        Vector3 forward = target.forward;
        forward.y = 0f;

        if (forward.sqrMagnitude < 0.0001f)
            return;

        Quaternion targetRotation = Quaternion.LookRotation(forward.normalized, Vector3.up);

        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            targetRotation,
            turnSpeed * Time.deltaTime);
    }
}