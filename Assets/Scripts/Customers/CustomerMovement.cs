using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.AI;

public class CustomerMovement : MonoBehaviour
{
    [SerializeField] private CustomerData data;
    [SerializeField] private Transform pizzaSlot; // where customers hold pizza
    
    [SerializeField] private GameObject timerCanvas;
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private Camera targetCamera;

    [Header("Sitting")]
    [Tooltip("Where the customer's root (feet) ends up while sitting, as an offset in the seat Transform's own space. " +
             "Tune Y (up/down) and Z (forward/back) until they sit properly on the chair.")]
    [SerializeField] private Vector3 sitOffset = Vector3.zero;
    [Tooltip("Seconds to glide from where they stopped onto the chair.")]
    [SerializeField] private float sitSnapDuration = 0.25f;
    [Tooltip("How close to the end of the path counts as arrived.")]
    [SerializeField] private float arrivalTolerance = 0.1f;
    
    private GameObject currentPizza;
    private bool slotReserved;
    private bool stillQueuing; // true while this customer still occupies a physical line slot
    private bool hasLeft; // guards against Leave() running more than once
    public bool seated;
    private int queueSlot;
    private int lastSeenQueueVersion;
    private Table currentTable;
    private Transform seat;

    // True while the customer is placed on the chair and the agent isn't driving the transform
    private bool agentDetached;
    
    private NavMeshAgent agent;
    private CustomerBehaviour behaviour;
    private CustomerAnimator customerAnimator;
    
    private Coroutine waitForOrderRoutine;
    private Coroutine updateTimerRoutine;
    private Coroutine sitRoutine;

    // True once this customer has started leaving the restaurant
    public bool HasLeft => hasLeft;

    // True while this customer is waiting for, or standing in, a line spot.
    // CustomerSpawner counts these to decide whether the line has room.
    public bool TakesLineSpot => !hasLeft && (!slotReserved || stillQueuing);

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        behaviour = GetComponent<CustomerBehaviour>();
        customerAnimator = GetComponent<CustomerAnimator>();
        agent.speed = data.walkSpeed;

