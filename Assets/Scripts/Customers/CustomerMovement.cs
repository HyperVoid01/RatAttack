using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class CustomerMovement : MonoBehaviour
{
    [SerializeField] private CustomerData data;
    [SerializeField] private Transform pizzaSlot; // where customers hold pizza
    
    private GameObject currentPizza;
    private bool slotReserved;
    private bool stillQueuing; // true while this customer still occupies a physical line slot
    private bool hasLeft; // guards against Leave() running more than once
    public bool seated;
    private int queueSlot;
    private int lastSeenQueueVersion;
    private Table currentTable;
    private Transform seat;
    
    private NavMeshAgent agent;
    private CustomerBehaviour behaviour;
    
    private Coroutine waitForOrderRoutine;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        behaviour = GetComponent<CustomerBehaviour>();
        agent.speed = data.walkSpeed;
    }

    private void Start()
    {
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
        waitForOrderRoutine = StartCoroutine(WaitForOrder());
    }

    public IEnumerator WaitForOrder()
    {
        yield return new WaitForSeconds(data.orderWaitTime);
        HUDManager.Instance.RemoveOrderDetails(behaviour);
        StartCoroutine(Leave());
    }
    
    public IEnumerator PickupPizza(Transform target, GameObject pizza) // Pickup pizza from pickup station
    {
        if (waitForOrderRoutine != null)
            StopCoroutine(waitForOrderRoutine);
        PickupStation.Instance.waitingCustomers.Remove(behaviour);
        agent.SetDestination(target.position);
        
        yield return new WaitUntil(() => Vector3.Distance(transform.position, agent.destination) < 0.5f);
        
        currentPizza = pizza;
        currentPizza.GetComponent<Rigidbody>().isKinematic = true;
        currentPizza.transform.rotation = Quaternion.Euler(Vector3.zero);
        agent.SetDestination(seat.position);
        HUDManager.Instance.RemoveOrderDetails(behaviour);
        
        yield return new WaitUntil(() => Vector3.Distance(transform.position, agent.destination) < 0.5f);
        
        seated = true;
        currentPizza.transform.position = currentTable.pizzaSlot.transform.position;
        
        StartCoroutine(behaviour.EatPizza(pizza));
    }

    public IEnumerator Leave() // Leave restaurant
    {
        if (hasLeft)
            yield break; // already leaving/left - never double-cleanup or double-Destroy

        hasLeft = true;

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

        if (currentTable)
            currentTable.LeaveSeat(gameObject);
        
        agent.SetDestination(CustomerSpawner.Instance.exitPoint.position);
        yield return new WaitUntil(() => Vector3.Distance(transform.position, agent.destination) < 0.5f);

        CustomerSpawner.Instance.customerCount--;
        Destroy(gameObject);
    }
}

// using System.Collections;
// using UnityEngine;
// using UnityEngine.AI;
//
// public class CustomerMovement : MonoBehaviour
// {
//     [SerializeField] private CustomerData data;
//     [SerializeField] private Transform pizzaSlot; // where customers hold pizza
//     
//     private GameObject currentPizza;
//     private bool slotReserved;
//     public bool seated;
//     private int queueSlot;
//     private int lastSeenQueueVersion;
//     private Table currentTable;
//     private Transform seat;
//     
//     private NavMeshAgent agent;
//     private CustomerBehaviour behaviour;
//     
//     private Coroutine waitForOrderRoutine;
//
//     private void Awake()
//     {
//         agent = GetComponent<NavMeshAgent>();
//         behaviour = GetComponent<CustomerBehaviour>();
//         agent.speed = data.walkSpeed;
//     }
//
//     private void Start()
//     {
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
//         lastSeenQueueVersion = OrderStation.Instance.QueueVersion;
//         agent.SetDestination(OrderStation.Instance.lineSlots[queueSlot].transform.position);
//     }
//
//     // Called by CustomerBehaviour once a table has been secured and the
//     // order is taken. A table is guaranteed non-null here.
//     public void LeaveLine(Table table)
//     {
//         currentTable = table;
//         OrderStation.Instance.LeaveQueue(behaviour);
//         seat = table.TakeSeat(gameObject);
//         agent.SetDestination(seat.position);
//         waitForOrderRoutine = StartCoroutine(WaitForOrder());
//     }
//
//     public IEnumerator WaitForOrder()
//     {
//         yield return new WaitForSeconds(data.orderWaitTime);
//         HUDManager.Instance.RemoveOrderDetails(behaviour);
//         StartCoroutine(Leave());
//     }
//     
//     public IEnumerator PickupPizza(Transform target, GameObject pizza) // Pickup pizza from pickup station
//     {
//         if (waitForOrderRoutine != null)
//             StopCoroutine(waitForOrderRoutine);
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
//         currentPizza.transform.position = currentTable.pizzaSlot.transform.position;
//         
//         StartCoroutine(behaviour.EatPizza(pizza));
//     }
//
//     public IEnumerator Leave() // Leave restaurant
//     {
//         if (currentTable)
//             currentTable.LeaveSeat(gameObject);
//         
//         agent.SetDestination(CustomerSpawner.Instance.exitPoint.position);
//         yield return new WaitUntil(() => Vector3.Distance(transform.position, agent.destination) < 0.5f);
//
//         CustomerSpawner.Instance.customerCount--;
//         Destroy(gameObject);
//     }
// }