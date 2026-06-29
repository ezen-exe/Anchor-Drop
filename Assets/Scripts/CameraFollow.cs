using UnityEngine;
using UnityEngine.EventSystems;

public class CameraFollow : MonoBehaviour
{
    [Header("Targeting")]
    public Transform player;
    public bool isFollowingPlayer = true;

    [Header("Offsets & Angles")]
    // FIXED: X offset changed to -20 so it accurately centers on the player mathematically
    public Vector3 isometricOffset = new Vector3(-20, 20, -20);
    public Vector3 isometricRotation = new Vector3(35.264f, 45f, 0f);
    
    public Vector3 topDownOffset = new Vector3(0, 50, 0);
    public Vector3 topDownRotation = new Vector3(90f, 0f, 0f);

    [Header("Movement Settings")]
    public float smoothTime = 0.3f;
    public float panSpeed = 0.05f;
    public float zoomSpeed = 0.1f;
    
    [Tooltip("Min/Max limits for Zooming (adjust these depending on your map scale)")]
    public float minZoom = 10f;
    public float maxZoom = 500f; // INCREASED: Allow zooming much further out

    private Vector3 currentVelocity = Vector3.zero;
    private bool isTopView = false;
    private Camera cam;
    
    // Store the original zoom level so we can reset it later
    private float defaultZoom; 

    void Start()
    {
        cam = GetComponent<Camera>();
        
        // Capture default zoom right as the scene starts
        if (cam != null)
        {
            defaultZoom = cam.orthographic ? cam.orthographicSize : cam.fieldOfView;
        }
        
        ResetToPlayer();
    }

    void LateUpdate()
    {
        HandleTouchInput();

        // 1. Handle Position
        if (isFollowingPlayer && player != null)
        {
            Vector3 targetOffset = isTopView ? topDownOffset : isometricOffset;
            Vector3 targetPosition = player.position + targetOffset;
            
            // Smoothly glide to the player
            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref currentVelocity, smoothTime);
        }

        // 2. Handle Rotation
        Quaternion targetRot = Quaternion.Euler(isTopView ? topDownRotation : isometricRotation);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 5f);
    }

    void HandleTouchInput()
    {
        if (Input.touchCount == 0) return;

        // Prevent dragging the map if the user is tapping a UI Button
        if (EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId)) return;

        // --- PANNING (Single Touch Drag) ---
        if (Input.touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Moved)
        {
            isFollowingPlayer = false; // Stop tracking the player
            Vector2 touchDelta = Input.GetTouch(0).deltaPosition;
            
            // Calculate movement relative to camera's current Y rotation so dragging feels natural
            Vector3 move = new Vector3(-touchDelta.x * panSpeed, 0, -touchDelta.y * panSpeed);
            move = Quaternion.Euler(0, transform.eulerAngles.y, 0) * move;
            
            transform.position += move;
        }
        
        // --- ZOOMING (Pinch with Two Fingers) ---
        if (Input.touchCount == 2)
        {
            isFollowingPlayer = false; // Stop tracking the player
            
            Touch touchZero = Input.GetTouch(0);
            Touch touchOne = Input.GetTouch(1);

            // Find the position in the previous frame of each touch
            Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
            Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

            // Find the magnitude of the vector (distance) between the touches in each frame
            float prevMagnitude = (touchZeroPrevPos - touchOnePrevPos).magnitude;
            float currentMagnitude = (touchZero.position - touchOne.position).magnitude;

            // Find the difference in the distances between each frame
            float difference = currentMagnitude - prevMagnitude;

            ZoomCamera(difference * zoomSpeed);
        }
    }

    void ZoomCamera(float increment)
    {
        // Adjusts either the Field of View (Perspective) or Size (Orthographic)
        if (cam.orthographic)
        {
            cam.orthographicSize = Mathf.Clamp(cam.orthographicSize - increment, minZoom, maxZoom);
        }
        else
        {
            cam.fieldOfView = Mathf.Clamp(cam.fieldOfView - increment, minZoom, maxZoom);
        }
    }

    // ==========================================
    // --- UI BUTTON METHODS (Call these from Buttons) ---
    // ==========================================

    public void ResetToPlayer()
    {
        isFollowingPlayer = true;
        isTopView = false; // Resets the view back to Isometric

        // Reset the zoom level back to its default state
        if (cam != null)
        {
            if (cam.orthographic)
            {
                cam.orthographicSize = defaultZoom;
            }
            else
            {
                cam.fieldOfView = defaultZoom;
            }
        }
    }

    public void ToggleTopView()
    {
        isTopView = !isTopView; // Swaps between true and false
    }
}