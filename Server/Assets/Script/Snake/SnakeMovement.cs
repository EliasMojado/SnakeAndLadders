using UnityEngine;

public class SnakeMovement : MonoBehaviour
{
    public float followDistance = 10f; // Distance within which the snake follows the player
    public float speed = 2f;           // Movement speed of the snake

    private Transform targetPlayer;    // The player the snake is currently targeting

    private Snake snakeComponent;

     private void Start()
    {
        // Get reference to Snake component
        snakeComponent = GetComponent<Snake>();
    }

    private void Update()
    {
        // Check for a target player and follow them
        if (targetPlayer != null)
        {
            float distance = Vector3.Distance(transform.position, targetPlayer.position);
            Debug.Log($"[SnakeMovement] Targeting player at position: {targetPlayer.position}, Distance: {distance}");

            if (distance <= followDistance)
            {
                FollowPlayer();
            }
            else
            {
                // If the player moves out of range, stop following
                Debug.Log("[SnakeMovement] Player out of range. Stopping follow.");
                targetPlayer = null;
            }
        }
        else
        {
            // Search for a nearby player
            Debug.Log("[SnakeMovement] No target player found. Searching for the nearest player...");
            FindNearestPlayer();
        }
    }

    private void FindNearestPlayer()
    {
        float closestDistance = followDistance;
        Transform nearestPlayer = null;

        // Iterate through all players in Player.list to find the closest one
        foreach (Player player in Player.list.Values)
        {
            // Get the player's position from the PlayerMovement component
            Vector3 playerPosition = player.Movement.transform.position;
            float distance = Vector3.Distance(transform.position, playerPosition);

            Debug.Log($"[SnakeMovement] Checking player {player.id} at position {player.Movement.transform.position}, Snake position: {transform.position}");
            Debug.Log($"[SnakeMovement] Distance to player: {distance}");

            if (distance < closestDistance)
            {
                closestDistance = distance;
                nearestPlayer = player.Movement.transform;
            }
        }

        // Assign the nearest player as the target
        if (nearestPlayer != null)
        {
            targetPlayer = nearestPlayer;
            Debug.Log($"[SnakeMovement] Nearest player found: {targetPlayer.position}");
        }
        else
        {
            Debug.Log("[SnakeMovement] No player within follow distance.");
        }
    }

    private void FollowPlayer()
    {
        // Move towards the player's position
        Vector3 direction = (targetPlayer.position - transform.position).normalized;
        transform.position += direction * speed * Time.deltaTime;

        Debug.Log($"[SnakeMovement] Following player at position {targetPlayer.position}, New position: {transform.position}");

        snakeComponent.UpdatePosition(transform.position);
    }
}
