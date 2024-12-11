using UnityEngine;
using System.Collections;

public class GameLogic : MonoBehaviour
{
    private static GameLogic _singleton;

    public static GameLogic Singleton
    {
        get => _singleton;

        private set
        {
            if (_singleton == null)
                _singleton = value;
            else if (_singleton != value)
            {
                Debug.Log($"{nameof(GameLogic)} instance already set! {_singleton} != {value}");
                Destroy(value);
            }
        }
    }

    public GameObject LocalPlayerPrefab => localPlayerPrefab;
    public GameObject PlayerPrefab => playerPrefab;
    public GameObject SnakePrefab => snakePrefab;

    [Header("Prefabs")]
    [SerializeField] public GameObject localPlayerPrefab;
    [SerializeField] public GameObject playerPrefab;
    [SerializeField] public GameObject snakePrefab;

    [Header("Camera")]
    [SerializeField] private Camera mainCamera;
    private float panSpeed = 5f;

    private void Awake()
    {
        Singleton = this;
    }

    private void PanCameraToCheese(Vector3 cheesePosition)
    {
        // Start a coroutine to smoothly pan the camera
        StartCoroutine(SmoothCameraPan(cheesePosition));
    }

    private IEnumerator SmoothCameraPan(Vector3 targetPosition)
    {
        Vector3 startPosition = mainCamera.transform.position;
        float elapsedTime = 0f;

        // Interpolate between the current camera position and the target cheese position
        while (elapsedTime < 1f)
        {
            mainCamera.transform.position = Vector3.Lerp(startPosition, targetPosition, elapsedTime);
            elapsedTime += Time.deltaTime * panSpeed;
            yield return null;
        }

        // Ensure the camera reaches the target position
        mainCamera.transform.position = targetPosition;
    }

    public void HandleCheeseCaptured(ushort playerId, Vector3 cheesePosition)
    {
        Debug.Log($"Handled cheese capture for Player {playerId}.");
        PanCameraToCheese(cheesePosition);
    }
}
