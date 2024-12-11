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

    public void HandleCheeseCaptured(ushort playerId, Vector3 cheesePosition)
    {
        Debug.Log($"Handled cheese capture for Player {playerId}.");

        mainCamera.GetComponent<CameraController>().StopFollowingPlayer();
        mainCamera.GetComponent<CameraController>().PanCameraToCheese(cheesePosition);
    }
}
