//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//[System.Serializable]
//public class Wave
//{
//    // counts[i] = how many enemies of monsterPrefabs[i] to spawn in this wave
//    public List<int> counts = new List<int>();

//    [Tooltip("Optional: duration in seconds for this wave. If > 0 the spawner will progress to the next wave after this time regardless of remaining monsters. If <= 0 the spawner will wait until all spawned monsters are destroyed (legacy behavior).")]
//    public float duration = 10f;
//}

//public class MonsterSpawner : MonoBehaviour
//{
//    [Header("Prefabs")]
//    [Tooltip("Preferred: list of different monster prefabs. Counts per wave are defined in Waves.")]
//    public List<GameObject> monsterPrefabs = new List<GameObject>();

//    [Header("Spawn")]
//    private bool useStartTiles = true;

//    [Header("Waves")]
//    [Tooltip("Configure waves. Each Wave.counts must have an entry per prefab in monsterPrefabs (or fewer — missing entries treated as 0).")]
//    public List<Wave> waves = new List<Wave>();

//    public float spawnSpacing = 0.4f;

//    [Header("Timing")]
//    public float spawnDelayBetweenMonsters = 0.2f;
//    public float delayBetweenWaves = 2f;
//    [Tooltip("If true, waves will loop when finished.")]
//    public bool loopWaves = false;

//    [Header("Initial Delay")]
//    [Tooltip("Time in seconds to wait after the scene/game is loaded before the first wave starts.")]
//    [SerializeField] private float _initialSpawnDelay = 5f;

//    private Coroutine _spawnRoutine;
//    private List<Vector3> _spawnLocations = new List<Vector3>();

//    private void Start()
//    {
//        CollectSpawnLocations();
//        StartSpawning();
//    }

//    // Finds all Tiles flagged as Start and caches their world positions.
//    private void CollectSpawnLocations()
//    {
//        _spawnLocations.Clear();
//        var tiles = FindObjectsOfType<Tile>();
//        foreach (var t in tiles)
//        {
//            if (t != null && t.IsStart)
//            {
//                var p = t.transform.position;
//                // snap to integer tile center just in case
//                _spawnLocations.Add(new Vector3(Mathf.Round(p.x), p.y, Mathf.Round(p.z)));
//            }
//        }
//    }

//    public void StartSpawning()
//    {
//        if (_spawnRoutine == null)
//            _spawnRoutine = StartCoroutine(SpawnWavesWithInitialDelay());
//    }

//    public void StopSpawning()
//    {
//        if (_spawnRoutine != null)
//        {
//            StopCoroutine(_spawnRoutine);
//            _spawnRoutine = null;
//        }
//    }

//    // Wrapper coroutine that waits the initial delay then runs the wave spawner.
//    private IEnumerator SpawnWavesWithInitialDelay()
//    {
//        if (_initialSpawnDelay > 0f)
//            yield return new WaitForSeconds(_initialSpawnDelay);

//        yield return StartCoroutine(SpawnWaves());

//        // Clear the routine handle when the spawning sequence finishes (if not looping).
//        _spawnRoutine = null;
//    }

//    private IEnumerator SpawnWaves()
//    {
//        int waveIndex = 0;
//        int spawnLocationsCount = Mathf.Max(1, _spawnLocations.Count);

//        do
//        {
//            if (waveIndex >= waves.Count) break;
//            Wave currentWave = waves[waveIndex];

//            List<GameObject> spawned = new List<GameObject>();

//            // spawn each prefab the requested number of times
//            int spawnCounter = 0;
//            for (int prefabIndex = 0; prefabIndex < monsterPrefabs.Count; prefabIndex++)
//            {
//                int countForPrefab = 0;
//                if (currentWave.counts != null && prefabIndex < currentWave.counts.Count)
//                    countForPrefab = Mathf.Max(0, currentWave.counts[prefabIndex]);

//                for (int c = 0; c < countForPrefab; c++)
//                {
//                    Vector3 basePos = _spawnLocations[spawnCounter % spawnLocationsCount];

//                    // small random spread to avoid overlapping exactly
//                    Vector3 offset = new Vector3(
//                        Random.Range(-spawnSpacing, spawnSpacing),
//                        0f,
//                        Random.Range(-spawnSpacing, spawnSpacing)
//                    );
//                    Vector3 spawnPos = basePos + offset;

//                    GameObject prefab = monsterPrefabs[prefabIndex];
//                    GameObject go = Instantiate(prefab, spawnPos, Quaternion.identity, GetMonstersParent());
//                    spawned.Add(go);

//                    spawnCounter++;

//                    if (spawnDelayBetweenMonsters > 0f)
//                        yield return new WaitForSeconds(spawnDelayBetweenMonsters);
//                    else
//                        yield return null;
//                }
//            }

//            // NEW: If the wave defines a positive duration, wait that amount of time
//            // and then progress to the next wave regardless of whether spawned monsters are still alive.
//            //if (currentWave.duration > 0f)
//            //{
//            float timer = 0f;
//            while (timer < currentWave.duration)
//            {
//                timer += Time.deltaTime;
//                yield return null;
//            }
//            //}

//            // Optional gap between waves
//            if (delayBetweenWaves > 0f)
//                yield return new WaitForSeconds(delayBetweenWaves);

//            waveIndex++;
//            if (waveIndex >= waves.Count && loopWaves && waves.Count > 0)
//            {
//                waveIndex = 0;
//            }
//        } while (loopWaves || (_spawnRoutine != null && (waveIndex < waves.Count)));

