using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomerSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [Tooltip("Seconds between customers. The timer only runs while the line has an empty spot.")]
    [SerializeField] private Vector2 customerSpawnInterval = new Vector2(9f, 11f);
    [Tooltip("Wait before the very first customer, once the game starts.")]
    [SerializeField] private float firstCustomerDelay = 3f;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] public Transform exitPoint;

    [Header("Line Limit")]
    [Tooltip("Hard cap on how many customers can wait in line, whatever the star rating.")]
    [SerializeField] private int maxLineSize = 5;
    [Tooltip("How many customers may wait in line at each star rating. Element 0 = 0 stars, element 1 = 1 star, and so on. Ratings past the end use the last element.")]
    [SerializeField] private int[] maxLineByStars = { 2, 3, 3, 4, 4, 5 };

    [Header("Inspector")]
    [Tooltip("Seconds between inspector spawns. The inspector ignores the line limit.")]
    [SerializeField] private float inspectorSpawnInterval;
    
    [Header("Customer Mesh")]
    [SerializeField] private GameObject customerPrefab;
    [SerializeField] private GameObject inspectorPrefab;

    public int customerCount;
    public static CustomerSpawner Instance;

    // Every customer spawned by the line spawner (the inspector is not tracked)
    private readonly List<CustomerMovement> spawnedCustomers = new List<CustomerMovement>();
    private bool initialized;
    
    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }
    
    public void Initialize()
    {
        // Never run the spawn loops twice
        if (initialized)
            return;

        initialized = true;

        StartCoroutine(SpawnCycle());
        StartCoroutine(SpawnInspector());
    }

    // How many customers are currently waiting for, or standing in, a line spot
    public int CountInLine()
    {
        spawnedCustomers.RemoveAll(c => c == null);

        int count = 0;

        foreach (CustomerMovement customer in spawnedCustomers)
        {
            if (customer.TakesLineSpot)
                count++;
        }

        return count;
    }

    // How many customers the current star rating allows in line
    public int GetMaxLineSize()
    {
        int allowed = maxLineSize;

        if (maxLineByStars != null && maxLineByStars.Length > 0)
        {
            int stars = ReputationManager.Instance != null ? ReputationManager.Instance.starRating : 0;
            int index = Mathf.Clamp(stars, 0, maxLineByStars.Length - 1);
            allowed = maxLineByStars[index];
        }

        return Mathf.Clamp(allowed, 0, maxLineSize);
    }

    public bool LineHasRoom()
    {
        return CountInLine() < GetMaxLineSize();
    }

    private IEnumerator SpawnCycle()
    {
        float timer = 0f;
        float nextSpawnTime = firstCustomerDelay;

        while (true)
        {
            // Line is full: don't spawn, and the timer starts over once a spot opens
            if (!LineHasRoom())
            {
                timer = 0f;
                nextSpawnTime = RollSpawnTime();
                yield return null;
                continue;
            }

            timer += Time.deltaTime;

            if (timer >= nextSpawnTime)
            {
                SpawnCustomer();

                timer = 0f;
                nextSpawnTime = RollSpawnTime();
            }

            yield return null;
        }
    }

    private float RollSpawnTime()
    {
        return Random.Range(customerSpawnInterval.x, customerSpawnInterval.y);
    }

    private void SpawnCustomer()
    {
        GameObject customer = Instantiate(customerPrefab, spawnPoint.position, Quaternion.identity);
        customerCount++;

        if (customer.TryGetComponent(out CustomerMovement movement))
        {
            spawnedCustomers.Add(movement);
        }
    }

    private IEnumerator SpawnInspector()
    {
        GameObject inspector = null;
        
        while (true)
        {
            yield return new WaitForSeconds(inspectorSpawnInterval);
            
            if (inspector != null)
            {
                yield return null; // wait a frame, then check again
                continue;
            }

            inspector = Instantiate(inspectorPrefab, spawnPoint.position, Quaternion.identity);
        }
    }

    private void OnValidate()
    {
        maxLineSize = Mathf.Max(maxLineSize, 0);
        firstCustomerDelay = Mathf.Max(firstCustomerDelay, 0f);

        // Keep min <= max and never below 0.5s
        customerSpawnInterval.x = Mathf.Max(customerSpawnInterval.x, 0.5f);
        customerSpawnInterval.y = Mathf.Max(customerSpawnInterval.y, customerSpawnInterval.x);

        if (maxLineByStars != null)
        {
            for (int i = 0; i < maxLineByStars.Length; i++)
            {
                maxLineByStars[i] = Mathf.Clamp(maxLineByStars[i], 0, maxLineSize);
            }
        }
    }
}

