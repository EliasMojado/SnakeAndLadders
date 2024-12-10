using Riptide;
using UnityEngine;
using System.Collections.Generic;

public class Snake : MonoBehaviour
{
    // Static dictionary to track all snake instances
    public static Dictionary<ushort, Snake> list = new Dictionary<ushort, Snake>();

    // Unique identifier for each snake
    public ushort id;

    // Handle removal of the snake from the dictionary when destroyed
    private void OnDestroy()
    {
        list.Remove(id);
    }

    // Spawns a new snake at the given position
    public static void Spawn(ushort id, Vector3 spawnPosition)
    {
        Debug.Log($"[Snake.Spawn] Attempting to spawn a snake with ID: {id} at position: {spawnPosition}");

        // Instantiate the snake object and assign its properties
        Snake snake = Instantiate(GameLogic.Singleton.SnakePrefab, spawnPosition, Quaternion.identity).GetComponent<Snake>();
        if (snake == null)
        {
            Debug.LogError("[Snake.Spawn] Failed to instantiate SnakePrefab. Please check if the prefab is assigned correctly in GameLogic.");
            return;
        }

        // Assign the unique ID to the snake
        snake.id = id;
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

    // Sends spawn data to all players
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

    // Sends all snakes to a specific client
    public static void SendAllSnakesTo(ushort clientId)
    {
        foreach (Snake snake in list.Values)
        {
            NetworkManager.Singleton.Server.Send(snake.AddSpawnData(
                Message.Create(MessageSendMode.Reliable, ServerToClientId.snakeSpawned)), clientId);
        }

        Debug.Log($"[Snake] Sent all {list.Count} snakes to client {clientId}");
    }

    // Adds spawn data to a message
    private Message AddSpawnData(Message message)
    {
        message.AddUShort(id);
        message.AddVector3(transform.position); // Use transform.position for position
        return message;
    }

    // Updates the snake's position
    public void UpdatePosition(Vector3 newPosition)
    {
        transform.position = newPosition; // Update the position directly

        // Notify all clients about the updated position
        SendUpdatedPosition();
    }

    // Sends position update to all clients
    private void SendUpdatedPosition()
    {
        NetworkManager.Singleton.Server.SendToAll(AddUpdateData(Message.Create(MessageSendMode.Unreliable, ServerToClientId.snakeUpdated)));
    }

    // Adds updated position data to a message
    private Message AddUpdateData(Message message)
    {
        message.AddUShort(id);
        message.AddVector3(transform.position); // Use transform.position for position
        return message;
    }
}
