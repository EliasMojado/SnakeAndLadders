using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform player;      // Reference to the player's transform
    private float initialX;       // Store the initial X position of the camera
    private float initialY;       // Store the initial Y position of the camera

    private void Start()
    {
        // Save the initial X and Y positions of the camera
        initialX = transform.position.x;
        initialY = transform.position.y;
    }

    private void LateUpdate()
    {
        if (player != null)
        {
            // Calculate new y position based on the player's elevation
            float targetY = initialY + player.position.y - 1.5f;

            if(targetY < initialY )
                targetY = initialY;

            Vector3 targetPosition = new Vector3(
                initialX,                            // Keep the initial X position
                targetY,
                -10f                             // Keep Z offset
            );

            transform.position = targetPosition;
        }
    }
}
