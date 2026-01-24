using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; // Added for MoveGameObjectToScene

[System.Serializable]
public class TilePrefabEntry
{
    public Tile Prefab;
    public List<Vector2Int> Positions = new List<Vector2Int>();
}

public class Node
{
    public int x;
    public int z;
    public string nodeType;

    public Node(int x_pos, int z_pos, string typeOfNode)
    {
        x = x_pos;
        z = z_pos;
        nodeType = typeOfNode;
    }
}



public class GridManager : MonoBehaviour
{
    [SerializeField] private int _width, _height;

    // Tiles for automatic generation
    [SerializeField] private Tile nonConstructibleTile;
    [SerializeField] private Tile pathTile;
    [SerializeField] private Tile intersectionTile;
    [SerializeField] private Tile startTile;
    [SerializeField] private Tile endTile;
    [SerializeField] private Tile constructibleTile;

    public Dictionary<Node,Node[]> pathGraph;

    // Manual generation
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

        private void GenerateGridFromMap(Texture2D mapImage)
    {
        Color[] colors = mapImage.GetPixels();
        int mapWidth = mapImage.width;
        int mapHeight = mapImage.height;

        int i = 0;
        for (int x = 0; x < mapWidth; x++)
        {
            for (int z = 0; z< mapHeight; z++)
            {
                Color tileColor = colors[i];
                (Tile tileToUse, string typeIfNode) = MakeTileFromColor(tileColor);
                if (!string.IsNullOrEmpty(typeIfNode))
                {
                    Node nodeToAdd = new Node(x,z,typeIfNode);
                    AddNodeToGraph(nodeToAdd);
                }
                var spawnTile = Instantiate(tileToUse, new Vector3(x, 0, z), Quaternion.identity, _gridParent);
                spawnTile.name = $"Tile {x} {z}";
                i++;
            }
        }

    }

    private void AddNodeToGraph(Node node)
    {
        //Each node has array length 4, with neigbors up, down, left, right, in that order
        this.pathGraph[node] = new Node[4];
        //TBD
    }

    private (Tile tile, string tiletype) MakeTileFromColor(Color tileColor)
    {
        Color nonConstColor = new Color32(229,229,229,255);
        Color pathColor = new Color32(255,233,127,255);
        Color interColor = new Color32(255,178,127,255);
        Color startColor = new Color32(0,255,33,255);
        Color endColor = new Color32(255,0,0,255);

        if (tileColor.Equals(nonConstColor))
        {
            return(nonConstructibleTile,"");
        }
        if (tileColor.Equals(pathColor))
        {
            return(pathTile,"path");
        }
        if (tileColor.Equals(interColor))
        {
            return(intersectionTile,"intersection");
        }
        if (tileColor.Equals(startColor))
        {
            return(startTile,"start");
        }
        if (tileColor.Equals(endColor))
        {
            return(endTile,"end");
        }
        return(constructibleTile, "");
    }
}

