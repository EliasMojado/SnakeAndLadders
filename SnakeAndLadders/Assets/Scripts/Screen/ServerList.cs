using Riptide;
using UnityEngine;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using TMPro;
using Riptide.Transports;
using UnityEngine.UI;

struct ServerInfo
{
    public string serverName;
    public string playerLeft;
    public string ip;
    public ushort port;
    public bool isOnline;
}

public class ServerList : MonoBehaviour
{
    private static ServerList _singleton;
    public UdpClient udpClient;
    Dictionary<string, ServerInfo> serverList = new Dictionary<string, ServerInfo>();
    [SerializeField] private GameObject serverListPrefab;
    Transform list;
    Transform localListTransform;
    Transform onlineListTransform;
    public GameObject getServerListInstance => serverListInstance;
    private GameObject serverListInstance;
    private GameObject localList;
    private GameObject onlineList;
    [SerializeField] private GameObject serverItemPrefab;
    private GameObject serverItemInstance;
    [SerializeField] private GameObject onlineConnectButton;
    private GameObject onlineConnectButtonInstance;
    string ipOnline = "46.250.236.30";
    ushort broadcastPortReceive = 7800;
    ushort broadcastPortSend = 7801;
    ushort localPort = 7777;
    public string username = "Player No Name";
    TMP_InputField inputField;

    private float updateInterval = 3.0f; // 1 second interval
    private float nextUpdateTime = 0f;

    public static ServerList Singleton
    {
        get => _singleton;

        private set
        {
            if (_singleton == null)
                _singleton = value;
            else if (_singleton != value)
            {
                Debug.Log($"{nameof(ServerList)} instance already set! {_singleton} != {value}");
                Destroy(value);
            }
        }
    }

    private void Awake()
    {
        Singleton = this;
    }


    private void OnDestroy()
    {
        udpClient.Close();
    }

    public void LoadScreen()
    {
        udpClient = new UdpClient(broadcastPortReceive);
        StartCoroutine(CallLoadServersRepeatedly());
        serverListInstance = Instantiate(ServerList.Singleton.serverListPrefab);
        list = serverListInstance.transform.GetChild(0).transform.GetChild(1);
        localListTransform = list.Find("LocalList");
        onlineListTransform = list.Find("OnlineList");

        // Get the canvas at index 2
        Transform canvasTransform = list.Find("Online");
        // Get the button at index 1 of the canvas
        Button connectOnlineButton = canvasTransform.GetChild(1).GetComponent<Button>();
        // Assign the ConnectOnline method to the button's onClick listener
        connectOnlineButton.onClick.AddListener(ConnectOnline);
        
    }

    public string GetUsername()
    {
        inputField = list.Find("Panel").GetChild(0).GetComponent<TMP_InputField>();
        return inputField.text;
    }

    public void ConnectOnline()
    {
        Debug.Log("ConnectOnline");
        NetworkManager.Singleton.Connect(ipOnline, localPort);
    }

    public async Task ListenForBroadcasts()
    {
        Debug.Log("ListenForBroadcasts");
        serverList.Clear();
        Debug.Log(serverList.Count);
        while (true)
        {
            UdpReceiveResult result = await udpClient.ReceiveAsync();
            string message = Encoding.UTF8.GetString(result.Buffer);
            Debug.Log($"Received broadcast from {result.RemoteEndPoint}: {message}");
            string[] messages = message.Split(',');
            string ip = messages[0].Split(':')[0];
            ushort port;
            if (!ushort.TryParse(messages[0].Split(':')[1], out port))
            {
                Debug.LogError("Failed to parse port from message.");
                continue;
            }
            string serverName = messages[1];
            string playerLeft = messages[2];

            serverList[result.RemoteEndPoint.ToString()] = new ServerInfo {
                ip = ip,
                port = port,
                serverName = serverName, 
                playerLeft = playerLeft,
                isOnline = false
            };
            // Handle the received broadcast message (e.g., add to server list)
        }
    }


