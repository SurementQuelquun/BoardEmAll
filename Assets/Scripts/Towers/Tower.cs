//using System.Collections.Generic;
//using UnityEngine;
//#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
//using UnityEngine.InputSystem;
//#endif

//public class Tower : MonoBehaviour
//{
//    public GameObject ObjectToPlace;
//    public float gridsize = 1f;

//    // Single shared ghost across all Tower instances
//    private static GameObject s_GhostObject;
//    private static GameObject s_GhostPrefab; // prefab used to create the current ghost
//    private static Tower s_GhostOwner; // which Tower instance is driving the ghost
//    private static HashSet<Vector3> s_OccupiedPositions = new HashSet<Vector3>(); // global occupied positions

//    // Prevent multiple placements in the same frame
//    private static int s_LastPlacementFrame = -1;

//    private void Start()
//    {
//        CreateGhostObjectIfNeeded();
//    }

//    private void Update()
//    {
//        // Ensure there's a ghost created once (first Tower that runs will create and become owner).
//        CreateGhostObjectIfNeeded();

//        // Only the owner updates ghost position and handles placement to avoid multiple instantiations.
//        if (s_GhostOwner != this)
//            return;

//        UpdateGhostPosition();

//        if (WasLeftMousePressedThisFrame())
//        {
//            PlaceObject();
//        }
//    }

//    void CreateGhostObjectIfNeeded()
//    {
//        // If a ghost already exists and matches this prefab, don't recreate it.
//        if (s_GhostObject != null && s_GhostPrefab == ObjectToPlace)
//        {
//            // If there's no owner yet (unlikely), assign this as owner.
//            if (s_GhostOwner == null)
//                s_GhostOwner = this;
//            return;
//        }

//        // If a ghost exists but was created from a different prefab, destroy it (and owner).
//        if (s_GhostObject != null)
//        {
//            Destroy(s_GhostObject);
//            s_GhostObject = null;
//            s_GhostPrefab = null;
//            s_GhostOwner = null;
//        }

//        if (ObjectToPlace == null)
//            return;

//        // Create the single shared ghost and remember its source prefab and owner.
//        s_GhostObject = Instantiate(ObjectToPlace);
//        s_GhostPrefab = ObjectToPlace;
//        s_GhostOwner = this;

//        // Disable collider on ghost so it doesn't interfere with raycasts and physics.
//        var col = s_GhostObject.GetComponent<Collider>();
//        if (col != null)
//            col.enabled = false;

//        Renderer[] renderers = s_GhostObject.GetComponentsInChildren<Renderer>();

//        foreach (Renderer renderer in renderers)
//        {
//            Material material = renderer.material;
//            Color color = material.color;
//            color.a = 0.5f; // Set alpha to 50%
//            material.color = color;

//            material.SetInt("_Mode", 2);
//            material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
//            material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
//            material.SetInt("_ZWrite", 0);
//            material.DisableKeyword("_ALPHATEST_ON");
//            material.EnableKeyword("_ALPHABLEND_ON");
//            material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
//            material.renderQueue = 3000;
//        }
//    }

//    void UpdateGhostPosition()
//    {
//        if (s_GhostObject == null)
//            return;

//        Vector2 mouseScreenPos = GetMouseScreenPosition();
//        Ray ray = Camera.main.ScreenPointToRay(new Vector3(mouseScreenPos.x, mouseScreenPos.y, 0f));

//        if (Physics.Raycast(ray, out RaycastHit hit))
//        {
//            Vector3 point = hit.point;
//            Vector3 snappedPosition = new Vector3(
//                Mathf.Round(point.x / gridsize) * gridsize,
//                Mathf.Round(point.y / gridsize) * gridsize,
//                Mathf.Round(point.z / gridsize) * gridsize
//            );

//            Debug.Log($"Ghost snapped position: {snappedPosition} position hit point: {point}");

//            s_GhostObject.transform.position = snappedPosition;
//            if (s_OccupiedPositions.Contains(snappedPosition))
//                SetGhostColor(Color.red);
//            else
//                SetGhostColor(new Color(1f, 1f, 1f, 0.5f));
//        }
//    }

