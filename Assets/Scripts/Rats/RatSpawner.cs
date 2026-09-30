using System.Collections;
using UnityEngine;

public class RatSpawner : MonoBehaviour
{
    // x - min || y - max
    [Header("Spawn Settings")]
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private GameObject playerRef;

    [Header("Gradual Ramp Up")]
    [Tooltip("Seconds to go from the start settings to the end settings.")]
    [SerializeField] private float rampDuration = 300f;
    [Tooltip("How the ramp progresses over time. Straight line = steady, curve = slow start then faster.")]
    [SerializeField] private AnimationCurve rampCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

    [Header("Rat Limit")]
    [Tooltip("How many rats can exist at the start.")]
    [SerializeField] private int startMaxRats = 1;
    [Tooltip("Most rats that can exist at once, reached at the end of the ramp.")]
    [SerializeField] private int maxSpawnCount;

    [Header("Time Between Spawns (seconds)")]
    [Tooltip("Gap between rats at the start of the game.")]
    [SerializeField] private Vector2 startSpawnInterval = new Vector2(8f, 12f);
    [Tooltip("Gap between rats at the end of the ramp.")]
    [SerializeField] private Vector2 endSpawnInterval = new Vector2(2f, 4f);
    [Tooltip("How often to check for a free spot when at the rat limit.")]
    [SerializeField] private float limitRecheckDelay = 0.5f;

    [Header("Rat Mesh")]
    [SerializeField] private GameObject ratPrefab;
    [SerializeField] private float minSize;
    [SerializeField] private float maxSize;

    public int ratCount;
    public static RatSpawner Instance;

    private Coroutine spawnRoutine;

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
        // Never run two spawn loops at once
        if (spawnRoutine != null)
            return;

        spawnRoutine = StartCoroutine(SpawnLoop());
    }

    private IEnumerator SpawnLoop()
    {
        float startTime = Time.time;

        while (true)
        {
            float progress = GetProgress(startTime);
            int allowedRats = GetAllowedRats(progress);

            // At the current limit, check again shortly instead of every frame
            if (ratCount >= allowedRats)
            {
                yield return new WaitForSeconds(limitRecheckDelay);
                continue;
            }

            SpawnRat();

            // The gap between rats shrinks as the ramp progresses
            Vector2 interval = Vector2.Lerp(startSpawnInterval, endSpawnInterval, progress);
            yield return new WaitForSeconds(Random.Range(interval.x, interval.y));
        }
    }

    // 0 at the start of the game, 1 once the ramp is complete
    private float GetProgress(float startTime)
    {
        float linear = Mathf.Clamp01((Time.time - startTime) / Mathf.Max(rampDuration, 1f));
        return Mathf.Clamp01(rampCurve.Evaluate(linear));
    }

    // Rat limit for this point in the ramp, never above the overall max
    private int GetAllowedRats(float progress)
    {
        int allowed = Mathf.RoundToInt(Mathf.Lerp(startMaxRats, maxSpawnCount, progress));
        return Mathf.Min(allowed, maxSpawnCount);
    }

    private void SpawnRat()
    {
        if (ratPrefab == null || spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogWarning($"{name}: Missing rat prefab or spawn points, can't spawn.");
            return;
        }

        // Get random spawn point
        Vector3 spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)].position;
        float randomSize = Random.Range(minSize, maxSize);

        GameObject spawnedRat = Instantiate(ratPrefab, spawnPoint, Quaternion.identity);
        spawnedRat.transform.localScale *= randomSize;

        if (playerRef != null && spawnedRat.TryGetComponent(out RatController controller))
        {
            controller.player = playerRef.transform;
        }

        ratCount++;
    }

    private void OnValidate()
    {
        rampDuration = Mathf.Max(rampDuration, 1f);
        limitRecheckDelay = Mathf.Max(limitRecheckDelay, 0.1f);
        maxSpawnCount = Mathf.Max(maxSpawnCount, 0);
        startMaxRats = Mathf.Clamp(startMaxRats, 0, maxSpawnCount);

        // Keep min <= max and never below 0.1s so spawns can't spam every frame
        startSpawnInterval.x = Mathf.Max(startSpawnInterval.x, 0.1f);
        startSpawnInterval.y = Mathf.Max(startSpawnInterval.y, startSpawnInterval.x);
        endSpawnInterval.x = Mathf.Max(endSpawnInterval.x, 0.1f);
        endSpawnInterval.y = Mathf.Max(endSpawnInterval.y, endSpawnInterval.x);
    }
}

// using System.Collections;
// using UnityEngine;
//
// public class RatSpawner : MonoBehaviour
// {
//     // x - min || y - max
//     [Header("Spawn Settings")]
//     [SerializeField] private Vector2 spawnInterval; // Time between wave spawns
//     [SerializeField] private Vector2Int spawnAmount; // Amount of rats in wave
//     [SerializeField] private Vector2 spawnRate; // Time between rat spawns in a wave
//     [SerializeField] private float spawnAmountMultiplier; // Increase of rats
//     [SerializeField] private Transform[] spawnPoints;
//     [SerializeField] private int maxSpawnCount; // Max rats in game
//     [SerializeField] private GameObject playerRef;
//     
//     [Header("Rat Mesh")]
//     [SerializeField] private GameObject ratPrefab;
//     [SerializeField] private float minSize;
//     [SerializeField] private float maxSize;
//
//     public int ratCount;
//     private bool waveSpawned;
//     public static RatSpawner Instance;
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
//     }
//
//     private IEnumerator SpawnRats()
//     {
//         int amountToSpawn = Random.Range(spawnAmount.x, spawnAmount.y);
//         
//         for (int i = 0; i < amountToSpawn; i++)
//         {
//             if (ratCount >= maxSpawnCount)
//                 yield break;
//             
//             // Get random spawn point
//             Vector3 spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)].position;
//             float randomSize = Random.Range(minSize, maxSize);
//             
//             GameObject spawnedRat = Instantiate(ratPrefab, spawnPoint, Quaternion.identity);
//             spawnedRat.transform.localScale *= randomSize;
//             spawnedRat.GetComponent<RatController>().player = playerRef.transform;
//             ratCount++;
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
//             // Skips spawning if there are too many rats
//             if (ratCount >= maxSpawnCount)
//                 continue;
//             
//             waveSpawned = false;
//             
//             // Spawn wave of rats
//             StartCoroutine(SpawnRats());
//             
//             // Wait until all rats have been spawned
//             yield return new WaitUntil(() => waveSpawned);
//             
//             // Wait for interval
//             yield return new WaitForSeconds(Random.Range(spawnInterval.x, spawnInterval.y));
//         }
//     }
// }