// using System.Collections;
// using UnityEngine;
//
// public class CustomerSpawner : MonoBehaviour
// {
//     [Header("Spawn Settings")]
//     [SerializeField] private Vector2 spawnInterval; // Time between wave spawns
//     [SerializeField] private float inspectorSpawnInterval;
//     [SerializeField] private Vector2Int spawnAmount; // Amount of customers in wave
//     [SerializeField] private Vector2 spawnRate; // Time between customer spawns in a wave
//     [SerializeField] private float spawnAmountMultiplier; // Increase of customers
//     [SerializeField] private Transform spawnPoint;
//     [SerializeField] public Transform exitPoint;
//     [SerializeField] private int maxSpawnCount; // Max customers in game
//     
//     [Header("Customer Mesh")]
//     [SerializeField] private GameObject customerPrefab;
//     [SerializeField] private GameObject inspectorPrefab;
//
//     public int customerCount;
//     private bool waveSpawned;
//     public static CustomerSpawner Instance;
//     
//     private void Awake()
//     {
//         if (Instance != null)
//         {
//             Destroy(gameObject);
//             return;
//         }
//
//         Instance = this;
//     }
//     
//     public void Initialize()
//     {
//         StartCoroutine(SpawnCycle());
//         StartCoroutine(SpawnInspector());
//     }
//
//     private IEnumerator SpawnCustomers()
//     {
//         int amountToSpawn = Random.Range(spawnAmount.x, spawnAmount.y);
//         
//         for (int i = 0; i < amountToSpawn; i++)
//         {
//             Instantiate(customerPrefab, spawnPoint.transform.position, Quaternion.identity);
//             customerCount++;
//             
//             yield return new WaitForSeconds(Random.Range(spawnRate.x, spawnRate.y));
//         }
//
//         spawnAmount.x = (int)(spawnAmount.x * spawnAmountMultiplier);
//         spawnAmount.y = (int)(spawnAmount.y * spawnAmountMultiplier);
//         waveSpawned = true;
//     }
//
//     private IEnumerator SpawnCycle()
//     {
//         while (true)
//         {
//             // Skips spawning if there are too many customers
//             if (customerCount >= maxSpawnCount || TableManager.Instance.TotalAvailableSlots() == 0)
//             {
//                 yield return null; // wait a frame, then check again
//                 continue;
//             }
//         
//             waveSpawned = false;
//         
//             // Spawn wave of customers
//             StartCoroutine(SpawnCustomers());
//         
//             // Wait until all customers have been spawned
//             yield return new WaitUntil(() => waveSpawned);
//         
//             // Wait for interval
//             yield return new WaitForSeconds(Random.Range(spawnInterval.x, spawnInterval.y));
//         }
//     }
//
//     private IEnumerator SpawnInspector()
//     {
//         GameObject inspector = null;
//         
//         while (true)
//         {
//             yield return new WaitForSeconds(inspectorSpawnInterval);
//             
//             if (inspector != null)
//             {
//                 yield return null; // wait a frame, then check again
//                 continue;
//             }
//
//             inspector = Instantiate(inspectorPrefab, spawnPoint.transform.position, Quaternion.identity);
//         }
//     }
// }
