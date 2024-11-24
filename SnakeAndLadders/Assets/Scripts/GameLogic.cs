using UnityEngine;

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

    private void Awake()
    {
        Singleton = this;
    }
}
