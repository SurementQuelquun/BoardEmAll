using System.Collections.Generic;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
using UnityEngine.InputSystem;
#endif

public class Tower : MonoBehaviour
{
    [Header("Tower Prefabs")]
    public GameObject flyingFishPrefab; // ID: 0
    public GameObject krakenPrefab;     // ID: 1
    public GameObject seaUrchinPrefab;  // ID: 2
    public GameObject sharkPrefab;      // ID: 3

    [Header("Settings")]
    public float gridsize = 1f;

    // Internal variable for the tower we are CURRENTLY building
    private GameObject currentObjectToPlace;

    // Ghost Management
    private static GameObject s_GhostObject;
    private static GameObject s_GhostPrefab;
    private static Tower s_GhostOwner;

    // Grid Tracking
    private static HashSet<Vector3Int> s_OccupiedPositions = new HashSet<Vector3Int>();
    private static int s_LastPlacementFrame = -1;

    private void Start()
    {
        // Optional: Start with no tower selected
        currentObjectToPlace = null;
    }

    private void Update()
    {
        // 1. Manage the Ghost
        CreateGhostObjectIfNeeded();

        if (s_GhostOwner != this) return;

        // 2. Move Ghost
        UpdateGhostPosition();

        // 3. Place on Click
        if (WasLeftMousePressedThisFrame())
        {

            PlaceObject();
        }
    }

    // --- NEW SELECTION FUNCTION ---
    // 0 = Fish, 1 = Kraken, 2 = Urchin, 3 = Shark
    public void SelectTowerByID(int towerID)
    {
        switch (towerID)
        {
            case 0: currentObjectToPlace = flyingFishPrefab; break;
            case 1: currentObjectToPlace = krakenPrefab; break;
            case 2: currentObjectToPlace = seaUrchinPrefab; break;
            case 3: currentObjectToPlace = sharkPrefab; break;
            default: currentObjectToPlace = null; break;
        }

        // Reset the ghost immediately to show the new tower
        DestroyGhost();
    }

    // --- GHOST & PLACEMENT LOGIC ---

