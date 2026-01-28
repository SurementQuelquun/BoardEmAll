//using UnityEngine;
//using UnityEngine.InputSystem;

//public class CameraMovement : MonoBehaviour
//{
//    public Transform targetObject;

//    public Camera Camera;
//    private InputAction LookAction;
//    private InputAction zoomAction;

//    public InputActionAsset InputSystem_Actions;

//    [SerializeField]
//    private float CameraRotationSpeed = 100f; 

//    private void Start()
//    {
//        LookAction = InputSystem.actions.FindAction("Look");
//    }
//    void Update()
//    {
//        Vector2 mousePosition = Mouse.current.position.ReadValue();
//        Ray ray = this.Camera.ScreenPointToRay(mousePosition);

//        Vector2 LookValue = LookAction.ReadValue<Vector2>();
//        if (LookAction.IsPressed())
//        {
//            float horizontalRotation = this.transform.rotation.eulerAngles.y - LookValue.x * Time.deltaTime * CameraRotationSpeed;
//            float verticalRotation = Mathf.Clamp(this.transform.rotation.eulerAngles.x + LookValue.y * Time.deltaTime * CameraRotationSpeed, 10f, 90f);

//            transform.rotation = Quaternion.Euler(verticalRotation, horizontalRotation, 0f);
//        }
//    }
//}

//using UnityEngine;
//using UnityEngine.InputSystem;

//public class TDCameraController : MonoBehaviour
//{
//    [Header("Input Setup")]
//    public InputActionAsset inputActions;
//    private InputAction moveAction;
//    private InputAction zoomAction;

//    [Header("Movement Settings")]
//    public float panSpeed = 20f;
//    public float zoomSpeed = 2f;
//    public float smoothing = 5f; // Higher is snappier, lower is smoother

//    [Header("Map Boundaries")]
//    // The X and Z limits of your map
//    public Vector2 mapLimitX = new Vector2(-50f, 50f);
//    public Vector2 mapLimitZ = new Vector2(-50f, 50f);

//    [Header("Zoom Boundaries")]
//    // How close and how high the camera can go
//    public float minHeight = 10f;
//    public float maxHeight = 50f;

//    private Vector3 targetPosition;

//    private void Awake()
//    {
//        // Initialize target position to current position to prevent snapping at start
//        targetPosition = transform.position;
//    }

//    private void OnEnable()
//    {
//        // Enable the Input Actions
//        // Assuming your Action Map is named "Player" and actions are "Move" and "Zoom"
//        if (inputActions != null)
//        {
//            var actionMap = inputActions.FindActionMap("Player");
//            moveAction = actionMap.FindAction("Move");
//            zoomAction = actionMap.FindAction("Zoom");

//            moveAction.Enable();
//            zoomAction.Enable();
//        }
//    }

//    private void OnDisable()
//    {
//        moveAction?.Disable();
//        zoomAction?.Disable();
//    }

//    private void Update()
//    {
//        HandleMovement();
//        HandleZoom();
//        MoveCamera();
//    }

//    private void HandleMovement()
//    {
//        // Read WASD (or Arrow keys) as a Vector2
//        Vector2 input = moveAction.ReadValue<Vector2>();

//        // Calculate the movement vector (X and Z only)
//        // We use transform.right and transform.forward to move relative to camera direction
//        // But we zero out the Y component so we don't fly into the ground
//        Vector3 forward = transform.forward;
//        forward.y = 0;
//        forward.Normalize();

//        Vector3 right = transform.right;
//        right.y = 0;
//        right.Normalize();

//        Vector3 moveDir = (forward * input.y + right * input.x).normalized;

//        // Update target position
//        targetPosition += moveDir * panSpeed * Time.deltaTime;

//        // Clamp X and Z (The Map Boundaries)
//        targetPosition.x = Mathf.Clamp(targetPosition.x, mapLimitX.x, mapLimitX.y);
//        targetPosition.z = Mathf.Clamp(targetPosition.z, mapLimitZ.x, mapLimitZ.y);
//    }

//    private void HandleZoom()
//    {
//        // Read Scroll Wheel (returns a Vector2, usually Y is the scroll value)
//        float scrollValue = zoomAction.ReadValue<Vector2>().y;

//        // Adjust smooth zoom sensitivity (Scroll values can be large)
//        float zoomAdjustment = scrollValue * zoomSpeed * 0.01f;

//        // Apply to height (Y axis)
//        // Note: We subtract because scrolling UP usually means zooming IN (lowering height)
//        targetPosition.y -= zoomAdjustment;

//        // Clamp Y (The Zoom Boundaries)
//        targetPosition.y = Mathf.Clamp(targetPosition.y, minHeight, maxHeight);
//    }

//    private void MoveCamera()
//    {
//        // Smoothly interpolate the current position to the target position
//        transform.position = Vector3.Lerp(transform.position, targetPosition, smoothing * Time.deltaTime);
//    }
//}

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

        // IMPORTANT: Scroll values are usually �120. We normalize this.
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