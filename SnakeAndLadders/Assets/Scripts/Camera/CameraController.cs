using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour
{
    public Transform player;      // Reference to the player's transform
    private float initialX;       // Store the initial X position of the camera
    private float initialY;       // Store the initial Y position of the camera
    private bool followPlayer = true; // Flag to control whether the camera follows the player
    public float panSpeed = 1f; // Speed of the camera pan

    private void Start()
    {
        // Save the initial X and Y positions of the camera
        initialX = transform.position.x;
        initialY = transform.position.y;
    }

    private void LateUpdate()
    {
        if (followPlayer && player != null)
        {
            // Calculate new y position based on the player's elevation
            float targetY = initialY + player.position.y - 1.5f;

            if (targetY < initialY)
                targetY = initialY;

            Vector3 targetPosition = new Vector3(
                initialX,                            // Keep the initial X position
                targetY,
                -10f                             // Keep Z offset
            );

            transform.position = targetPosition;
        }
    }

    public void StopFollowingPlayer()
    {
        followPlayer = false;
    }

    public void ResumeFollowingPlayer()
    {
        followPlayer = true;
    }

    public void PanCameraToCheese(Vector3 cheesePosition)
    {
        // Stop the camera from following the player
        StopFollowingPlayer();

        // Start a coroutine to smoothly pan the camera
        StartCoroutine(SmoothCameraPan(cheesePosition));
    }

    public IEnumerator SmoothCameraPan(Vector3 targetPosition)
    {
        // Keep the current X position to avoid horizontal movement
        float startX = transform.position.x;
        float startY = transform.position.y;
        float targetY = targetPosition.y;  // Only target the vertical position (Y axis)
        
        float elapsedTime = 0f;

        // Interpolate only the Y position of the camera, keeping the X position fixed
        while (elapsedTime < 1f)
        {
            float newY = Mathf.Lerp(startY, targetY, elapsedTime);
            transform.position = new Vector3(startX, newY, -10f); // Keep X fixed, only move vertically
            elapsedTime += Time.deltaTime * panSpeed;
            yield return null;
        }

        // Ensure the camera reaches the target position
        transform.position = new Vector3(startX, targetY, -10f);
    }
}
