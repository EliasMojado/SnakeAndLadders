using Riptide;
using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    private static UIManager _singleton;

    public static UIManager Singleton
    {
        get => _singleton;

        private set
        {
            if (_singleton == null)
                _singleton = value;
            else if (_singleton != value)
            {
                Debug.Log($"{nameof(UIManager)} instance already set! {_singleton} != {value}");
                Destroy(value);
            }
        }
    }

    [Header("Connect")]
    [SerializeField] private GameObject connectPanel;
    [SerializeField] private TMP_InputField usernameField;

    [SerializeField] private TMP_InputField addressField;

    [SerializeField] private TMP_InputField portField;

    private void Awake()
    { 
        Singleton = this;
    }

    public void ConnectClicked()
    {
        string ipAddress = addressField.text;
        if (!IsValidIPAddress(ipAddress))
        {
            Debug.LogError("Invalid IP address.");
            return;
        }

        if (!ushort.TryParse(portField.text, out ushort port))
        {
            Debug.LogError("Invalid port number.");
            return;
        }

        usernameField.interactable = false;
        connectPanel.SetActive(false);

        NetworkManager.Singleton.Connect(ipAddress, port);
    }

    public void DisconnectClicked() {
        NetworkManager.Singleton.Client.Disconnect();
    }

    public void BackToMain()
    {
        usernameField.interactable = true;
        connectPanel.SetActive(true);

        foreach (Player player in Player.list.Values)
        {
            Destroy(player.gameObject);
        }

        foreach (Snake snake in Snake.list.Values)
        {
            Destroy(snake.gameObject);
        }
    }

    public void SendName()
    {
        Message message = Message.Create(MessageSendMode.Reliable, ClientToServerId.name);
        message.AddString(usernameField.text);
        NetworkManager.Singleton.Client.Send(message);
    }

    private bool IsValidIPAddress(string ipAddress)
    {
        // Regular expression to validate IPv4 addresses
        string pattern = @"^(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\." +
                        @"(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\." +
                        @"(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\." +
                        @"(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$";

        return System.Text.RegularExpressions.Regex.IsMatch(ipAddress, pattern);
    }
}
