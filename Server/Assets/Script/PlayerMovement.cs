using Riptide;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private Player player;
    [SerializeField] private float Speed;
    [SerializeField] private float ClimbSpeed;
    [SerializeField] private CharacterController controller;

    private float speed = 1;
    private float climbingSpeed = 4;
    private bool[] inputs;

    private void OnValidate()
    {
        if (controller == null)
            controller = GetComponent<CharacterController>();
        if (player == null)
            player = GetComponent<Player>();

        Initialize();
    }

    private void Start()
    {
        Initialize();
        inputs = new bool[5];
    }

    private void Move(Vector2 inputDirection)
    {
        Vector2 moveDirection = new Vector2(inputDirection.x, inputDirection.y);
        moveDirection *= speed;
        //float horizontalInput = Input.GetAxis("Horizontal");
        //float verticalInput = Input.GetAxis("Vertical");

        //if (Input.GetKey(KeyCode.Space) && grounded)
        //    Jump();

        //if (isClimbing)
        //{
        //    grounded = true;
        //    // Allow vertical movement when climbing
        //    body.velocity = new Vector2(body.velocity.x, verticalInput * climbingSpeed);
        //    // Disable horizontal movement while climbing
        //    anim.SetBool("run", false);
        //}

        //if (!isCollidingWithRightWall && horizontalInput > 0.01f)
        //{
        //    body.velocity = new Vector2(horizontalInput * speed, body.velocity.y);
        //    isCollidingWithLeftWall = false;

        //    // flip the sprite to the right
        //    transform.localScale = characterScale;
        //}
        //else if (!isCollidingWithLeftWall && horizontalInput < -0.01f)
        //{
        //    body.velocity = new Vector2(horizontalInput * speed, body.velocity.y);
        //    isCollidingWithRightWall = false;

        //    // flip the sprite to the right
        //    transform.localScale = new Vector3(-characterScale.x, characterScale.y, characterScale.z);
        //}

        //// if horizontalInput is not 0 (stationary), trigger run animation
        //anim.SetBool("run", horizontalInput != 0);

        //// if colliding with floor, set grounded
        //anim.SetBool("grounded", grounded);

        //// Get and use the player's position
        //Vector3 playerPosition = transform.position;

        controller.Move(moveDirection);
        Debug.Log(transform.position);
        SendMovement();
    }

    private void FixedUpdate()
    {
        Vector2 inputDirection = Vector2.zero;
        if (inputs[0]) inputDirection.y += 1;
        if (inputs[1]) inputDirection.x -= 1;
        if (inputs[2]) inputDirection.y -= 1;
        if (inputs[3]) inputDirection.x += 1;
        Move(inputDirection);
    }

    public void SetInput(bool[] inputs)
    {
        this.inputs = inputs;
    }

    private void SendMovement()
    {
        Message message = Message.Create(MessageSendMode.Unreliable, ServerToClientId.playerMovement);
        message.AddUShort(player.id);
        message.AddVector2(transform.position);
        NetworkManager.Singleton.Server.SendToAll(message);
    }

    private void Initialize()
    {
        // Initialize the player movement
    }
}