//        _spawnRoutine = null;
//    }

//    private Transform GetMonstersParent()
//    {
//        GameObject parent = GameObject.Find("Monsters");
//        if (parent == null) parent = new GameObject("Monsters");
//        return parent.transform;
//    }
//}


using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Wave
{
    public List<int> counts = new List<int>();

    [Tooltip("Optional: duration in seconds for this wave.")]
    public float duration = 10f;
}

public class MonsterSpawner : MonoBehaviour
{
    [Header("Prefabs")]
    public List<GameObject> monsterPrefabs = new List<GameObject>();

    [Header("Waves")]
    public List<Wave> waves = new List<Wave>();

    public float spawnSpacing = 0.4f;

    [Header("Timing")]
    public float spawnDelayBetweenMonsters = 0.2f;
    public float delayBetweenWaves = 2f;
    public bool loopWaves = false;

    [Header("Initial Delay")]
    [Tooltip("Time in seconds to wait after the scene/game is loaded before the first wave starts.")]
    [SerializeField] private float _initialSpawnDelay = 5f;

    private Coroutine _spawnRoutine;
    private List<Vector3> _spawnLocations = new List<Vector3>();

    private void Start()
    {
        // We do NOT call CollectSpawnLocations here anymore.
        // We let the coroutine handle it so it can wait for the map.
        StartSpawning();
    }

    private void CollectSpawnLocations()
    {
        _spawnLocations.Clear();
        var tiles = FindObjectsOfType<Tile>();
        foreach (var t in tiles)
        {
            if (t != null && t.IsStart)
            {
                var p = t.transform.position;
                _spawnLocations.Add(new Vector3(Mathf.Round(p.x), p.y, Mathf.Round(p.z)));
            }
        }
    }

    public void StartSpawning()
    {
        if (_spawnRoutine == null)
            _spawnRoutine = StartCoroutine(SpawnWavesWithInitialDelay());
    }

    public void StopSpawning()
    {
        if (_spawnRoutine != null)
        {
            StopCoroutine(_spawnRoutine);
            _spawnRoutine = null;
        }
    }

    // --- THIS IS THE KEY FIX ---
    private IEnumerator SpawnWavesWithInitialDelay()
    {
        // 1. Wait the initial delay defined in the inspector
        if (_initialSpawnDelay > 0f)
            yield return new WaitForSeconds(_initialSpawnDelay);

        // 2. WAIT FOR MAP TO LOAD
        // We attempt to find spawn locations. If the list is empty, 
        // we wait a frame and try again until we find them.
        CollectSpawnLocations();

        while (_spawnLocations.Count == 0)
        {
            // Optional: Log a warning if you want to know it's waiting
            // Debug.Log("Spawner waiting for map tiles...");

            yield return null; // Wait for the next frame
            CollectSpawnLocations(); // Try finding tiles again
        }

        // 3. Map found, now we can safely start spawning
        yield return StartCoroutine(SpawnWaves());

        _spawnRoutine = null;
    }

    private IEnumerator SpawnWaves()
    {
        int waveIndex = 0;
        // Safety check to ensure we don't divide by zero if something went wrong
        int spawnLocationsCount = Mathf.Max(1, _spawnLocations.Count);

        do
        {
            if (waveIndex >= waves.Count) break;
            Wave currentWave = waves[waveIndex];

            // spawn each prefab the requested number of times
            int spawnCounter = 0;
            for (int prefabIndex = 0; prefabIndex < monsterPrefabs.Count; prefabIndex++)
            {
                int countForPrefab = 0;
                if (currentWave.counts != null && prefabIndex < currentWave.counts.Count)
                    countForPrefab = Mathf.Max(0, currentWave.counts[prefabIndex]);

                for (int c = 0; c < countForPrefab; c++)
                {
                    // FIX: This now relies on _spawnLocations being populated by the wait loop above
                    Vector3 basePos = _spawnLocations[spawnCounter % spawnLocationsCount];

                    Vector3 offset = new Vector3(
                        Random.Range(-spawnSpacing, spawnSpacing),
                        0f,
                        Random.Range(-spawnSpacing, spawnSpacing)
                    );
                    Vector3 spawnPos = basePos + offset;

                    GameObject prefab = monsterPrefabs[prefabIndex];
                    if (prefab != null)
                    {
                        Instantiate(prefab, spawnPos, Quaternion.identity, GetMonstersParent());
                    }

                    spawnCounter++;

                    if (spawnDelayBetweenMonsters > 0f)
                        yield return new WaitForSeconds(spawnDelayBetweenMonsters);
                    else
                        yield return null;
                }
            }

            float timer = 0f;
            while (timer < currentWave.duration)
            {
                timer += Time.deltaTime;
                yield return null;
            }

            if (delayBetweenWaves > 0f)
                yield return new WaitForSeconds(delayBetweenWaves);

            waveIndex++;
            if (waveIndex >= waves.Count && loopWaves && waves.Count > 0)
            {
                waveIndex = 0;
            }
        } while (loopWaves || (_spawnRoutine != null && (waveIndex < waves.Count)));

        _spawnRoutine = null;
    }

    private Transform GetMonstersParent()
    {
        GameObject parent = GameObject.Find("Monsters");
        if (parent == null) parent = new GameObject("Monsters");
        return parent.transform;
    }
}