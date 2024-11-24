using Riptide;
using UnityEngine;
using System.Collections.Generic;

public class Snake : MonoBehaviour
{
    public static Dictionary<ushort, Snake> list = new Dictionary<ushort, Snake>();

    public ushort id;
    public Vector3 position;

    private void OnDestroy()
    {
        list.Remove(id);
    }

    public static void Spawn(ushort id, Vector3 spawnPosition)
    {
        // Log the intent to spawn a snake
        Debug.Log($"[Snake.Spawn] Attempting to spawn a snake with ID: {id} at position: {spawnPosition}");

        // Instantiate the snake object and assign properties
        Snake snake = Instantiate(GameLogic.Singleton.SnakePrefab, spawnPosition, Quaternion.identity).GetComponent<Snake>();
        if (snake == null)
        {
            Debug.LogError("[Snake.Spawn] Failed to instantiate SnakePrefab. Please check if the prefab is assigned correctly in GameLogic.");
            return;
        }

        // Assign properties to the snake instance
        snake.id = id;
        snake.position = spawnPosition;
        Debug.Log($"[Snake.Spawn] Successfully instantiated snake with ID: {id}");

        // Add the snake to the dictionary
        if (list.ContainsKey(id))
        {
            Debug.LogError($"[Snake.Spawn] Snake with ID {id} already exists in the list. This may indicate an ID conflict.");
            return;
        }
        list.Add(id, snake);
        Debug.Log($"[Snake.Spawn] Snake with ID {id} added to the list. Total snakes: {list.Count}");

        // Notify all players about the new snake
        snake.SendSpawned();
        Debug.Log($"[Snake.Spawn] Spawn notification sent for Snake ID: {id}");
    }
    
    private void SendSpawned()
    {
        if (NetworkManager.Singleton == null)
        {
            Debug.LogError("[Snake.SendSpawned] NetworkManager.Singleton is null!");
            return;
        }

        if (NetworkManager.Singleton.Server == null)
        {
            Debug.LogError("[Snake.SendSpawned] NetworkManager.Singleton.Server is null!");
            return;
        }

        Debug.Log("[Snake.SendSpawned] NetworkManager and Server are initialized. Sending spawn data...");
        NetworkManager.Singleton.Server.SendToAll(AddSpawnData(Message.Create(MessageSendMode.Reliable, ServerToClientId.snakeSpawned)));
    }

    public static void SendAllSnakesTo(ushort clientId)
    {
        foreach (Snake snake in list.Values)
        {
            NetworkManager.Singleton.Server.Send(snake.AddSpawnData(
                Message.Create(MessageSendMode.Reliable, ServerToClientId.snakeSpawned)), clientId);
        }

        Debug.Log($"[Snake] Sent all {list.Count} snakes to client {clientId}");
    }


    private Message AddSpawnData(Message message)
    {
        message.AddUShort(id);
        message.AddVector3(position);
        return message;
    }

    public void UpdatePosition(Vector3 newPosition)
    {
        position = newPosition;
        transform.position = newPosition;

        // Notify all clients about the updated position
        SendUpdatedPosition();
    }

    private void SendUpdatedPosition()
    {
        NetworkManager.Singleton.Server.SendToAll(AddUpdateData(Message.Create(MessageSendMode.Unreliable, ServerToClientId.snakeUpdated)));
    }

    private Message AddUpdateData(Message message)
    {
        message.AddUShort(id);
        message.AddVector3(position);
        return message;
    }
}
