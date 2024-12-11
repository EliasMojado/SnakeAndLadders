using Riptide;
using Riptide.Utils;
using System;
using UnityEngine;

public enum ServerToClientId : ushort
{
    playerSpawned = 1,
    playerMovement,
    snakeSpawned,
    snakeUpdated,
    handshake,
    cheeseCaptured
}

public enum ClientToServerId : ushort
{
    name = 1,
    input,
    handshake,
    cheeseCaptured
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
            else if (_singleton != value)
            {
                Debug.Log($"{nameof(NetworkManager)} instance already set! {_singleton} != {value}");
                Destroy(value);
            }
        }
    }

    public Client Client { get; private set; }

    private void Awake()
    {
        Singleton = this;
    }

    private void Start()
    {
        Application.targetFrameRate = 60;
        RiptideLogger.Initialize(Debug.Log, Debug.Log, Debug.LogWarning, Debug.LogError, false);

        Client = new Client();
        Client.Connected += DidConnect;
        Client.ConnectionFailed += FailedToConnect;
        Client.Disconnected += DidDisconnect;
        Client.ClientDisconnected += PlayerLeft;
        Application.runInBackground = true;
    }

    private void FixedUpdate()
    {
        Client.Update();
    }

    private void OnApplicationQuit()
    {
        Client.Disconnect();
    }

    public void Connect(string ipAddress, ushort port)
    {
        Client.Connect($"{ipAddress}:{port}");
    }

    public void DidConnect(object sender, EventArgs e)
    {
        Debug.Log("Connected to server");
        ServerList.Singleton.getServerListInstance.SetActive(false);
        UIManager.Singleton.SendName();
        BackToLoby.Singleton.backToLobbyButton.gameObject.SetActive(true);
    }

    private void FailedToConnect(object sender, EventArgs e)
    {
        Debug.Log("Failed to connect to server");
        //UIManager.Singleton.BackToMain();
    }

    private void DidDisconnect(object sender, EventArgs e)
    {
        UIManager.Singleton.BackToMain();
    }

    private void PlayerLeft(object sender, ClientDisconnectedEventArgs e)
    {
        Destroy(Player.list[e.Id].gameObject);
    }
}
