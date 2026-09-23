using System.Collections;
using UnityEngine;

public class CustomerSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private Vector2 spawnInterval; // Time between wave spawns
    [SerializeField] private Vector2Int spawnAmount; // Amount of customers in wave
    [SerializeField] private Vector2 spawnRate; // Time between customer spawns in a wave
    [SerializeField] private float spawnAmountMultiplier; // Increase of customers
    [SerializeField] private Transform spawnPoint;
    [SerializeField] public Transform exitPoint;
    [SerializeField] private int maxSpawnCount; // Max customers in game
    
    [Header("Customer Mesh")]
    [SerializeField] private GameObject customerPrefab;

    public int customerCount;
    private bool waveSpawned;
    public static CustomerSpawner Instance;
    
    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }
    
    private void Start()
    {
        StartCoroutine(SpawnCycle());
    }

    private IEnumerator SpawnCustomers()
    {
        int amountToSpawn = Random.Range(spawnAmount.x, spawnAmount.y);
        
        for (int i = 0; i < amountToSpawn; i++)
        {
            Instantiate(customerPrefab, spawnPoint.transform.position, Quaternion.identity);
            customerCount++;
            
            yield return new WaitForSeconds(Random.Range(spawnRate.x, spawnRate.y));
        }

        spawnAmount.x = (int)(spawnAmount.x * spawnAmountMultiplier);
        spawnAmount.y = (int)(spawnAmount.y * spawnAmountMultiplier);
        waveSpawned = true;
    }

    private IEnumerator SpawnCycle()
    {
        while (true)
        {
            // Skips spawning if there are too many customers
            if (customerCount >= maxSpawnCount || TableManager.Instance.TotalAvailableSlots() == 0)
            {
                yield return null; // wait a frame, then check again
                continue;
            }
        
            waveSpawned = false;
        
            // Spawn wave of customers
            StartCoroutine(SpawnCustomers());
        
            // Wait until all customers have been spawned
            yield return new WaitUntil(() => waveSpawned);
        
            // Wait for interval
            yield return new WaitForSeconds(Random.Range(spawnInterval.x, spawnInterval.y));
        }
    }
}