//    void SetGhostColor(Color color)
//    {
//        if (s_GhostObject == null)
//            return;

//        Renderer[] renderers = s_GhostObject.GetComponentsInChildren<Renderer>();
//        foreach (Renderer renderer in renderers)
//        {
//            Material material = renderer.material;
//            material.color = color;
//        }
//    }

//    // New: check the tile under the ghost at the moment of placement.
//    void PlaceObject()
//    {
//        if (s_GhostObject == null || s_GhostPrefab == null)
//            return;

//        // Guard: only one placement per frame
//        if (Time.frameCount == s_LastPlacementFrame)
//            return;

//        // Raycast down from slightly above the ghost to find the tile it's sitting on
//        Vector3 origin = s_GhostObject.transform.position + Vector3.up * 0.5f;
//        if (!Physics.Raycast(origin, Vector3.down, out RaycastHit hit, 1f))
//            return; // no surface under ghost -> don't place

//        // Look for a Tile component on the hit object or its parents
//        var hitTile = hit.collider.GetComponentInParent<Tile>();

//        // If there's a Tile component, prefer its constructible flag if present.
//        bool canPlace = true;
//        if (hitTile != null)
//        {
//            // try to read a boolean property/field named "IsConstructible" or "_isConstructible"
//            var type = hitTile.GetType();
//            var prop = type.GetProperty("IsConstructible");
//            if (prop != null && prop.PropertyType == typeof(bool))
//            {
//                canPlace = (bool)prop.GetValue(hitTile);
//            }
//            else
//            {
//                var field = type.GetField("_isConstructible", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
//                            ?? type.GetField("isConstructible", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
//                            ?? type.GetField("IsConstructible", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
//                if (field != null && field.FieldType == typeof(bool))
//                    canPlace = (bool)field.GetValue(hitTile);
//                // if no property/field found, assume tile is constructible (true)
//            }
//        }
//        else
//        {
//            // No Tile component found: require that the hit object be explicitly tagged "Constructible"
//            //canPlace = hit.collider.gameObject.CompareTag("Constructible");
//        }

//        if (!canPlace)
//            return;

//        Vector3 placementPosition = s_GhostObject.transform.position;
//        if (!s_OccupiedPositions.Contains(placementPosition))
//        {
//            s_LastPlacementFrame = Time.frameCount;
//            Instantiate(s_GhostPrefab, placementPosition, Quaternion.identity);
//            s_OccupiedPositions.Add(placementPosition);
//        }
//    }

//    // Input abstraction to support both the new Input System package and the legacy Input Manager.
//    private Vector2 GetMouseScreenPosition()
//    {
//#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
//        return Mouse.current.position.ReadValue();
//#else
//        return Input.mousePosition;
//#endif
//    }

//    private bool WasLeftMousePressedThisFrame()
//    {
//#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
//        return Mouse.current.leftButton.wasPressedThisFrame;
//#else
//        return Input.GetMouseButtonDown(0);
//#endif
//    }
//}
using System.Collections.Generic;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
using UnityEngine.InputSystem;
#endif

public class Tower : MonoBehaviour
{
    public GameObject ObjectToPlace;
    public float gridsize = 1f;

    // Single shared ghost across all Tower instances
    private static GameObject s_GhostObject;
    private static GameObject s_GhostPrefab;
    private static Tower s_GhostOwner;

    // CHANGED: Now using Vector3Int to store occupied slots as strict Integers
    private static HashSet<Vector3Int> s_OccupiedPositions = new HashSet<Vector3Int>();

    // Prevent multiple placements in the same frame
    private static int s_LastPlacementFrame = -1;

    private void Start()
    {
        CreateGhostObjectIfNeeded();
    }

    private void Update()
    {
        CreateGhostObjectIfNeeded();

        if (s_GhostOwner != this)
            return;

        UpdateGhostPosition();

        if (WasLeftMousePressedThisFrame())
        {
            PlaceObject();
        }
    }