    private void Update()
    {
        //Debug.Log(Time.time);
        //if (Time.time >= nextUpdateTime)
        //{
        //    LoadServers();
        //    nextUpdateTime = Time.time + updateInterval;
        //}
    }

    private IEnumerator CallLoadServersRepeatedly()
    {
        while (true)
        {
            yield return new WaitForSeconds(2f);
            LoadServers();
        }
    }

    public void LoadServers()
    {
        Debug.Log("LoadServers");
        foreach (Transform child in localListTransform)
        {
            Debug.Log("destroyed child");
            Destroy(child.gameObject);
        }

        Debug.Log("serverList.Count: " + serverList.Count);

        //// Add new server items
        foreach (KeyValuePair<string, ServerInfo> server in serverList)
        {
            GameObject serverItemInstance = ServerList.Singleton.serverItemPrefab;
            GameObject localServerItem = Instantiate(serverItemInstance);
            localServerItem.transform.SetParent(localListTransform, false);
            localServerItem.name = server.Value.serverName;

            Button button = localServerItem.GetComponentInChildren<Button>();
            if (button != null)
            {
                button.onClick.AddListener(() => NetworkManager.Singleton.Connect(server.Value.ip, server.Value.port));
            }
            else
            {
                Debug.LogError("Button component not found in localServerItem.");
            }

            Transform serverNameTransform = localServerItem.transform.Find("ServerName");
            if (serverNameTransform != null)
            {
                TextMeshProUGUI textComponent = serverNameTransform.GetComponent<TextMeshProUGUI>();
                if (textComponent != null)
                {
                    textComponent.text = server.Value.serverName;
                }
                else
                {
                    Debug.LogError("TextMeshProUGUI component not found on the 'serverName' object.");
                }
            }

            Transform playerLeftTransform = localServerItem.transform.Find("PlayerLeft");
            if (serverNameTransform != null)
            {
                TextMeshProUGUI textComponent = playerLeftTransform.GetComponent<TextMeshProUGUI>();
                if (textComponent != null)
                {
                    textComponent.text = server.Value.playerLeft;
                }
                else
                {
                    Debug.LogError("TextMeshProUGUI component not found on the 'serverName' object.");
                }
            }
        }


        serverList.Clear();

        //// Instantiate serverItem and set its parent to onlineListTransform
        //GameObject onlineServerItem = Instantiate(serverItem);
        //onlineServerItem.transform.SetParent(onlineListTransform, false);
        //onlineServerItem.name = "OnlineServerItem";
    }

    //private async Task ScanOnline()
    //{
    //    for (ushort i = 0; i < 10; i++)
    //    {
    //        NetworkManager.Singleton.Connect(ipOnline, (ushort)(onlineStartPort + i));
    //    }
    //}

    //private void scanLocal()
    //{
    //    var host = Dns.GetHostEntry(Dns.GetHostName());
    //    string networkIp = "";
    //    foreach (var ip in host.AddressList)
    //    {
    //        if (ip.AddressFamily == AddressFamily.InterNetwork)
    //        {
    //            string[] ipParts = ip.ToString().Split('.');
    //            if (ipParts.Length == 4)
    //            {
    //                networkIp = $"{ipParts[0]}.{ipParts[1]}.{ipParts[2]}";
    //                break;
    //            }
    //        }
    //    }

    //    if (string.IsNullOrEmpty(networkIp))
    //    {
    //        throw new System.Exception("No network adapters with an IPv4 address in the system!");
    //    }

    //    for (ushort i = 1; i < 255; i++)
    //    {
    //        string ipToScan = $"{networkIp}.{i}";
    //        Task.Run(() =>
    //        {
    //            Client client = new Client();
    //            Debug.Log
    //            client.Connect(ipToScan);
    //        });
    //    }
    //}

    private void OnConnected(string ip)
    {
        Debug.Log($"Connected to {ip}");
        // Handle successful connection (e.g., add to server list UI)
    }

    private void OnConnectionFailed(string ip)
    {
        Debug.Log($"Failed to connect to {ip}");
        // Handle failed connection
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

    private void sendHandShake()
    {

    }
}
