using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform player;
    // Offset for isometric view: moved back and up
    public Vector3 offset = new Vector3(20, 20, -20); 
    public float smoothTime = 0.3f; 
    
    private Vector3 currentVelocity = Vector3.zero;

    void Start()
    {
        // Set to isometric angle: 35.264 degrees X, 45 degrees Y
        transform.rotation = Quaternion.Euler(35.264f, 45f, 0f);
        
        if (player == null)
        {
            Debug.LogWarning("CameraFollow: Player transform is not assigned!");
        }
    }

    void LateUpdate()
    {
        if (player != null)
        {
            Vector3 targetPosition = player.position + offset;
            // Smoothly move the camera to the target position
            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref currentVelocity, smoothTime);
            
            // Maintain isometric rotation
            transform.rotation = Quaternion.Euler(35.264f, 45f, 0f);
        }
    }
}