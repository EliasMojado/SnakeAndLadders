using UnityEngine;

public class SnakeMovement : MonoBehaviour
{
    private float followDistance = 5f; // Distance within which the snake follows the player
    public float speed = 4f;           // Movement speed of the snake
    public float jumpForce = 10f;       // Force applied for jumping
    public float groundCheckDistance = 0.5f; // Distance to check if the snake is grounded

    private Transform targetPlayer;    // The player the snake is currently targeting
    private Snake snakeComponent;      // Reference to the Snake component
    private Rigidbody2D rb;              // Rigidbody for the snake
    private bool isGrounded;           // To check if the snake is on the ground

    private void Start()
    {
        // Get reference to Snake component
        snakeComponent = GetComponent<Snake>();

        // Ignore collisions between all snakes
        foreach (Snake otherSnake in Snake.list.Values)
        {
            if (otherSnake != this)
            {
                Physics2D.IgnoreCollision(GetComponent<Collider2D>(), otherSnake.GetComponent<Collider2D>());
            }
        }
        
        // Get the Rigidbody component
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError("[SnakeMovement] Rigidbody is missing! Add a Rigidbody component to the snake object.");
        }
    }

    private void Update()
    {
        // Check for a target player and follow them
        if (targetPlayer != null)
        {
            float distance = Vector2.Distance(transform.position, targetPlayer.position);
            Debug.Log($"[SnakeMovement] Targeting player at position: {targetPlayer.position}, Distance: {distance}");

            if (distance < followDistance)
            {   
                // If the player is within range, follow them
                Debug.Log("[SnakeMovement] Player within range. Distance: " + distance + " Follow Distance: " + followDistance);
                FollowPlayer();
            }
            else
            {
                // If the player moves out of range, stop following
                Debug.Log("[SnakeMovement] Player out of range. Distance: " + distance);
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

    private void OnCollisionEnter2D(Collision2D collision)
    {
        foreach (ContactPoint2D contact in collision.contacts)
        {
            if (contact.normal.y > 0.5f && collision.gameObject.CompareTag("Ground"))
            {
                isGrounded = true;
                break;
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = false;
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
            float distance = Vector2.Distance(transform.position, playerPosition);

            // Debug.Log($"[SnakeMovement] Checking player {player.id} at position {player.Movement.transform.position}, Snake position: {transform.position}");
            // Debug.Log($"[SnakeMovement] Distance to player: {distance}");

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
        // Determine the direction to move
        Vector2 direction = (targetPlayer.position - transform.position).normalized;
        direction.y = 0; // Ignore vertical movement for horizontal following

        // Ensure consistent ground check position relative to the snake
        float checkOffsetX = direction.x > 0 ? 0.5f : -0.5f; // Check 1 unit to the left or right based on direction
        float checkOffsetY = -0.5f; // Slightly below the snake's position for better accuracy

        Vector2 groundCheckPosition = new Vector2(transform.position.x + checkOffsetX, transform.position.y + checkOffsetY);

        // Perform the raycast
        RaycastHit2D hit = Physics2D.Raycast(groundCheckPosition, Vector2.down, groundCheckDistance);
        Debug.Log($"[SnakeMovement] GroundCheckPosition: {groundCheckPosition}, Direction: {direction}, SnakePosition: {transform.position}");

        // Check if the raycast hit something
        if (hit.collider != null)
        {
            Debug.Log($"[SnakeMovement] Raycast hit: {hit.collider.name}, Tag: {hit.collider.tag}");

            // Check if the object has the "Ground" tag
            if (!hit.collider.CompareTag("Ground"))
            {
                Debug.Log("[SnakeMovement] Object hit, but it's not 'Ground'.");
                rb.velocity = new Vector2(0, rb.velocity.y); // Stop horizontal movement
                return;
            }
        }
        else
        {
            Debug.Log("[SnakeMovement] Raycast did not hit any object.");
            rb.velocity = new Vector2(0, rb.velocity.y); // Stop horizontal movement
            return;
        }

        if(direction.x > 0){
            rb.velocity = new Vector2(speed, rb.velocity.y);
        }else{
            rb.velocity = new Vector2(-speed, rb.velocity.y);
        }

        // Trigger jump if player is above the snake and horizontally aligned
        if (Mathf.Abs(targetPlayer.position.x - transform.position.x) < 1f && targetPlayer.position.y > transform.position.y + 0.5f)
        {
            Debug.Log($"[SnakeMovement] Jump triggered! Player position: {targetPlayer.position}, Snake position: {transform.position}");
            Jump();
        }

        snakeComponent.UpdatePosition(transform.position);
    }

    private void Jump()
    {   
        if(isGrounded){
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            isGrounded = false;
        }else{
            Debug.Log("[SnakeMovement] Snake is not grounded. Cannot jump.");
        }
    }
}
