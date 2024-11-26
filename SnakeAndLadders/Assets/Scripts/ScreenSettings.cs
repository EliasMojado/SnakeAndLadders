using UnityEngine;

public class ScreenSettings : MonoBehaviour
{
    void Start()
    {
        // Set the screen mode to windowed with a specific resolution
        Screen.SetResolution(1920, 1080, true); // Width, Height, Fullscreen (false for windowed)
    }
}