    void CreateGhostObjectIfNeeded()
    {
        if (s_GhostObject != null && s_GhostPrefab == ObjectToPlace)
        {
            if (s_GhostOwner == null)
                s_GhostOwner = this;
            return;
        }

        if (s_GhostObject != null)
        {
            Destroy(s_GhostObject);
            s_GhostObject = null;
            s_GhostPrefab = null;
            s_GhostOwner = null;
        }

        if (ObjectToPlace == null)
            return;

        s_GhostObject = Instantiate(ObjectToPlace);
        s_GhostPrefab = ObjectToPlace;
        s_GhostOwner = this;

        var col = s_GhostObject.GetComponent<Collider>();
        if (col != null)
            col.enabled = false;

        Renderer[] renderers = s_GhostObject.GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            Material material = renderer.material;
            Color color = material.color;
            color.a = 0.5f;
            material.color = color;

            material.SetInt("_Mode", 2);
            material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            material.SetInt("_ZWrite", 0);
            material.DisableKeyword("_ALPHATEST_ON");
            material.EnableKeyword("_ALPHABLEND_ON");
            material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            material.renderQueue = 3000;
        }
    }

    void UpdateGhostPosition()
    {
        if (s_GhostObject == null)
            return;

        Vector2 mouseScreenPos = GetMouseScreenPosition();
        Ray ray = Camera.main.ScreenPointToRay(new Vector3(mouseScreenPos.x, mouseScreenPos.y, 0f));

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            // NEW: Calculate the coordinate as a pure Integer first
            Vector3Int gridPos = WorldToGridPosition(hit.point);

            // Convert back to world space for visual placement
            // We cast gridPos to Vector3 so we can multiply by the float gridsize
            Vector3 snappedPosition = (Vector3)gridPos * gridsize;

            s_GhostObject.transform.position = snappedPosition;

            // Check the HashSet using the Integer coordinate
            if (s_OccupiedPositions.Contains(gridPos))
                SetGhostColor(Color.red);
            else
                SetGhostColor(new Color(1f, 1f, 1f, 0.5f));
        }
    }

    void PlaceObject()
    {
        if (s_GhostObject == null || s_GhostPrefab == null)
            return;

        if (Time.frameCount == s_LastPlacementFrame)
            return;

        // Check surface below
        Vector3 origin = s_GhostObject.transform.position + Vector3.up * 0.5f;
        if (!Physics.Raycast(origin, Vector3.down, out RaycastHit hit, 1f))
            return;

        var hitTile = hit.collider.GetComponentInParent<Tile>();
        bool canPlace = true;

        // (Kept your existing Reflection logic for IsConstructible)
        if (hitTile != null)
        {
            var type = hitTile.GetType();
            var prop = type.GetProperty("IsConstructible");
            if (prop != null && prop.PropertyType == typeof(bool))
            {
                canPlace = (bool)prop.GetValue(hitTile);
            }
            else
            {
                var field = type.GetField("_isConstructible", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                            ?? type.GetField("isConstructible", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
                            ?? type.GetField("IsConstructible", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                if (field != null && field.FieldType == typeof(bool))
                    canPlace = (bool)field.GetValue(hitTile);
            }
        }

        if (!canPlace)
            return;

        // NEW: Calculate the integer position based on the Ghost's current world position
        Vector3Int gridPos = WorldToGridPosition(s_GhostObject.transform.position);

        // Check using Integer math
        if (!s_OccupiedPositions.Contains(gridPos))
        {
            s_LastPlacementFrame = Time.frameCount;

            // Instantiate at the exact snapped position
            Instantiate(s_GhostPrefab, s_GhostObject.transform.position, Quaternion.identity);

            // Add the integer coordinate to the set
            s_OccupiedPositions.Add(gridPos);
        }
    }

    // Helper method to keep the math consistent in one place
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
        if (s_GhostObject == null)
            return;

        Renderer[] renderers = s_GhostObject.GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            Material material = renderer.material;
            material.color = color;
        }
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