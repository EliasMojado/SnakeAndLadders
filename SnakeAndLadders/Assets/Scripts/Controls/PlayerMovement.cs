using Riptide;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private Rigidbody2D body;
    private Vector3 characterScale = new Vector3(5, 5, 5);
    private Animator anim;
    private float speed = 8;
    private float climbingSpeed = 4;
    private bool grounded;
    private bool isCollidingWithLeftWall;
    private bool isCollidingWithRightWall;
    private bool isClimbing;
    private bool[] inputs;
    private void Start()
    {
        inputs = new bool[5];
    }
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
        float verticalInput = Input.GetAxis("Vertical");

        if (Input.GetKey(KeyCode.Space) && grounded)
            Jump();

        if (isClimbing)
        {
            grounded = true;
            // Allow vertical movement when climbing
            body.velocity = new Vector2(body.velocity.x, verticalInput * climbingSpeed);
            // Disable horizontal movement while climbing
            anim.SetBool("run", false);
        }

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

        // if horizontalInput is not 0 (stationary), trigger run animation
        anim.SetBool("run", horizontalInput != 0);

        // if colliding with floor, set grounded
        anim.SetBool("grounded", grounded);

        // Get and use the player's position
        Vector3 playerPosition = transform.position;

        SendInput(new Vector2(playerPosition.x, playerPosition.y));
    }

    #region Messages

    private void SendInput(Vector2 position)
    {
        Message message = Message.Create(MessageSendMode.Unreliable, ClientToServerId.input);
        message.AddBools(inputs, false);
        message.AddVector2(position);
        NetworkManager.Singleton.Client.Send(message);
    }

    #endregion
    private void Jump()
    {
        body.velocity = new Vector2(body.velocity.x, speed * 1.2f);
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

    // When entering the ladder trigger area
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Ladder"))
        {
            // Enable climbing when player touches ladder
            isClimbing = true;
        }
    }

    // When exiting the ladder trigger area
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Ladder"))
        {
            // Disable climbing when player exits ladder
            isClimbing = false;
        }
    }
}
