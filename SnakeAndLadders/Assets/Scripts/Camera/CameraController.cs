using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour
{
    public Transform player;      // Reference to the player's transform
    private float initialX;       // Store the initial X position of the camera
    private float initialY;       // Store the initial Y position of the camera
    private bool followPlayer = true; // Flag to control whether the camera follows the player
    public float panSpeed = 1f; // Speed of the camera pan

    private AudioSource audioSource;  // Reference to the AudioSource component
    public AudioClip winClip;         // Reference to the win audio clip

    private void Start()
    {
        // Save the initial X and Y positions of the camera
        initialX = transform.position.x;
        initialY = transform.position.y;

        // Get the AudioSource component attached to the Camera
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip = winClip;
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
        StopAllOtherAudio();

        // Start the camera pan and play audio simultaneously
        StartCoroutine(SmoothCameraPan(cheesePosition));
    }

    // Coroutine to smoothly pan the camera and play the win audio while panning
    public IEnumerator SmoothCameraPan(Vector3 targetPosition)
    {
        // Play the win audio as soon as the panning starts
        PlayWinAudio();

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

        // Stop the audio after the pan is complete
    }

    private void PlayWinAudio()
    {
        if (audioSource != null && winClip != null)
        {
            audioSource.clip = winClip;
            audioSource.Play();  // Play the audio
        }
    }

    private void stopWinAudio()
    {
        if (audioSource != null)
        {
            audioSource.Stop();  // Stop the audio after the pan is complete
        }
    }
    
    private void StopAllOtherAudio()
    {
        AudioSource[] allAudioSources = FindObjectsOfType<AudioSource>();

        // Stop all audio sources except for the one attached to the camera
        foreach (AudioSource otherSource in allAudioSources)
        {
            if (otherSource != audioSource)
            {
                otherSource.Stop();
            }
        }
    }
}
