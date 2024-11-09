using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private float speed = 8;
    private Rigidbody2D body;
    private Vector3 characterScale = new Vector3(5, 5, 5);
    private Animator anim;
    private bool grounded;
    private bool isCollidingWithWall;

    private void Awake()
    {
        // grab reference of rigid body
        body = GetComponent<Rigidbody2D>();
        // grab reference of animator
        anim = GetComponent<Animator>();
    }

    private void Update()
    {
        float horizontalInput = Input.GetAxis("Horizontal");

        // If there's no wall collision, allow horizontal movement
        if (!isCollidingWithWall)
        {
            body.velocity = new Vector2(horizontalInput * speed, body.velocity.y);
        }

        // sprite flipping based on directions
        if (horizontalInput > 0.01f)                    // player is moving to the right
            transform.localScale = characterScale;
        else if (horizontalInput < -0.01f)              // player is moving to the left
            transform.localScale = new Vector3(-characterScale.x, characterScale.y, characterScale.z);

        if (Input.GetKey(KeyCode.Space) && grounded)
            Jump();

        // if horizontalInput is not 0 (stationary), trigger run animation
        anim.SetBool("run", horizontalInput != 0);
        
        // if colliding with floor, set grounded
        anim.SetBool("grounded", grounded);

        // Get and use the player's position
        Vector3 playerPosition = transform.position;
    }

    private void Jump()
    {
        body.velocity = new Vector2(body.velocity.x, speed);
        anim.SetTrigger("jump");
        grounded = false;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        bool wallCollisionDetected = false;

        // Loop through each contact point in the collision
        foreach (ContactPoint2D contact in collision.contacts)
        {
            // Check the normal of the collision to determine if it's below the player (ground)
            if (contact.normal.y > 0.5f) // Normal points upwards for ground collision
            {
                if (collision.gameObject.CompareTag("Ground"))
                {
                    Debug.Log("Ground collision detected");

                    grounded = true;  // The player is grounded, so allow jumping
                }
            }
            else if (contact.normal.x != 0) // If the collision is along the X-axis, it's a wall collision
            {
                if (collision.gameObject.CompareTag("Ground"))
                {
                    Debug.Log("Wall collision detected");

                    grounded = false;
                    isCollidingWithWall = true;
                    wallCollisionDetected = true;
                }
            }
        }

        if (!wallCollisionDetected)
        {
            isCollidingWithWall = false;
        }
    }
}
