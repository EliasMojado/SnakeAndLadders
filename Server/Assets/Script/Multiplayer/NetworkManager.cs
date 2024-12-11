using Riptide;
using Riptide.Utils;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SocialPlatforms;


public enum ServerToClientId : ushort
{
    playerSpawned = 1,
    playerMovement,
    snakeSpawned,
    snakeUpdated,
    handshake,
}

public enum ClientToServerId : ushort
{
    name = 1,
    input,
    handshake,
}

public class NetworkManager : MonoBehaviour
{
    private static NetworkManager _singleton;
    private UdpClient udpClient;
    ushort BroadcastPortReceive = 7801;
    ushort BroadcastPortSend = 7800;
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
        if (Singleton != null)
        {
            Debug.LogWarning("NetworkManager instance already set!");
            Destroy(this);
            return;
        }
        Singleton = this;
    }

    private void Start()
    {
        Application.targetFrameRate = 60;
        RiptideLogger.Initialize(Debug.Log, Debug.Log, Debug.LogWarning, Debug.LogError, false);

        Server = new Server();

        Singleton = this;
        udpClient = new UdpClient();
        udpClient.EnableBroadcast = true;
        InvokeRepeating(nameof(SendBroadcast), 0f, 2f); // Broadcast every
        // localhost
        Server.Start(port, maxClientCount);

        // lan
        // Server.Start(port, maxClientCount, "192.168.196.140");

        Server.ClientDisconnected += PlayerLeft;

        Application.runInBackground = true;
        
        Debug.Log("[NetworkManager] Server initialized and started.");
    }

    private void OnApplicationQuit()
    {
        udpClient.Close();
        Server.Stop();
    }

    private void OnDestroy()
    {
        udpClient.Close();
    }


    private void SendBroadcast()
    {
        var ip = GetLocalIPAddress();
        Debug.Log("SendBroadcast");
        int connectedClients = Player.list.Count;
        string message = $"{ip}:{port}, Server{port}, {connectedClients}/{maxClientCount}";
        byte[] data = Encoding.UTF8.GetBytes(message);
        Debug.Log(IPAddress.Broadcast);
        IPEndPoint endPoint = new IPEndPoint(IPAddress.Broadcast, BroadcastPortSend);
        Debug.Log($"Broadcast message sent to {endPoint.Address}:{endPoint.Port}");
        udpClient.Send(data, data.Length, endPoint);
    }

    private string GetLocalIPAddress()
    {
        var host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (var ip in host.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                return ip.ToString();
            }
        }
        throw new System.Exception("No network adapters with an IPv4 address in the system!");
    }

    private void FixedUpdate()
    {
        Server.Update();
    }
    private void PlayerLeft(object sender, ServerDisconnectedEventArgs e)
    {
        Destroy(Player.list[e.Client.Id].gameObject);
    }

    [MessageHandler((ushort)ClientToServerId.handshake)]
    private static void HandleHandshake(ushort clientId, Message message)
    {
        string name = message.GetString();
        Debug.LogWarning($"[NetworkManager] Received handshake from {clientId}: {name}");
    }

}
