using Riptide;
using System.Collections.Generic;
using UnityEngine;

public class Snake : MonoBehaviour
{
    public static Dictionary<ushort, Snake> list = new Dictionary<ushort, Snake>();

    public ushort id;
    public Vector3 position;

    private void OnDestroy()
    {
        Debug.Log($"Destroying snake with ID: {id}");
        list.Remove(id);
    }

    public static void Spawn(ushort id, Vector3 spawnPosition)
    {
        // Debug log to verify spawn is called
        Debug.Log($"Spawning snake with ID: {id} at position: {spawnPosition}");

        // Instantiate the snake object and assign properties
        GameObject prefab = GameLogic.Singleton.SnakePrefab;
        if (prefab == null)
        {
            Debug.LogError("SnakePrefab is not assigned in GameLogic!");
            return;
        }

        Snake snake = Instantiate(prefab, spawnPosition, Quaternion.identity).GetComponent<Snake>();
        if (snake == null)
        {
            Debug.LogError("Snake prefab does not have the Snake script attached!");
            return;
        }

        snake.id = id;
        snake.position = spawnPosition;

        // Add to the list of snakes
        if (list.ContainsKey(id))
        {
            Debug.LogWarning($"A snake with ID: {id} already exists! Overwriting...");
        }
        list[id] = snake;

        Debug.Log($"Snake with ID: {id} successfully spawned.");
    }

    public void UpdatePosition(Vector3 newPosition)
    {
        Debug.Log($"Updating snake position: ID = {id}, New Position = {newPosition}");
        position = newPosition;
        transform.position = newPosition;
    }

    // Message handler to spawn a new snake
    [MessageHandler((ushort)ServerToClientId.snakeSpawned)]
    private static void SnakeSpawned(Message message)
    {
        ushort snakeId = message.GetUShort();  // Get the snake ID
        Vector3 spawnPosition = message.GetVector3();  // Get the spawn position for the snake

        Debug.Log($"SnakeSpawned message received: ID = {snakeId}, Spawn Position = {spawnPosition}");
        Spawn(snakeId, spawnPosition);
    }

    // Message handler to update the snake's position
    [MessageHandler((ushort)ServerToClientId.snakeUpdated)]
    private static void SnakeUpdated(Message message)
    {
        ushort snakeId = message.GetUShort();  // Get the snake ID
        Vector3 newPosition = message.GetVector3();  // Get the updated position for the snake

        Debug.Log($"SnakeUpdated message received: ID = {snakeId}, New Position = {newPosition}");

        // Update the snake's position on the client
        if (list.TryGetValue(snakeId, out Snake snake))
        {
            snake.UpdatePosition(newPosition);
        }
        else
        {
            Debug.LogWarning($"No snake with ID: {snakeId} exists to update.");
        }
    }
}
