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

    // Support tower prefabs (IDs 4..7)
    public GameObject kelpiPrefab;     // ID: 4
    public GameObject sirensPrefab;     // ID: 5
    public GameObject cyllaPrefab;  // ID: 6
    public GameObject energyPrefab; // ID: 7

    [Header("Visuals")]
    public GameObject rangeIndicatorPrefab;

    [Header("Settings")]
    public float gridsize = 1f;

    // Internal variable for the tower we are CURRENTLY building
    private GameObject currentObjectToPlace;

    // Ghost Management
    private static GameObject s_GhostObject;
    private static GameObject s_GhostPrefab;
    private static Tower s_GhostOwner;

    private static GameObject s_RangeIndicator;

    // Grid Tracking
    private static HashSet<Vector3Int> s_OccupiedPositions = new HashSet<Vector3Int>();
    private static int s_LastPlacementFrame = -1;


    private void Start()
    {
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

    // --- SELECTION FUNCTION ---
    public void SelectTowerByID(int towerID)
    {
        switch (towerID)
        {
            case 0: currentObjectToPlace = flyingFishPrefab; break;
            case 1: currentObjectToPlace = krakenPrefab; break;
            case 2: currentObjectToPlace = seaUrchinPrefab; break;
            case 3: currentObjectToPlace = sharkPrefab; break;

            // Support towers
            case 4: currentObjectToPlace = kelpiPrefab; break;
            case 5: currentObjectToPlace = sirensPrefab; break;
            case 6: currentObjectToPlace = cyllaPrefab; break;
            case 7: currentObjectToPlace = energyPrefab; break;

            default: currentObjectToPlace = null; break;
        }
        DestroyGhost();
    }

    // --- GHOST LOGIC ---
    void CreateGhostObjectIfNeeded()
    {
        if (rangeIndicatorPrefab == null)
        {
            Debug.LogWarning("rangeIndicatorPrefab non assigné dans l'inspecteur !");
        }
        else
        {
            Debug.Log("rangeIndicatorPrefab assigné : " + rangeIndicatorPrefab.name);
        }

        Debug.Log("CreateGhostObjectIfNeeded appelé"); // 🔹 log de début
        if (currentObjectToPlace == null)
        {
            // Only destroy if WE own it. This prevents conflicts.
            if (s_GhostOwner == this) DestroyGhost();
            return;
        }

        if (s_GhostObject != null && s_GhostPrefab == currentObjectToPlace)
        {
            if (s_GhostOwner == null) s_GhostOwner = this;
            return;
        }

        DestroyGhost();

        s_GhostObject = Instantiate(currentObjectToPlace);
        s_GhostPrefab = currentObjectToPlace;
        s_GhostOwner = this;

        Debug.Log("Ghost créé pour : " + s_GhostPrefab.name);

        // --- RANGE VISUAL ---
        TowerCombat combat = s_GhostObject.GetComponent<TowerCombat>();
        if (combat != null)
        {
            combat.isPlaced = false; //ghost = jamais actif
        }

        if (combat == null)
        {
            Debug.LogWarning("TowerCombat non trouvé sur le ghost !");
        }
        else
        {
            Debug.Log("TowerCombat trouvé avec range = " + combat.range);
        }

        if (combat != null && rangeIndicatorPrefab != null)
        {
            s_RangeIndicator = Instantiate(rangeIndicatorPrefab);
            s_RangeIndicator.transform.SetParent(s_GhostObject.transform);

            float diameter = combat.range * 2f;
            s_RangeIndicator.transform.localScale = new Vector3(diameter, 0.05f, diameter);
            s_RangeIndicator.transform.localPosition = Vector3.zero;

            Debug.Log("RangeIndicator créé et positionné : " + s_RangeIndicator.transform.position);

            Renderer r = s_RangeIndicator.GetComponent<Renderer>();
            if (r == null)
            {
                Debug.LogWarning("Renderer manquant sur RangeIndicator !");
            }
            else
            {
                Debug.Log("Material assigné : " + r.sharedMaterial.name + " | Alpha : " + r.sharedMaterial.color.a);
            }
        }

        // Disable collider on ghost so we don't click it
        var col = s_GhostObject.GetComponent<Collider>();
        if (col != null) col.enabled = false;

        // Make transparent
        Renderer[] renderers = s_GhostObject.GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            Material material = renderer.material;
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

        if (s_RangeIndicator != null)
        {
            Destroy(s_RangeIndicator);
            s_RangeIndicator = null;
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
            s_GhostObject.transform.position = (Vector3)gridPos * gridsize;

            if (s_OccupiedPositions.Contains(gridPos))
                SetGhostColor(Color.red);
            else
                SetGhostColor(new Color(1f, 1f, 1f, 0.5f));
        }

        if (s_RangeIndicator != null)
        {
            s_RangeIndicator.transform.position = s_GhostObject.transform.position;
        }
    }

    void PlaceObject()
    {
        if (s_GhostObject == null || s_GhostPrefab == null) return;
        if (Time.frameCount == s_LastPlacementFrame) return;

        Vector3 origin = s_GhostObject.transform.position + Vector3.up * 0.5f;
        if (!Physics.Raycast(origin, Vector3.down, out RaycastHit hit, 1f)) return;

        var hitTile = hit.collider.GetComponentInParent<Tile>();
        bool canPlace = true;
        if (hitTile != null)
        {
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

            // Instantiate
            GameObject newTower = Instantiate(s_GhostPrefab, s_GhostObject.transform.position, Quaternion.identity);

            TowerCombat combat = newTower.GetComponent<TowerCombat>();
            if (combat != null)
            {
                combat.isPlaced = true; //tour ACTIVE
            }
            // Name
            newTower.name = $"{s_GhostPrefab.name} [{gridPos.x}, {gridPos.z}]";

            // Organize
            Transform parentFolder = GetTowerParentFolder(s_GhostPrefab.name);
            newTower.transform.parent = parentFolder;

            s_OccupiedPositions.Add(gridPos);
            currentObjectToPlace = null; 
            DestroyGhost();
        }
    }

    // --- HELPERS ---
    Transform GetTowerParentFolder(string towerType)
    {
        GameObject towersRoot = GameObject.Find("Towers");
        if (towersRoot == null) towersRoot = new GameObject("Towers");

        Transform subFolder = towersRoot.transform.Find(towerType);
        if (subFolder == null)
        {
            GameObject newSubFolder = new GameObject(towerType);
            newSubFolder.transform.parent = towersRoot.transform;
            subFolder = newSubFolder.transform;
        }
        return subFolder;
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
}