        if (customerAnimator == null)
        {
            Debug.LogWarning($"{name}: No CustomerAnimator on the customer root, so sit and eat animations won't play.");
        }
    }

    private void Start()
    {
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }
        
        timerCanvas.SetActive(false);
        
        TryReserveSlot();
    }

    private void Update()
    {
        // Line is physically full - keep polling for a free slot instead
        // of indexing into lineSlots with an invalid position.
        if (!slotReserved)
        {
            TryReserveSlot();
            return;
        }

        // Once a customer has left the physical line (served, seated, or
        // fled), they must stop reacting to queue shifts - otherwise a
        // later QueueVersion bump can yank a seated customer back toward
        // a line slot.
        if (!stillQueuing)
            return;

        // Catch up on ALL missed shifts at once, even if this customer is
        // still walking (not yet "inLine") or several serves happened
        // while they were en route. Using a delta instead of a boolean
        // check means no shift ever gets silently dropped.
        int versionDelta = OrderStation.Instance.QueueVersion - lastSeenQueueVersion;
        if (versionDelta > 0 && queueSlot > 0)
        {
            int newSlot = Mathf.Max(0, queueSlot - versionDelta);
            if (newSlot != queueSlot)
            {
                queueSlot = newSlot;
                behaviour.inLine = false;
                agent.SetDestination(OrderStation.Instance.lineSlots[queueSlot].transform.position);
            }

            lastSeenQueueVersion = OrderStation.Instance.QueueVersion;
        }

        if (!behaviour.inLine && Vector3.Distance(transform.position, agent.destination) < 0.5f)
        {
            behaviour.inLine = true;

            // Only register for service once - shifting forward later must not re-add customer
            if (!behaviour.joinedQueue)
            {
                behaviour.joinedQueue = true;
                OrderStation.Instance.JoinQueue(behaviour);
            }
        }
    }

    private void LateUpdate()
    {
        timerCanvas.transform.rotation = Quaternion.LookRotation(timerCanvas.transform.position - targetCamera.transform.position);
        
        if (currentPizza && !seated)
        {
            currentPizza.transform.position = pizzaSlot.position;
        }
    }

    private void TryReserveSlot()
    {
        int slot = OrderStation.Instance.ReserveSlot();
        if (slot == -1)
            return; // still no room in line, try again next frame

        queueSlot = slot;
        slotReserved = true;
        stillQueuing = true;
        lastSeenQueueVersion = OrderStation.Instance.QueueVersion;
        agent.SetDestination(OrderStation.Instance.lineSlots[queueSlot].transform.position);
    }

    // Called by CustomerBehaviour once a table has been secured and the
    // order is taken. A table is guaranteed non-null here.
    public void LeaveLine(Table table)
    {
        stillQueuing = false;
        currentTable = table;
        OrderStation.Instance.LeaveQueue(behaviour);
        seat = table.TakeSeat(gameObject);
        agent.SetDestination(seat.position);
        sitRoutine = StartCoroutine(SitAfterOrder());
        waitForOrderRoutine = StartCoroutine(WaitForOrder());
        updateTimerRoutine = StartCoroutine(UpdateTimer());
    }

    // Sits at the table while waiting for the pizza
    private IEnumerator SitAfterOrder()
    {
        yield return SitDown();

        sitRoutine = null;
    }

    // Waits until the customer has walked to the seat, moves them onto the chair, then plays the sit animation
    private IEnumerator SitDown()
    {
        yield return new WaitUntil(HasArrivedAtDestination);
        yield return SnapToSeat();

        if (customerAnimator != null)
            customerAnimator.SetSitting(true);
    }

    // Arrival check based on the agent itself, so it works even when the destination
    // isn't exactly on the NavMesh (like a chair)
    private bool HasArrivedAtDestination()
    {
        if (agent.pathPending)
            return false;

        if (agent.remainingDistance > agent.stoppingDistance + arrivalTolerance)
            return false;

        // Wait until the agent has actually stopped moving
        return !agent.hasPath || agent.velocity.sqrMagnitude < 0.01f;
    }

    // Glides the customer from where they stopped onto the chair, matching the seat's position and facing
    private IEnumerator SnapToSeat()
    {
        // Stop the agent driving the transform so the customer can be placed on the chair
        agentDetached = true;
        agent.updatePosition = false;
        agent.updateRotation = false;

        Vector3 startPosition = transform.position;
        Quaternion startRotation = transform.rotation;

        Vector3 endPosition = seat.TransformPoint(sitOffset);
        Quaternion endRotation = Quaternion.Euler(0f, seat.eulerAngles.y, 0f); // upright, facing the seat's forward

        float elapsed = 0f;

        while (elapsed < sitSnapDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / sitSnapDuration);

            transform.SetPositionAndRotation(
                Vector3.Lerp(startPosition, endPosition, t),
                Quaternion.Slerp(startRotation, endRotation, t));

            yield return null;
        }

        transform.SetPositionAndRotation(endPosition, endRotation);
    }

    // Puts the agent back on the NavMesh spot the customer walked to, so they can walk again
    private void StandUp()
    {
        if (!agentDetached)
            return;

        agentDetached = false;

        Vector3 navPosition = agent.nextPosition;

        agent.updatePosition = true;
        agent.updateRotation = true;
        agent.Warp(navPosition);
    }

    private IEnumerator WaitForOrder()
    {
        yield return new WaitForSeconds(data.orderWaitTime);
        HUDManager.Instance.RemoveOrderDetails(behaviour);
        StartCoroutine(Leave());
        
        if (behaviour.checkForRats)
            HUDManager.Instance.ShowInspectorReview(false);
    }

    private IEnumerator UpdateTimer()
    {
        timerCanvas.SetActive(true);
        float time = data.orderWaitTime;
        
        while (time > 0f)
        {
            timerText.text = Mathf.Round(time).ToString(); 
            time -= Time.deltaTime;
            yield return null;
        }
        
        timerCanvas.SetActive(false);
    }
    
    public IEnumerator PickupPizza(Transform target, GameObject pizza) // Pickup pizza from pickup station
    {
        if (waitForOrderRoutine != null)
            StopCoroutine(waitForOrderRoutine);
        if (updateTimerRoutine != null)
        {
            StopCoroutine(updateTimerRoutine);
            timerCanvas.SetActive(false);
        }

        // Stand up to go and get the pizza
        if (sitRoutine != null)
        {
            StopCoroutine(sitRoutine);
            sitRoutine = null;
        }

        if (customerAnimator != null)
            customerAnimator.SetSitting(false);

        StandUp();
        
        PickupStation.Instance.waitingCustomers.Remove(behaviour);
        agent.SetDestination(target.position);
        
        yield return new WaitUntil(() => Vector3.Distance(transform.position, agent.destination) < 0.5f);
        
        currentPizza = pizza;
        currentPizza.GetComponent<Rigidbody>().isKinematic = true;
        currentPizza.transform.rotation = Quaternion.Euler(Vector3.zero);
        agent.SetDestination(seat.position);
        HUDManager.Instance.RemoveOrderDetails(behaviour);

        // Walk back, settle onto the chair and sit
        yield return SitDown();
        
        seated = true;
        currentPizza.transform.position = currentTable.pizzaSlot.transform.position;
        
        StartCoroutine(behaviour.EatPizza(pizza));
    }

    // unhappy = true costs reputation (timed out, saw a rat, etc).
    // Pass false for a customer who was served and is leaving happy.
    public IEnumerator Leave(bool unhappy = true) // Leave restaurant
    {
        if (hasLeft)
            yield break; // already leaving/left - never double-cleanup or double-Destroy

        hasLeft = true;

        // Stand up and stop any sit/eat animation before walking out
        if (sitRoutine != null)
        {
            StopCoroutine(sitRoutine);
            sitRoutine = null;
        }

        if (customerAnimator != null)
        {
            customerAnimator.SetEating(false);
            customerAnimator.SetSitting(false);
        }

        StandUp();

        // If this customer is scared off (or otherwise pulled out) while
        // still walking to / standing in the physical line, they must be
        // released from OrderStation's bookkeeping here. Otherwise they
        // stay a "ghost" in customersInLine, reservedSlots never frees up,
        // and QueueVersion never fires - so everyone behind them is stuck.
        if (stillQueuing)
        {
            stillQueuing = false;
            behaviour.joinedQueue = false;
            OrderStation.Instance.RemoveFromLine(behaviour);
        }
        
        if (updateTimerRoutine != null)
        {
            StopCoroutine(updateTimerRoutine);
            timerCanvas.SetActive(false);
        }

        if (currentTable)
            currentTable.LeaveSeat(gameObject);
        
        if (unhappy)
            ReputationManager.Instance.DecreaseReputation(data.reputationDecrease);
        
        agent.SetDestination(CustomerSpawner.Instance.exitPoint.position);
        yield return new WaitUntil(() => Vector3.Distance(transform.position, agent.destination) < 0.5f);

        CustomerSpawner.Instance.customerCount--;
        Destroy(gameObject);
    }
}
// using System.Collections;
// using TMPro;
// using UnityEngine;
// using UnityEngine.AI;
//
// public class CustomerMovement : MonoBehaviour
// {
//     [SerializeField] private CustomerData data;
//     [SerializeField] private Transform pizzaSlot; // where customers hold pizza
//     
//     [SerializeField] private GameObject timerCanvas;
//     [SerializeField] private TMP_Text timerText;
//     [SerializeField] private Camera targetCamera;
//     
//     private GameObject currentPizza;
//     private bool slotReserved;
//     private bool stillQueuing; // true while this customer still occupies a physical line slot
//     private bool hasLeft; // guards against Leave() running more than once
//     public bool seated;
//     private int queueSlot;
//     private int lastSeenQueueVersion;
//     private Table currentTable;
//     private Transform seat;
//     
//     private NavMeshAgent agent;
//     private CustomerBehaviour behaviour;
//     private CustomerAnimator customerAnimator;
//     
//     private Coroutine waitForOrderRoutine;
//     private Coroutine updateTimerRoutine;
//     private Coroutine sitRoutine;
//
//     private void Awake()
//     {
//         agent = GetComponent<NavMeshAgent>();
//         behaviour = GetComponent<CustomerBehaviour>();
//         customerAnimator = GetComponent<CustomerAnimator>();
//         agent.speed = data.walkSpeed;
//     }
//
//     private void Start()
//     {
//         if (targetCamera == null)
//         {
//             targetCamera = Camera.main;
//         }
//         
//         timerCanvas.SetActive(false);
//         
//         TryReserveSlot();
//     }
//
//     private void Update()
//     {
//         // Line is physically full - keep polling for a free slot instead
//         // of indexing into lineSlots with an invalid position.
//         if (!slotReserved)
//         {
//             TryReserveSlot();
//             return;
//         }
//
//         // Once a customer has left the physical line (served, seated, or
//         // fled), they must stop reacting to queue shifts - otherwise a
//         // later QueueVersion bump can yank a seated customer back toward
//         // a line slot.
//         if (!stillQueuing)
//             return;
//
//         // Catch up on ALL missed shifts at once, even if this customer is
//         // still walking (not yet "inLine") or several serves happened
//         // while they were en route. Using a delta instead of a boolean
//         // check means no shift ever gets silently dropped.
//         int versionDelta = OrderStation.Instance.QueueVersion - lastSeenQueueVersion;
//         if (versionDelta > 0 && queueSlot > 0)
//         {
//             int newSlot = Mathf.Max(0, queueSlot - versionDelta);
//             if (newSlot != queueSlot)
//             {
//                 queueSlot = newSlot;
//                 behaviour.inLine = false;
//                 agent.SetDestination(OrderStation.Instance.lineSlots[queueSlot].transform.position);
//             }
//
//             lastSeenQueueVersion = OrderStation.Instance.QueueVersion;
//         }
//
//         if (!behaviour.inLine && Vector3.Distance(transform.position, agent.destination) < 0.5f)
//         {
//             behaviour.inLine = true;
//
//             // Only register for service once - shifting forward later must not re-add customer
//             if (!behaviour.joinedQueue)
//             {
//                 behaviour.joinedQueue = true;
//                 OrderStation.Instance.JoinQueue(behaviour);
//             }
//         }
//     }
//
//     private void LateUpdate()
//     {
//         timerCanvas.transform.rotation = Quaternion.LookRotation(timerCanvas.transform.position - targetCamera.transform.position);
//         
//         if (currentPizza && !seated)
//         {
//             currentPizza.transform.position = pizzaSlot.position;
//         }
//     }
//
//     private void TryReserveSlot()
//     {
//         int slot = OrderStation.Instance.ReserveSlot();
//         if (slot == -1)
//             return; // still no room in line, try again next frame
//
//         queueSlot = slot;
//         slotReserved = true;
//         stillQueuing = true;
//         lastSeenQueueVersion = OrderStation.Instance.QueueVersion;
//         agent.SetDestination(OrderStation.Instance.lineSlots[queueSlot].transform.position);
//     }
//
//     // Called by CustomerBehaviour once a table has been secured and the
//     // order is taken. A table is guaranteed non-null here.
//     public void LeaveLine(Table table)
//     {
//         stillQueuing = false;
//         currentTable = table;
//         OrderStation.Instance.LeaveQueue(behaviour);
//         seat = table.TakeSeat(gameObject);
//         agent.SetDestination(seat.position);
//         sitRoutine = StartCoroutine(SitWhenArrived());
//         waitForOrderRoutine = StartCoroutine(WaitForOrder());
//         updateTimerRoutine = StartCoroutine(UpdateTimer());
//     }
//
//     // Plays the sit animation once the customer reaches their seat
//     private IEnumerator SitWhenArrived()
//     {
//         yield return new WaitUntil(() => Vector3.Distance(transform.position, seat.position) < 0.5f);
//
//         if (customerAnimator != null)
//             customerAnimator.SetSitting(true);
//
//         sitRoutine = null;
//     }
//
//     private IEnumerator WaitForOrder()
//     {
//         yield return new WaitForSeconds(data.orderWaitTime);
//         HUDManager.Instance.RemoveOrderDetails(behaviour);
//         StartCoroutine(Leave());
//     }
//
//     private IEnumerator UpdateTimer()
//     {
//         timerCanvas.SetActive(true);
//         float time = data.orderWaitTime;
//         
//         while (time > 0f)
//         {
//             timerText.text = Mathf.Round(time).ToString(); 
//             time -= Time.deltaTime;
//             yield return null;
//         }
//         
//         timerCanvas.SetActive(false);
//     }
//     
//     public IEnumerator PickupPizza(Transform target, GameObject pizza) // Pickup pizza from pickup station
//     {
//         if (waitForOrderRoutine != null)
//             StopCoroutine(waitForOrderRoutine);
//         if (updateTimerRoutine != null)
//         {
//             StopCoroutine(updateTimerRoutine);
//             timerCanvas.SetActive(false);
//         }
//
//         // Stand up to go and get the pizza
//         if (sitRoutine != null)
//         {
//             StopCoroutine(sitRoutine);
//             sitRoutine = null;
//         }
//
//         if (customerAnimator != null)
//             customerAnimator.SetSitting(false);
//         
//         PickupStation.Instance.waitingCustomers.Remove(behaviour);
//         agent.SetDestination(target.position);
//         
//         yield return new WaitUntil(() => Vector3.Distance(transform.position, agent.destination) < 0.5f);
//         
//         currentPizza = pizza;
//         currentPizza.GetComponent<Rigidbody>().isKinematic = true;
//         currentPizza.transform.rotation = Quaternion.Euler(Vector3.zero);
//         agent.SetDestination(seat.position);
//         HUDManager.Instance.RemoveOrderDetails(behaviour);
//         
//         yield return new WaitUntil(() => Vector3.Distance(transform.position, agent.destination) < 0.5f);
//         
//         seated = true;
//
//         if (customerAnimator != null)
//             customerAnimator.SetSitting(true);
//
//         currentPizza.transform.position = currentTable.pizzaSlot.transform.position;
//         
//         StartCoroutine(behaviour.EatPizza(pizza));
//     }
//
//     public IEnumerator Leave() // Leave restaurant
//     {
//         if (hasLeft)
//             yield break; // already leaving/left - never double-cleanup or double-Destroy
//
//         hasLeft = true;
//
//         // Stand up and stop any sit/eat animation before walking out
//         if (sitRoutine != null)
//         {
//             StopCoroutine(sitRoutine);
//             sitRoutine = null;
//         }
//
//         if (customerAnimator != null)
//         {
//             customerAnimator.SetEating(false);
//             customerAnimator.SetSitting(false);
//         }
//
//         // If this customer is scared off (or otherwise pulled out) while
//         // still walking to / standing in the physical line, they must be
//         // released from OrderStation's bookkeeping here. Otherwise they
//         // stay a "ghost" in customersInLine, reservedSlots never frees up,
//         // and QueueVersion never fires - so everyone behind them is stuck.
//         if (stillQueuing)
//         {
//             stillQueuing = false;
//             behaviour.joinedQueue = false;
//             OrderStation.Instance.RemoveFromLine(behaviour);
//         }
//         
//         if (updateTimerRoutine != null)
//         {
//             StopCoroutine(updateTimerRoutine);
//             timerCanvas.SetActive(false);
//         }
//
//         if (currentTable)
//             currentTable.LeaveSeat(gameObject);
//         
//         ReputationManager.Instance.DecreaseReputation(data.reputationDecrease);
//         
//         agent.SetDestination(CustomerSpawner.Instance.exitPoint.position);
//         yield return new WaitUntil(() => Vector3.Distance(transform.position, agent.destination) < 0.5f);
//
//         CustomerSpawner.Instance.customerCount--;
//         Destroy(gameObject);
//     }
// }
