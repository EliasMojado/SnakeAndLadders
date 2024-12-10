using Riptide;
using Riptide.Utils;
using UnityEngine;

public enum ServerToClientId : ushort
{
    playerSpawned = 1,
    playerMovement,
    snakeSpawned,
    snakeUpdated,
}

public enum ClientToServerId : ushort
{
    name = 1,
    input,
}

public class NetworkManager : MonoBehaviour
{
    private static NetworkManager _singleton;

    public static NetworkManager Singleton
    {
        get => _singleton;

        private set
        {
            if (_singleton == null)
                _singleton = value;
            else if(_singleton != value)
            {
                Debug.Log($"{nameof(NetworkManager)} instance already set! {_singleton} != {value}");
                Destroy(value);
            }
        }
    }

    public Server Server { get; private set; }

    [SerializeField] private ushort port;
    [SerializeField] private ushort maxClientCount;

    private void Awake()
    {
        Singleton = this;
    }

    private void Start()
    {
        Application.targetFrameRate = 60;
        RiptideLogger.Initialize(Debug.Log, Debug.Log, Debug.LogWarning, Debug.LogError, false);

        Server = new Server();
        
        // localhost
        Server.Start(port, maxClientCount);

        // lan
        // Server.Start(port, maxClientCount, "192.168.196.140");

        Server.ClientDisconnected += PlayerLeft;

        Application.runInBackground = true;
        
        Debug.Log("[NetworkManager] Server initialized and started.");
    }

    private void FixedUpdate()
    {
        Server.Update();
    }

    private void OnApplicationQuit()
    {
        Server.Stop();
    }

    private void PlayerLeft(object sender, ServerDisconnectedEventArgs e)
    {
        Destroy(Player.list[e.Client.Id].gameObject);
    }

}
