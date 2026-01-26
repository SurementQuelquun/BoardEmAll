using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterSpawner : MonoBehaviour
{
    [Header("Prefab & Spawn")]
    public GameObject monsterPrefab;     // assign Monster/skeleton prefab
    public Transform spawnPoint;         // spawn position (optional)

    [Header("Waves")]
    public List<int> waves = new List<int> { 6, 8, 10 }; // example: wave1=6, wave2=8, ...
    public float spawnSpacing = 0.4f;    // random spread around spawn point

    [Header("Timing")]
    public float spawnDelayBetweenMonsters = 0.2f; // small delay between individual spawns
    public float delayBetweenWaves = 2f;          // delay after a wave finishes before next wave starts
    public bool startOnAwake = true;
    public bool loopWaves = false;                 // repeat waves after last

    private Coroutine _spawnRoutine;

    void Start()
    {
        if (startOnAwake) StartSpawning();
    }

    public void StartSpawning()
    {
        if (_spawnRoutine == null) _spawnRoutine = StartCoroutine(SpawnWaves());
    }

    public void StopSpawning()
    {
        if (_spawnRoutine != null)
        {
            StopCoroutine(_spawnRoutine);
            _spawnRoutine = null;
        }
    }

    private IEnumerator SpawnWaves()
    {
        int waveIndex = 0;
        do
        {
            if (waveIndex >= waves.Count) break;
            int count = waves[waveIndex];

            // Spawn this wave
            List<GameObject> spawned = new List<GameObject>(count);
            for (int i = 0; i < count; i++)
            {
                Vector3 basePos = spawnPoint != null ? spawnPoint.position : transform.position;
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

            // Wait until all spawned monsters are dead (destroyed)
            while (true)
            {
                bool anyAlive = false;
                for (int i = spawned.Count - 1; i >= 0; i--)
                {
                    if (spawned[i] == null)
                    {
                        // already destroyed, remove from list
                        spawned.RemoveAt(i);
                    }
                    else
                    {
                        anyAlive = true;
                    }
                }

                if (!anyAlive) break;
                yield return new WaitForSeconds(0.25f);
            }

            // Wave finished
            yield return new WaitForSeconds(delayBetweenWaves);

            waveIndex++;
            if (waveIndex >= waves.Count && loopWaves)
            {
                waveIndex = 0;
            }
        } while (loopWaves || _spawnRoutine != null && waveIndex < waves.Count);

        _spawnRoutine = null;
    }

    private Transform GetMonstersParent()
    {
        GameObject parent = GameObject.Find("Monsters");
        if (parent == null) parent = new GameObject("Monsters");
        return parent.transform;
    }
}