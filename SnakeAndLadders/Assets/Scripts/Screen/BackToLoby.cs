using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BackToLoby : MonoBehaviour
{
    [SerializeField] public GameObject backToLobbyButton;
    private GameObject backToLobbyButtonInstance;

    private static BackToLoby _singleton;

    public static BackToLoby Singleton
    {
        get => _singleton;

        private set
        {
            if (_singleton == null)
                _singleton = value;
            else if (_singleton != value)
            {
                Debug.Log($"{nameof(BackToLoby)} instance already set! {_singleton} != {value}");
                Destroy(value);
            }
        }
    }

    private void Awake()
    {
        Singleton = this;
    }
    private void Start()
    {
        backToLobbyButton.gameObject.SetActive(false);
        backToLobbyButton.GetComponent<Button>().onClick.AddListener(BackToLobby);
    }

    public void BackToLobby()
    {
        Debug.Log("BackToMain");
        ServerList.Singleton.getServerListInstance.SetActive(true);
        backToLobbyButton.gameObject.SetActive(false);
        NetworkManager.Singleton.Client.Disconnect();
    }
}
