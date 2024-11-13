using Riptide;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private Rigidbody2D body;
    private Vector3 characterScale = new Vector3(5, 5, 5);
    private Animator anim;
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
        if(Input.GetKey(KeyCode.W))
        {
            inputs[0] = true;
        }

        if (Input.GetKey(KeyCode.A))
        {
            inputs[1] = true;
        }

        if (Input.GetKey(KeyCode.S))
        {
            inputs[2] = true;
        }

        if (Input.GetKey(KeyCode.D))
        {
            inputs[3] = true;
        }

        if (Input.GetKey(KeyCode.Space))
        {
            inputs[4] = true;
        }
    }

    private void FixedUpdate()
    {
        SendInput();
        for(int i = 0; i < inputs.Length; i++)
        {
            inputs[i] = false;
        }
    }

    #region Messages

    private void SendInput()
    {
        Message message = Message.Create(MessageSendMode.Unreliable, ClientToServerId.input);
        message.AddBools(inputs, false);
        NetworkManager.Singleton.Client.Send(message);
    }

    #endregion
    //private void Jump()
    //{
    //    body.velocity = new Vector2(body.velocity.x, speed * 1.2f);
    //    anim.SetTrigger("jump");
    //    grounded = false;
    //}

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
