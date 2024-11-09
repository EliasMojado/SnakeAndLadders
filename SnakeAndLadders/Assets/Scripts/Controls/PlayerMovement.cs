using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private float speed = 8;
    private Rigidbody2D body;
    private Vector3 characterScale = new Vector3(5, 5, 5);
    private Animator anim;
    private bool grounded;

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

        body.velocity = new Vector2(horizontalInput * speed, body.velocity.y);
        
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
        Debug.Log("Player Position: " + playerPosition);
    }

    private void Jump()
    {
        body.velocity = new Vector2(body.velocity.x, speed);
        anim.SetTrigger("jump");
        grounded = false;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {   
        // add another check if it is below
        if(collision.gameObject.tag == "Ground")
            grounded = true;
    }
}
