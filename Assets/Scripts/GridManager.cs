using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TilePrefabEntry
{
    public Tile Prefab;
    public List<Vector2Int> Positions = new List<Vector2Int>();
}

public class GridManager : MonoBehaviour
{
    [SerializeField] private int _width, _height;

    // Default tile used when no entry matches a coordinate
    [SerializeField] private Tile _defaultTilePrefab;

    // Configure prefab -> list of coordinates in the Inspector
    [SerializeField] private List<TilePrefabEntry> _tilePrefabs = new List<TilePrefabEntry>();

    // Simple folder name for created tiles in the Hierarchy
    [SerializeField] private string _gridParentName = "Tiles";
    private Transform _gridParent;

    void Start()
    {
        EnsureParent();
        GenerateGrid();
    }

    void EnsureParent()
    {
        var existing = GameObject.Find(_gridParentName);
        if (existing != null)
        {
            _gridParent = existing.transform;
            return;
        }

        var go = new GameObject(_gridParentName);
        go.transform.position = Vector3.zero;
        _gridParent = go.transform;
    }

    void GenerateGrid()
    {
        for (int x = 0; x < _width; x++)
        {
            for (int z = 0; z < _height; z++)
            {
                var coord = new Vector2Int(x, z);
                Tile prefabToUse = GetPrefabForCoordinate(coord) ?? _defaultTilePrefab;

                if (prefabToUse == null)
                {
                    Debug.LogWarning($"No tile prefab assigned for coordinate {coord} and no default prefab set.");
                    continue;
                }

                // Parent the instantiated tile under the folder so it appears grouped in the Hierarchy.
                var spawnTile = Instantiate(prefabToUse, new Vector3(x, 0, z), Quaternion.identity, _gridParent);
                spawnTile.name = $"Tile {x} {z}";

                var isOffset = (x % 2 == 0 && z % 2 != 0) || (x % 2 != 0 && z % 2 == 0);
                //spawnTile.Init(isOffset);
            }
        }
    }

    private Tile GetPrefabForCoordinate(Vector2Int coord)
    {
        // Return the first matching prefab for the coordinate (allows overlapping entries; first wins)
        foreach (var entry in _tilePrefabs)
        {
            if (entry == null || entry.Prefab == null || entry.Positions == null)
                continue;

            // Positions list can be edited in the Inspector: add Vector2Int entries for coordinates.
            if (entry.Positions.Contains(coord))
                return entry.Prefab;
        }

        return null;
    }
}
