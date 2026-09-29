using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class ManagerController : MonoBehaviour
{
    [SerializeField] private Transform[] positions;
    [SerializeField] private bool loop;

    private NavMeshAgent navMeshAgent;
    private int currentIndex = -1;
    private int lastMoveFrame = -1;

    private void Awake()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
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
        navMeshAgent.SetDestination(positions[currentIndex].position);
    }
}