    void CreateGhostObjectIfNeeded()
    {
        // If we have nothing selected, destroy any existing ghost and return
        if (currentObjectToPlace == null)
        {
            DestroyGhost();
            return;
        }

        // If we already have the correct ghost, do nothing
        if (s_GhostObject != null && s_GhostPrefab == currentObjectToPlace)
        {
            if (s_GhostOwner == null) s_GhostOwner = this;
            return;
        }

        // If ghost is wrong type, destroy it
        DestroyGhost();

        // Create new ghost
        s_GhostObject = Instantiate(currentObjectToPlace);
        s_GhostPrefab = currentObjectToPlace;
        s_GhostOwner = this;

        // Make it transparent and disable collider
        var col = s_GhostObject.GetComponent<Collider>();
        if (col != null) col.enabled = false;

        Renderer[] renderers = s_GhostObject.GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            Material material = renderer.material;
            // Setup transparency
            material.SetFloat("_Mode", 2);
            material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            material.SetInt("_ZWrite", 0);
            material.DisableKeyword("_ALPHATEST_ON");
            material.EnableKeyword("_ALPHABLEND_ON");
            material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            material.renderQueue = 3000;

            Color color = material.color;
            color.a = 0.5f;
            material.color = color;
        }
    }

    void DestroyGhost()
    {
        if (s_GhostObject != null)
        {
            Destroy(s_GhostObject);
            s_GhostObject = null;
            s_GhostPrefab = null;
            s_GhostOwner = null;
        }
    }

    void UpdateGhostPosition()
    {
        if (s_GhostObject == null) return;

        Vector2 mouseScreenPos = GetMouseScreenPosition();
        Ray ray = Camera.main.ScreenPointToRay(new Vector3(mouseScreenPos.x, mouseScreenPos.y, 0f));

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Vector3Int gridPos = WorldToGridPosition(hit.point);
            Vector3 snappedPosition = (Vector3)gridPos * gridsize;
            s_GhostObject.transform.position = snappedPosition;

            // Red if occupied, standard if free
            if (s_OccupiedPositions.Contains(gridPos))
                SetGhostColor(Color.red);
            else
                SetGhostColor(new Color(1f, 1f, 1f, 0.5f));
        }
    }

    void PlaceObject()
    {
        if (s_GhostObject == null || s_GhostPrefab == null) return;
        if (Time.frameCount == s_LastPlacementFrame) return;

        Vector3 origin = s_GhostObject.transform.position + Vector3.up * 0.5f;
        if (!Physics.Raycast(origin, Vector3.down, out RaycastHit hit, 1f)) return;

        // Check if Constructible (Your existing logic)
        var hitTile = hit.collider.GetComponentInParent<Tile>();
        bool canPlace = true;
        if (hitTile != null)
        {
            // Reflection fallback for "IsConstructible"
            var type = hitTile.GetType();
            var prop = type.GetProperty("IsConstructible");
            if (prop != null && prop.PropertyType == typeof(bool)) canPlace = (bool)prop.GetValue(hitTile);
            else
            {
                var field = type.GetField("_isConstructible", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                            ?? type.GetField("isConstructible", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
                            ?? type.GetField("IsConstructible", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                if (field != null && field.FieldType == typeof(bool)) canPlace = (bool)field.GetValue(hitTile);
            }
        }

        if (!canPlace) return;

        Vector3Int gridPos = WorldToGridPosition(s_GhostObject.transform.position);

        if (!s_OccupiedPositions.Contains(gridPos))
        {
            s_LastPlacementFrame = Time.frameCount;
            GameObject newTower = Instantiate(s_GhostPrefab, s_GhostObject.transform.position, Quaternion.identity);

           
            newTower.name = $"{s_GhostPrefab.name} [{gridPos.x}, {gridPos.z}]";

            
            Transform parentFolder = GetTowerParentFolder(s_GhostPrefab.name);
            newTower.transform.parent = parentFolder;

            // 4. Mark position as occupied
            s_OccupiedPositions.Add(gridPos);
            currentObjectToPlace = null;
            DestroyGhost();
        }
    }

    private Vector3Int WorldToGridPosition(Vector3 worldPosition)
    {
        return new Vector3Int(
            Mathf.RoundToInt(worldPosition.x / gridsize),
            Mathf.RoundToInt(worldPosition.y / gridsize),
            Mathf.RoundToInt(worldPosition.z / gridsize)
        );
    }

    void SetGhostColor(Color color)
    {
        if (s_GhostObject == null) return;
        Renderer[] renderers = s_GhostObject.GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers) renderer.material.color = color;
    }

    private Vector2 GetMouseScreenPosition()
    {
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
        return Mouse.current.position.ReadValue();
#else
        return Input.mousePosition;
#endif
    }

    private bool WasLeftMousePressedThisFrame()
    {
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
        return Mouse.current.leftButton.wasPressedThisFrame;
#else
        return Input.GetMouseButtonDown(0);
#endif
    }
    Transform GetTowerParentFolder(string towerType)
    {
        // 1. Find or Create the Main "Towers" folder
        GameObject towersRoot = GameObject.Find("Towers");
        if (towersRoot == null)
        {
            towersRoot = new GameObject("Towers");
        }

        // 2. Find or Create the Sub-folder (e.g., "Shark") inside "Towers"
        // We look for a child with the name of the tower
        Transform subFolder = towersRoot.transform.Find(towerType);

        if (subFolder == null)
        {
            // If it doesn't exist, create it and parent it to "Towers"
            GameObject newSubFolder = new GameObject(towerType);
            newSubFolder.transform.parent = towersRoot.transform;
            subFolder = newSubFolder.transform;
        }

        return subFolder;
    }
}