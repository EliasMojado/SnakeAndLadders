using System.Collections;
using System.Collections.Generic;
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

    public GameObject PlayerPrefab => playerPrefab;
    public GameObject SnakePrefab => snakePrefab; 

    [Header("Prefabs")]
    [SerializeField] public GameObject playerPrefab;
    [SerializeField] public GameObject snakePrefab;

    private void Awake()
    {
        Singleton = this;

        if (SnakePrefab == null)
        {
            Debug.LogError("[GameLogic] SnakePrefab is not assigned.");
            return;
        }

        StartCoroutine(SpawnSnakesWhenServerReady());
    }

    private IEnumerator SpawnSnakesWhenServerReady()
    {
        while (NetworkManager.Singleton == null || NetworkManager.Singleton.Server == null)
        {
            Debug.Log("[GameLogic] Waiting for NetworkManager and Server to initialize...");
            yield return null;
        }

        Debug.Log("[GameLogic] Server is ready. Spawning snakes...");
        ushort newSnakeId = 1;

        Vector3 spawnPosition = new Vector3(5f,5f, 5f);
        Snake.Spawn(newSnakeId, spawnPosition);
        newSnakeId++;

        spawnPosition = new Vector3(13f, 5f, 5f);
        Snake.Spawn(newSnakeId, spawnPosition);
        newSnakeId++;

        
    }
}
