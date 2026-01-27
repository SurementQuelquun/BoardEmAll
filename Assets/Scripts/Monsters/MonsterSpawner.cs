using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterSpawner : MonoBehaviour
{
    [Header("Prefab & Spawn")]
    public GameObject monsterPrefab;
    //public Transform spawnPoint; // optional fallback when no start tiles found
    [Tooltip("If true, automatically create spawn locations from Tiles marked as Start.")]
    public bool useStartTiles = true;

    [Header("Waves")]
    public List<int> waves = new List<int> { 6, 8, 10 };
    public float spawnSpacing = 0.4f;

    [Header("Timing")]
    public float spawnDelayBetweenMonsters = 0.2f;
    public float delayBetweenWaves = 2f;
    [Tooltip("If true, spawning begins automatically on Start.")]
    public bool startOnAwake = true;
    [Tooltip("If true, waves will loop when finished.")]
    public bool loopWaves = false;

    [Header("Initial Delay")]
    [Tooltip("Time in seconds to wait after the scene/game is loaded before the first wave starts.")]
    [SerializeField] private float _initialSpawnDelay = 5f;

    private Coroutine _spawnRoutine;
    private List<Vector3> _spawnLocations = new List<Vector3>();

    [System.Obsolete]
    void Start()
    {
        CollectSpawnLocations();
        if (startOnAwake) StartSpawning();
    }

    // Finds all Tiles flagged as Start and caches their world positions.
    [System.Obsolete]
    private void CollectSpawnLocations()
    {
        _spawnLocations.Clear();

        if (useStartTiles)
        {
            var tiles = FindObjectsOfType<Tile>();
            foreach (var t in tiles)
            {
                if (t != null && t.IsStart)
                {
                    var p = t.transform.position;
                    // snap to integer tile center just in case
                    _spawnLocations.Add(new Vector3(Mathf.Round(p.x), p.y, Mathf.Round(p.z)));
                }
            }
        }

        // If still empty, use this object's position as last fallback
        if (_spawnLocations.Count == 0)
        {
            _spawnLocations.Add(transform.position);
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

    // Wrapper coroutine that waits the initial delay then runs the wave spawner.
    private IEnumerator SpawnWavesWithInitialDelay()
    {
        if (_initialSpawnDelay > 0f)
            yield return new WaitForSeconds(_initialSpawnDelay);

        yield return StartCoroutine(SpawnWaves());

        // Clear the routine handle when the spawning sequence finishes (if not looping).
        _spawnRoutine = null;
    }

    private IEnumerator SpawnWaves()
    {
        int waveIndex = 0;
        int spawnLocationsCount = Mathf.Max(1, _spawnLocations.Count);

        do
        {
            if (waveIndex >= waves.Count) break;
            int count = waves[waveIndex];

            List<GameObject> spawned = new List<GameObject>(count);

            for (int i = 0; i < count; i++)
            {
                // cycle through spawn locations so each start tile acts as a spawner
                Vector3 basePos = _spawnLocations[i % spawnLocationsCount];

                // small random spread to avoid overlapping exactly
                Vector3 offset = new Vector3(
                    Random.Range(-spawnSpacing, spawnSpacing),
                    0f,
                    Random.Range(-spawnSpacing, spawnSpacing)
                );
                Vector3 spawnPos = basePos + offset;

                GameObject go = Instantiate(monsterPrefab, spawnPos, Quaternion.identity, GetMonstersParent());
                spawned.Add(go);

                if (spawnDelayBetweenMonsters > 0f)
                    yield return new WaitForSeconds(spawnDelayBetweenMonsters);
                else
                    yield return null;
            }

            // Wait until all spawned monsters are destroyed
            while (true)
            {
                for (int i = spawned.Count - 1; i >= 0; i--)
                {
                    if (spawned[i] == null) spawned.RemoveAt(i);
                }

                if (spawned.Count == 0) break;
                yield return new WaitForSeconds(0.25f);
            }

            yield return new WaitForSeconds(delayBetweenWaves);

            waveIndex++;
            if (waveIndex >= waves.Count && loopWaves)
            {
                waveIndex = 0;
            }
        } while (loopWaves || (_spawnRoutine != null && waveIndex < waves.Count));

        _spawnRoutine = null;
    }

    private Transform GetMonstersParent()
    {
        GameObject parent = GameObject.Find("Monsters");
        if (parent == null) parent = new GameObject("Monsters");
        return parent.transform;
    }
}