

using UnityEngine;
using UnityEngine.InputSystem;

public class RTSKeyboardCamera : MonoBehaviour
{
    [Header("Input Setup")]
    public InputActionAsset inputActions;
    private InputAction moveAction;   // WASD (Panning)
    private InputAction lookAction;   // QEZX (Rotation)
    private InputAction zoomAction;   // Mouse Scroll (Zoom)

    [Header("Map Boundaries (Pivot Movement)")]
    public float panSpeed = 20f;
    public Vector2 mapLimitX = new Vector2(-50f, 50f);
    public Vector2 mapLimitZ = new Vector2(-50f, 50f);

    // The invisible point on the ground the camera looks at
    private Vector3 pivotPoint;

    [Header("Rotation Settings (Q/E/Z/X)")]
    public float rotationSpeed = 60f; // Degrees per second
    public float minPitch = 20f;      // Lowest angle (near ground)
    public float maxPitch = 80f;      // Highest angle (overhead)

    [Header("Zoom Settings (Scroll)")]
    public float minZoom = 10f;       // Closest to pivot
    public float maxZoom = 60f;       // Furthest from pivot
    public float zoomSpeed = 2f;      // Sensitivity
    public float zoomSmoothing = 5f;  // Makes zoom fluid

    // Internal variables to track where the camera should be
    private float currentZoom = 30f;
    private float targetZoom = 30f;
    private float currentYaw = 0f;    // Horizontal rotation
    private float currentPitch = 45f; // Vertical rotation

    private void Awake()
    {
        // Initialize the pivot point based on where the camera starts
        // We project a ray forward to find "ground zero" relative to camera
        pivotPoint = transform.position;
        pivotPoint.y = 0;

        // Set initial zoom target
        targetZoom = currentZoom;
    }

    private void OnEnable()
    {
        if (inputActions != null)
        {
            var map = inputActions.FindActionMap("Player");

            moveAction = map.FindAction("Move"); // WASD
            lookAction = map.FindAction("Look"); // QEZX
            zoomAction = map.FindAction("Zoom"); // Scroll Wheel

            moveAction.Enable();
            lookAction.Enable();
            zoomAction.Enable();
        }
    }

    private void Update()
    {
        HandlePivotMovement();
        HandleKeyboardRotation();
        HandleZoom();
        UpdateCameraTransform();
    }

    private void HandlePivotMovement()
    {
        // 1. Move the Pivot (The point we are looking at)
        Vector2 input = moveAction.ReadValue<Vector2>();

        // Calculate Forward/Right relative to the camera's current Yaw
        // This ensures "W" always moves "Up" on the screen, regardless of rotation
        Quaternion yawRotation = Quaternion.Euler(0, currentYaw, 0);

        Vector3 moveDir = yawRotation * new Vector3(input.x, 0, input.y);

        pivotPoint += moveDir * panSpeed * Time.deltaTime;

        // Clamp the pivot so it doesn't leave the map
        pivotPoint.x = Mathf.Clamp(pivotPoint.x, mapLimitX.x, mapLimitX.y);
        pivotPoint.z = Mathf.Clamp(pivotPoint.z, mapLimitZ.x, mapLimitZ.y);
    }

    private void HandleKeyboardRotation()
    {
        // 2. Rotate around the Pivot using Q/E/Z/X
        // You said "Look" is mapped to a Vector2 (Up/Down/Left/Right)
        Vector2 rotationInput = lookAction.ReadValue<Vector2>();

        // rotationInput.x comes from Q (Left) and E (Right)
        // rotationInput.y comes from Z (Up) and X (Down)

        // Rotate Horizontal (Yaw)
        currentYaw += rotationInput.x * rotationSpeed * Time.deltaTime;

        // Rotate Vertical (Pitch)
        // We add input.y because Z is usually "positive" in Input System
        currentPitch += rotationInput.y * rotationSpeed * Time.deltaTime;

        // Clamp the Pitch (The Demi-Sphere limits)
        currentPitch = Mathf.Clamp(currentPitch, minPitch, maxPitch);
    }

    private void HandleZoom()
    {
        // 3. Zoom Logic
        // We read the Vector2.y from the scroll wheel
        float scrollDelta = zoomAction.ReadValue<Vector2>().y;

        // IMPORTANT: Scroll values are usually ±120. We normalize this.
        if (Mathf.Abs(scrollDelta) > 0.1f)
        {
            // Normalize: 120 becomes ~1
            float normalizedScroll = scrollDelta > 0 ? 1 : -1;

            // Subtracting moves closer (Zoom In), Adding moves away (Zoom Out)
            targetZoom -= normalizedScroll * zoomSpeed;
            targetZoom = Mathf.Clamp(targetZoom, minZoom, maxZoom);
        }

        // Smoothly interpolate the actual zoom value
        currentZoom = Mathf.Lerp(currentZoom, targetZoom, zoomSmoothing * Time.deltaTime);
    }

    private void UpdateCameraTransform()
    {
        // 4. Mathematical Magic: Spherical Coordinates
        // Create a rotation based on our Pitch and Yaw
        Quaternion rotation = Quaternion.Euler(currentPitch, currentYaw, 0);

        // Calculate offset: A vector pointing backwards from the pivot
        Vector3 offset = rotation * Vector3.forward * -currentZoom;

        // Apply position
        transform.position = pivotPoint + offset;

        // Always look at the pivot
        transform.LookAt(pivotPoint);
    }
}