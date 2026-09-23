using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class RatController : MonoBehaviour, ITargetable
{
    [SerializeField] private RatData data;
    [SerializeField] private LayerMask pizzaLayerMask;
    [SerializeField] private Transform pizzaSlot;
    [SerializeField] private GameObject aliveMesh;
    [SerializeField] private GameObject deadMesh;
    
    public Collider boxCollider;
    
    private GameObject currentPizza;
    private RigidbodyInterpolation pizzaOriginalInterpolation;
    private int currentHealth;
    [HideInInspector] public Transform player;
    private NavMeshAgent agent;
    private float idleTimer;
    private bool isWaiting;
    private bool isFleeing;
    private bool isChasingPizza; // true from the moment a pizza is spotted until it's grabbed/lost

    // Cleanup
    private Coroutine cleanUpRoutine;
    private Coroutine shakeRoutine;
    private Vector3 shakeOrigin;

    private void Start()
    {
        boxCollider = GetComponent<Collider>();
        agent = GetComponent<NavMeshAgent>();
        agent.speed = data.speed;
        currentHealth = data.maxHealth;

        StartCoroutine(HuntPizzas());
    }

    private void Update()
    {
        if (currentHealth <= 0)
            return;
        
        float distToPlayer = Vector3.Distance(transform.position, player.position);

        // Flees when player is nearby
        if (distToPlayer <= data.fleeRadius)
        {
            isFleeing = true;
            isWaiting = false;
            Flee();
            return;
        }

        // Stops fleeing when player is far away
        if (isFleeing)
        {
            isFleeing = false;
            SetNewDestination();
        }

        // While chasing a pizza, don't let the idle/wander logic below
        // reassign the agent's destination out from under HuntPizzas().
        if (isChasingPizza)
            return;

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            if (!isWaiting)
            {
                isWaiting = true;
                idleTimer = Random.Range(data.minIdleTime, data.maxIdleTime);
            }
            else
            {
                idleTimer -= Time.deltaTime;
                if (idleTimer <= 0f)
                {
                    isWaiting = false;
                    SetNewDestination();
                }
            }
        }
    }

    private IEnumerator HuntPizzas()
    {
        while (currentHealth > 0)
        {
            yield return new WaitForSeconds(0.2f);

            // If already have pizza, skip
            if (currentPizza)
            {
                continue;
            }
            
            GameObject foundPizza = LookForPizzas();

            if (!foundPizza)
                continue;

            isChasingPizza = true;
            isWaiting = false; // don't let a stale idle timer fire mid-chase
            agent.SetDestination(foundPizza.transform.position);

            // Wait on actual proximity to the pizza rather than the agent's
            // current destination/remainingDistance - keeps this correct
            // even if something else (e.g. Flee) briefly reassigns the
            // destination mid-chase.
            yield return new WaitUntil(() => foundPizza == null || (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance));

            isChasingPizza = false;

            // Pizza got grabbed by another rat (or destroyed) while we were walking over
            if (!foundPizza)
                continue;

            GrabPizza(foundPizza);
        }
    }

    private void GrabPizza(GameObject pizza)
    {
        currentPizza = pizza;

        // Stop it from being picked up by other rats / interacting with the world while carried
        if (pizza.TryGetComponent(out Collider pizzaCollider))
            pizzaCollider.enabled = false;
        
        if (pizza.TryGetComponent(out Rigidbody pizzaRb))
        {
            pizzaRb.isKinematic = true;
            pizzaOriginalInterpolation = pizzaRb.interpolation;
            pizzaRb.interpolation = RigidbodyInterpolation.None;
        }

        pizza.transform.SetParent(pizzaSlot);
        pizza.transform.localPosition = Vector3.zero;
        pizza.transform.localRotation = Quaternion.identity;

        StartCoroutine(EatPizza());
    }

    private IEnumerator EatPizza()
    {
        yield return new WaitForSeconds(data.eatDuration);

        if (currentPizza)
        {
            Destroy(currentPizza);
            currentPizza = null;
            transform.localScale *= data.sizeGrowthMultiplier;
        }
        // HuntPizzas loop picks back up automatically once currentPizza is null
    }

    private GameObject LookForPizzas()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, data.pizzaDetectRadius, pizzaLayerMask);

        GameObject nearest = null;
        float nearestDist = float.MaxValue;

        foreach (Collider hit in hits)
        {
            if (!hit.CompareTag("Pizza"))
                continue;

            float dist = Vector3.Distance(transform.position, hit.transform.position);
            if (dist < nearestDist)
            {
                nearestDist = dist;
                nearest = hit.gameObject;
            }
        }

        return nearest;
    }

    private void Flee()
    {
        Vector3 fleeTarget = GetFleeLocation();
        agent.SetDestination(fleeTarget);
    }

    private Vector3 GetFleeLocation()
    {
        // Direction from player to rat, extended out to fleeDistance
        Vector3 directionAwayFromPlayer = (transform.position - player.position).normalized;
        Vector3 candidatePoint = transform.position + directionAwayFromPlayer * data.fleeDistance;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(candidatePoint, out hit, data.fleeDistance, NavMesh.AllAreas))
        {
            return hit.position;
        }

        // Fallback if that direction is off the mesh 
        return GetRandomLocation();
    }

    private void SetNewDestination()
    {
        Vector3 randomPoint = GetRandomLocation();
        agent.SetDestination(randomPoint);
    }

    private Vector3 GetRandomLocation()
    {
        Vector3 randomDirection = Random.insideUnitSphere * data.wanderRadius;
        randomDirection += transform.position;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomDirection, out hit, data.wanderRadius, NavMesh.AllAreas))
        {
            return hit.position;
        }

        return transform.position;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, data.fleeRadius);
        
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, data.wanderRadius);
        
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, data.pizzaDetectRadius);
    }

    public void TakeDamage(int damage)
    {
        currentHealth = Mathf.Clamp(currentHealth - damage, 0, data.maxHealth);

        if (currentHealth <= 0)
        {
            StopAllCoroutines();

            // Drop whatever pizza it was carrying instead of destroying it with the rat
            if (currentPizza)
            {
                currentPizza.transform.SetParent(null);

                if (currentPizza.TryGetComponent(out Collider pizzaCollider))
                    pizzaCollider.enabled = true;

                if (currentPizza.TryGetComponent(out Rigidbody pizzaRb))
                {
                    pizzaRb.isKinematic = false;
                    pizzaRb.interpolation = pizzaOriginalInterpolation;
                }
            }
            
            RatSpawner.Instance.ratCount--;
            agent.enabled = false;
            GetComponent<Collider>().enabled = false;
            aliveMesh.SetActive(false);
            deadMesh.SetActive(true);
            Debug.Log("Died");
        }
    }

    public void Heal(int healing)
    {
        currentHealth = Mathf.Clamp(currentHealth + healing, 0, data.maxHealth);
    }

    public void StartCleaning()
    {
        if (cleanUpRoutine != null)
            return;

        shakeOrigin = transform.localPosition;
        cleanUpRoutine = StartCoroutine(CleanUp());
        shakeRoutine = StartCoroutine(CleanUpShakeAnimation());
    }

    public void StopCleaning()
    {
        if (cleanUpRoutine == null)
            return;
        
        StopCoroutine(cleanUpRoutine);
        cleanUpRoutine = null;
        StopCoroutine(shakeRoutine);
        shakeRoutine = null;
        
        transform.localPosition = shakeOrigin;
    }

    private IEnumerator CleanUp()
    {
        yield return new WaitForSeconds(data.cleanUpDuration);
        StopCoroutine(shakeRoutine);
        Destroy(gameObject);
    }

    private IEnumerator CleanUpShakeAnimation()
    {
        Vector3 shakeDirection;
        
        while (true)
        {
            yield return new WaitForSeconds(0.1f);
            
            shakeDirection = Random.onUnitSphere * 0.05f;
            transform.localPosition = shakeOrigin + shakeDirection;

            yield return new WaitForSeconds(0.1f);
            
            transform.localPosition = shakeOrigin;
        }
    }
}