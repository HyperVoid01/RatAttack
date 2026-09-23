using System.Collections;
using UnityEngine;

public class RatSpawner : MonoBehaviour
{
    // x - min || y - max
    [Header("Spawn Settings")]
    [SerializeField] private Vector2 spawnInterval; // Time between wave spawns
    [SerializeField] private Vector2Int spawnAmount; // Amount of rats in wave
    [SerializeField] private Vector2 spawnRate; // Time between rat spawns in a wave
    [SerializeField] private float spawnAmountMultiplier; // Increase of rats
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private int maxSpawnCount; // Max rats in game
    [SerializeField] private GameObject playerRef;
    
    [Header("Rat Mesh")]
    [SerializeField] private GameObject ratPrefab;
    [SerializeField] private float minSize;
    [SerializeField] private float maxSize;

    public int ratCount;
    private bool waveSpawned;
    public static RatSpawner Instance;

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

    private IEnumerator SpawnRats()
    {
        int amountToSpawn = Random.Range(spawnAmount.x, spawnAmount.y);
        
        for (int i = 0; i < amountToSpawn; i++)
        {
            if (ratCount >= maxSpawnCount)
                yield break;
            
            // Get random spawn point
            Vector3 spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)].position;
            float randomSize = Random.Range(minSize, maxSize);
            
            GameObject spawnedRat = Instantiate(ratPrefab, spawnPoint, Quaternion.identity);
            spawnedRat.transform.localScale *= randomSize;
            spawnedRat.GetComponent<RatController>().player = playerRef.transform;
            ratCount++;
            
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
            // Skips spawning if there are too many rats
            if (ratCount >= maxSpawnCount)
                continue;
            
            waveSpawned = false;
            
            // Spawn wave of rats
            StartCoroutine(SpawnRats());
            
            // Wait until all rats have been spawned
            yield return new WaitUntil(() => waveSpawned);
            
            // Wait for interval
            yield return new WaitForSeconds(Random.Range(spawnInterval.x, spawnInterval.y));
        }
    }
}
