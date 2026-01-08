//using System.Collections.Generic;
//using UnityEngine;

//[System.Serializable]
//public class TilePrefabEntry
//{
//    public Tile Prefab;
//    public List<Vector2Int> Positions = new List<Vector2Int>();
//}

//public class GridManager : MonoBehaviour
//{
//    [SerializeField] private int _width, _height;

//    // Default tile used when no entry matches a coordinate
//    [SerializeField] private Tile _defaultTilePrefab;

//    // Configure prefab -> list of coordinates in the Inspector
//    [SerializeField] private List<TilePrefabEntry> _tilePrefabs = new List<TilePrefabEntry>();

//    // Simple folder name for created tiles in the Hierarchy
//    [SerializeField] private string _gridParentName = "Tiles";
//    private Transform _gridParent;

//    // New: control whether grid is generated automatically in Start.
//    // Set false so a bootstrapper (App) can load the scene first and then call Generate().
//    [SerializeField] private bool _autoGenerate = false;

//    void Start()
//    {
//        if (_autoGenerate)
//        {
//            EnsureParent();
//            GenerateGrid();
//        }
//    }

//    // Public method so other code (App) can trigger generation after scene load.
//    public void Generate()
//    {
//        EnsureParent();
//        GenerateGrid();
//    }

//    void EnsureParent()
//    {
//        var existing = GameObject.Find(_gridParentName);
//        if (existing != null)
//        {
//            _gridParent = existing.transform;
//            return;
//        }

//        var go = new GameObject(_gridParentName);
//        go.transform.position = Vector3.zero;
//        _gridParent = go.transform;
//    }

//    void GenerateGrid()
//    {
//        for (int x = 0; x < _width; x++)
//        {
//            for (int z = 0; z < _height; z++)
//            {
//                var coord = new Vector2Int(x, z);
//                Tile prefabToUse = GetPrefabForCoordinate(coord) ?? _defaultTilePrefab;

//                if (prefabToUse == null)
//                {
//                    Debug.LogWarning($"No tile prefab assigned for coordinate {coord} and no default prefab set.");
//                    continue;
//                }

//                // Parent the instantiated tile under the folder so it appears grouped in the Hierarchy.
//                var spawnTile = Instantiate(prefabToUse, new Vector3(x, 0, z), Quaternion.identity, _gridParent);
//                spawnTile.name = $"Tile {x} {z}";

//                var isOffset = (x % 2 == 0 && z % 2 != 0) || (x % 2 != 0 && z % 2 == 0);
//                //spawnTile.Init(isOffset);
//            }
//        }
//    }

//    private Tile GetPrefabForCoordinate(Vector2Int coord)
//    {
//        // Return the first matching prefab for the coordinate (allows overlapping entries; first wins)
//        foreach (var entry in _tilePrefabs)
//        {
//            if (entry == null || entry.Prefab == null || entry.Positions == null)
//                continue;

//            // Positions list can be edited in the Inspector: add Vector2Int entries for coordinates.
//            if (entry.Positions.Contains(coord))
//                return entry.Prefab;
//        }

//        return null;
//    }
//}


using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; // Added for MoveGameObjectToScene

[System.Serializable]
public class TilePrefabEntry
{
    public Tile Prefab;
    public List<Vector2Int> Positions = new List<Vector2Int>();
}

public class GridManager : MonoBehaviour
{
    [SerializeField] private int _width, _height;
    [SerializeField] private Tile _defaultTilePrefab;
    [SerializeField] private List<TilePrefabEntry> _tilePrefabs = new List<TilePrefabEntry>();
    [SerializeField] private string _gridParentName = "Tiles";

    private Transform _gridParent;
    [SerializeField] private bool _autoGenerate = false;

    void Start()
    {
        if (_autoGenerate)
        {
            EnsureParent();
            GenerateGrid();
        }
    }

    public void Generate()
    {
        EnsureParent();
        GenerateGrid();
    }

    void EnsureParent()
    {
        // If we already have the reference, we are good
        if (_gridParent != null) return;

        // Create the holder GameObject
        var go = new GameObject(_gridParentName);
        go.transform.position = Vector3.zero;

        // IMPORTANT: Force this new object into the same scene as this GridManager script
        // This ensures it doesn't accidentally end up in "App" or "DontDestroyOnLoad"
        SceneManager.MoveGameObjectToScene(go, this.gameObject.scene);

        _gridParent = go.transform;
    }

    void GenerateGrid()
    {
        // Clear old children if regenerating (optional safety)
        if (_gridParent.childCount > 0)
        {
            foreach (Transform child in _gridParent)
                Destroy(child.gameObject);
        }

        for (int x = 0; x < _width; x++)
        {
            for (int z = 0; z < _height; z++)
            {
                var coord = new Vector2Int(x, z);
                Tile prefabToUse = GetPrefabForCoordinate(coord) ?? _defaultTilePrefab;

                if (prefabToUse == null) continue;

                var spawnTile = Instantiate(prefabToUse, new Vector3(x, 0, z), Quaternion.identity, _gridParent);
                spawnTile.name = $"Tile {x} {z}";

                // Ensure the spawned tile is also in the correct scene (should happen automatically via parent, but good for safety)
                // spawnTile.transform.SetParent(_gridParent); 
            }
        }
    }

    private Tile GetPrefabForCoordinate(Vector2Int coord)
    {
        foreach (var entry in _tilePrefabs)
        {
            if (entry == null || entry.Prefab == null || entry.Positions == null) continue;
            if (entry.Positions.Contains(coord)) return entry.Prefab;
        }
        return null;
    }
}