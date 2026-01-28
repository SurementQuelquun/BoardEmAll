using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; // Added for MoveGameObjectToScene

[System.Serializable]
public class TilePrefabEntry
{
    public Tile Prefab;
    public List<Vector2Int> Positions = new List<Vector2Int>();

    public TilePrefabEntry(Tile prefab)
    {
        this.Prefab = prefab;
        this.Positions = new List<Vector2Int>();
    }
}

public class Node
{
    public int x_pos;
    public int z_pos;
    public string nodeType;

    public Node(int x, int z, string type)
    {
        this.x_pos = x;
        this.z_pos = z;
        this.nodeType = type;
    }
    //private bool Equals(Node toCompare)
    //{
    //    if (this.x_pos != toCompare.x_pos) return false;
    //    if (this.z_pos != toCompare.z_pos) return false;
    //    if (!this.nodeType.Equals(toCompare.nodeType)) return false;
    //    return true;
    //}
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

    private Transform _gridParent;
    [SerializeField] private bool _autoGenerate = true;
    public List<Vector2> _startPoints = new List<Vector2>();


    void Start()
    {
        if (_autoGenerate)
        {
            EnsureParent();
            GenerateGraph(_map);
            FillGraphNeighbors(pathGraph, _map);
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
        Color32[] _colors = mapImage.GetPixels32();
        _height = mapImage.height;
        _width = mapImage.width;

        //Debug.Log($"Texture width={mapImage.width}, height={mapImage.height}, format={mapImage.format}");

        int i = 0;
        for(int z = 0; z < _height; z++)
        {
            for(int x = 0; x< _width; x++)
            {
                Color32 _currentTileColor = _colors[i];
                (Tile _currentTile, string _currentTypeOfTile) = GetTileFromColor(_currentTileColor);
                //print(_currentTypeOfTile);
                // add node to graph keys
                if (_currentTile._isNode)
                {
                    Node _currentNode = new Node(x,z,_currentTypeOfTile);
                    List<Node> _currentNodeNeighbors = new List<Node>();
                    pathGraph.Add(_currentNode, _currentNodeNeighbors);
                }

                // add position to correct tileprefabentry
                Vector2Int _pos = new Vector2Int(x,z);
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
                    _startPoints.Add(_pos);
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
                Color32 _upNeighborColor = _mapImage.GetPixel(node_x,node_z+1);
                (Tile _upNeighborTile, string _upNeighborType) = GetTileFromColor(_upNeighborColor);
                if (_upNeighborTile._isNode)
                {
                    Node _upNeighbor = new Node(node_x,node_z+1,_upNeighborType);
                    g.Value.Add(_upNeighbor);
                }

            }
            // Down :
            if(node_z > 0)
            {
                Color32 _downNeighborColor = _mapImage.GetPixel(node_x,node_z-1);
                (Tile _downNeighborTile, string _downNeighborType) = GetTileFromColor(_downNeighborColor);
                if (_downNeighborTile._isNode)
                {
                    Node _downNeighbor = new Node(node_x,node_z-1,_downNeighborType);
                    g.Value.Add(_downNeighbor);
                }
            }
            // Left :
            if(node_x > 0)
            {
                Color32 _leftNeighborColor = _mapImage.GetPixel(node_x-1,node_z);
                (Tile _leftNeighborTile, string _leftNeighborType) = GetTileFromColor(_leftNeighborColor);
                if (_leftNeighborTile._isNode)
                {
                    Node _leftNeighbor = new Node(node_x-1,node_z,_leftNeighborType);
                    g.Value.Add(_leftNeighbor);
                }
            }
            // Right:
            if(node_x < _width -1)
            {
                Color32 _rightNeighborColor = _mapImage.GetPixel(node_x+1,node_z);
                (Tile _rightNeighborTile, string _rightNeighborType) = GetTileFromColor(_rightNeighborColor);
                if (_rightNeighborTile._isNode)
                {
                    Node _rightNeighbor = new Node(node_x+1,node_z,_rightNeighborType);
                    g.Value.Add(_rightNeighbor);
                }
            }

        }
    }

    private (Tile tile, string nodeType) GetTileFromColor(Color32 tileColor)
    {
        //Color32 _color = (Color32)tileColor;


        //Color _nonConstColor = new Color32(229,229,229,255);
        //Color _pathColor = new Color32(255,233,127,255);
        //Color _interColor = new Color32(255,178,127,255);
        //Color _startColor = new Color32(0,255,33,255);
        //Color _endColor = new Color32(255,0,0,255);

        if (tileColor.Equals(new Color32(229,229,229,255)))
        {
            return(_nonConstructibleTilePrefab,"non constructible");
        }
        if (tileColor.Equals(new Color32(255,233,127,255)))
        {
            return(_pathTilePrefab,"path");
        }
        if (tileColor.Equals(new Color32(255,178,127,255)))
        {
            return(_intersectionTilePrefab,"intersection");
        }
        if (tileColor.Equals(new Color32(0,255,33,255)))
        {
            return(_startTilePrefab,"start");
        }
        if (tileColor.Equals(new Color32(255,0,0,255)))
        {
            return(_endTilePrefab,"end");
        }
        return(_defaultTilePrefab, "");
    }
}