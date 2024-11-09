using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private float speed = 8;
    private Rigidbody2D body;
    private Vector3 characterScale = new Vector3(5, 5, 5);
    private Animator anim;
    private bool grounded;
    private bool isCollidingWithLeftWall;
    private bool isCollidingWithRightWall;

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
        if (!isCollidingWithRightWall && horizontalInput > 0.01f)
        {
            body.velocity = new Vector2(horizontalInput * speed, body.velocity.y);
            isCollidingWithLeftWall = false;

            // flip the sprite to the right
            transform.localScale = characterScale;
        }
        else if (!isCollidingWithLeftWall && horizontalInput < -0.01f)
        {
            body.velocity = new Vector2(horizontalInput * speed, body.velocity.y);
            isCollidingWithRightWall = false;

            // flip the sprite to the right
            transform.localScale = new Vector3(-characterScale.x, characterScale.y, characterScale.z);
        }

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
            if (contact.normal.y > 0.5f) // Normal points upwards for ground collision
            {
                if (collision.gameObject.CompareTag("Ground"))
                {
                    grounded = true;
                }
            }
            else if (contact.normal.x != 0) // If the collision is along the X-axis, it's a wall collision
            {
                if (collision.gameObject.CompareTag("Ground"))
                {
                    if (contact.normal.x > 0) // Collision on the left side
                    {
                        isCollidingWithLeftWall = true;
                        isCollidingWithRightWall = false;
                        wallCollisionDetected = true;

                    }
                    else if (contact.normal.x < 0) // Collision on the right side
                    {
                        isCollidingWithRightWall = true;
                        isCollidingWithLeftWall = false;
                        wallCollisionDetected = true;

                    }
                }
            }
        }

        if (!wallCollisionDetected)
        {
            isCollidingWithRightWall = false;
            isCollidingWithLeftWall = false;
        }
    }



}
