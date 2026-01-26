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
    public int x_pos;
    public int z_pos;
    public string nodeType;

    private bool Equals(Node toCompare)
    {
        if (this.x_pos != toCompare.x_pos) return false;
        if (this.z_pos != toCompare.z_pos) return false;
        if (!this.nodeType.Equals(toCompare.nodeType)) return false;
        return true;
    }
}

public class GridManager : MonoBehaviour
{
    [SerializeField] private Texture2D _map;

    [SerializeField] private int _width, _height;

    [SerializeField] private Tile _nonConstructibleTilePrefab;
    [SerializeField] private Tile _startTilePrefab;
    [SerializeField] private Tile _pathTilePrefab;
    [SerializeField] private Tile _intersectionTilePrefab;
    [SerializeField] private Tile _endTilePrefab;
    [SerializeField] private Tile _defaultTilePrefab;

    [SerializeField] private List<TilePrefabEntry> _tilePrefabs = new List<TilePrefabEntry>();
    [SerializeField] private string _gridParentName = "Tiles";

    public Dictionary<Node,List<Node>> pathGraph;
    [SerializeField] public Texture2D mapImage;

    private Transform _gridParent;
    [SerializeField] private bool _autoGenerate = false;

    void Start()
    {
        if (_autoGenerate)
        {
            EnsureParent();
            //GenerateGraph(_map);
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

    private void GenerateGraph(Texture2D mapImage)
    {
        // Create TilePrefabEntry for each Tile type (Prefab, Positions)
        TilePrefabEntry _nonConstrTileEntry = new TilePrefabEntry(_nonConstructibleTilePrefab);
        TilePrefabEntry _startTileEntry = new TilePrefabEntry(_startTilePrefab);
        TilePrefabEntry _pathTileEntry = new TilePrefabEntry(_pathTilePrefab);
        TilePrefabEntry _intersectTileEntry = new TilePrefabEntry(_intersectionTilePrefab);
        TilePrefabEntry _endTileEntry = new TilePrefabEntry(_endTilePrefab);

        // Create empty graph
        pathGraph = new Dictionary<Node, List<Node>>();

        //Read map
        Color[] _colors = mapImage.GetPixels();
        _height = mapImage.height;
        _width = mapImage.width;

        int i = 0;
        for(int x = 0; x < _width; x++)
        {
            for(int z = 0; z< _height; z++)
            {
                Color _currentTileColor = _colors[i];
                (Tile _currentTile, string _currentTypeOfTile) = GetTileFromColor(_currentTileColor);
                // add node to graph keys
                if (_currentTile._isNode)
                {
                    Node _currentNode = new Node(x,z,_currentTypeOfTile);
                    List<Node> _currentNodeNeighbors = new List<Node>();
                    pathGraph.Add(_currentNode, _currentNodeNeighbors);
                }

                // add position to correct tileprefabentry
                Vector2Int _pos = Vector2Int(x,z);
                if (_currentTypeOfTile.Equals("non constructible"))
                {
                    _nonConstrTileEntry.Positions.Add(_pos);
                }
                if (_currentTypeOfTile.Equals("path"))
                {
                    _pathTileEntry.Positions.Add(_pos);
                }
                if (_currentTypeOfTile.Equals("intersection"))
                {
                    _intersectTileEntry.Positions.Add(_pos);
                }
                if (_currentTypeOfTile.Equals("start"))
                {
                    _startTileEntry.Positions.Add(_pos);
                }
                if (_currentTypeOfTile.Equals("end"))
                {
                    _endTileEntry.Positions.Add(_pos);
                }

                i++;
            }
        }

        // Create List of these TilePrefabEntry (_tilePrefabs)
        _tilePrefabs = new List<TilePrefabEntry>();
        _tilePrefabs.Add(_nonConstrTileEntry);
        _tilePrefabs.Add(_startTileEntry);
        _tilePrefabs.Add(_pathTileEntry);
        _tilePrefabs.Add(_intersectTileEntry);
        _tilePrefabs.Add(_endTileEntry);


    }

    private void FillGraphNeighbors(Dictionary<Node,List<Node>> _graph, Texture2D _mapImage)
    {
        foreach(KeyValuePair<Node,List<Node>> g in _graph)
        {
            // Get key position
            int node_x = g.Key.x_pos;
            int node_z = g.Key.z_pos;
            // For possible positions, see if neighbors are nodes, If so, add them to list for that key
            // Up :
            if(node_z < _height -1)
            {
                Color _upNeighborColor = _mapImage.GetPixel(node_x,node_z+1);
                (Tile _upNeighborTile, string _upNeighborType) = MakeTileFromColor(_upNeighborColor);
                if (_upNeighborTile._isNode)
                {
                    Node _upNeighbor = newNode(node_x,node_z+1,_upNeighborType);
                    g.Value.Add(_upNeighbor);
                }

            }
            // Down :
            if(node_z > 0)
            {
                Color _downNeighborColor = _mapImage.GetPixel(node_x,node_z-1);
                (Tile _downNeighborTile, string _downNeighborType) = MakeTileFromColor(_downNeighborColor);
                if (_downNeighborTile._isNode)
                {
                    Node _downNeighbor = newNode(node_x,node_z-1,_downNeighborType);
                    g.Value.Add(_downNeighbor);
                }
            }
            // Left :
            if(node_x > 0)
            {
                Color _leftNeighborColor = _mapImage.GetPixel(node_x-1,node_z);
                (Tile _leftNeighborTile, string _leftNeighborType) = MakeTileFromColor(_leftNeighborColor);
                if (_leftNeighborTile._isNode)
                    Node _leftNeighbor = newNode(node_x-1,node_z,_leftNeighborType);
                    g.Value.Add(_leftNeighbor);
            }
            // Right:
            if(node_x < _width -1)
            {
                Color _rightNeighborColor = _mapImage.GetPixel(node_x+1,node_z);
                (Tile _rightNeighborTile, string _rightNeighborType) = MakeTileFromColor(_rightNeighborColor);
                if (_rightNeighborTile._isNode)
                {
                    Node _rightNeighbor = newNode(node_x+1,node_z,_neighborType);
                    g.Value.Add(_rightNeighbor);
                }
            }

        }
    }

    private (Tile tile, string nodeType) GetTileFromColor(Color color)
    {
        Color _nonConstColor = new Color32(229,229,229,255);
        Color _pathColor = new Color32(255,233,127,255);
        Color _interColor = new Color32(255,178,127,255);
        Color _startColor = new Color32(0,255,33,255);
        Color _endColor = new Color32(255,0,0,255);

        if (tileColor.Equals(_nonConstColor))
        {
            return(nonConstructibleTile,"non contructible");
        }
        if (tileColor.Equals(_pathColor))
        {
            return(pathTile,"path");
        }
        if (tileColor.Equals(_interColor))
        {
            return(intersectionTile,"intersection");
        }
        if (tileColor.Equals(_startColor))
        {
            return(startTile,"start");
        }
        if (tileColor.Equals(_endColor))
        {
            return(endTile,"end");
        }
        return(constructibleTile, "");
    }
}