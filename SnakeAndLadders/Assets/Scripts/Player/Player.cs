using Riptide;
using Riptide.Transports;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Player : MonoBehaviour
{
    public static Dictionary<ushort, Player> list = new Dictionary<ushort, Player>();

    public ushort id { get; private set; }
    public bool isLocal { get; private set; }

    private Vector3 characterScale = new Vector3(2.3f, 2.3f, 2.3f);

    public string username;

    private Vector3 targetPosition;
    private Vector3 previousPosition;

    private float interpolationSpeed = 30f;

    private void Start()
    {
        targetPosition = transform.position;
        previousPosition = transform.position;
    }

    private void Update()
    {
        if (!isLocal)
        {
            transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * interpolationSpeed);
        }
    }

    private Animator anim;
    private Rigidbody2D body;
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        anim = GetComponent<Animator>(); 
        body = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnDestroy()
    {
        list.Remove(id);
    }

    private void Move(Vector2 position)
    {
        targetPosition = new Vector3(position.x, position.y, 0);
    }

    public static void Spawn(ushort id, string username, Vector3 position)
    {
        Player player;
        if (id == NetworkManager.Singleton.Client.Id)
        {
            player = Instantiate(GameLogic.Singleton.LocalPlayerPrefab, position, Quaternion.identity).GetComponent<Player>();
            player.isLocal = true;
            Camera.main.GetComponent<CameraController>().player = player.transform;
        }
        else
        {
            player = Instantiate(GameLogic.Singleton.PlayerPrefab, position, Quaternion.identity).GetComponent<Player>();
            player.isLocal = false;
        }

        player.name = $"Player {id} ({(string.IsNullOrEmpty(username) ? "Guest" : username)})";
        player.id = id;
        player.username = username;

        Transform canvasTransform = player.transform.GetChild(0); // Assuming Canvas is the first child
        Transform usernameTransform = canvasTransform.GetChild(0); // Assuming the TextMeshProUGUI is the first child of Canvas
        TextMeshProUGUI textComponent = usernameTransform.GetComponent<TextMeshProUGUI>();
        if (textComponent != null)
        {
            textComponent.text = username;
        }
        else
        {
            Debug.LogError("TextMeshProUGUI component not found on the 'username' object.");
        }

        list.Add(id, player);
    }

    private void UpdateState(int state)
    {
        // Cast the state integer to the PlayerState enum
        Constants.PlayerState playerState = (Constants.PlayerState)state;
        FlipSprite();

        // Update animations or behaviors based on the state received from the server
        switch (playerState)
        {
            case Constants.PlayerState.Idle:
                anim.SetBool("run", false);  // Ensure the "run" animation is false
                // anim.SetBool("climb", false); // Make sure the climbing animation is off
                anim.SetBool("grounded", true); // Ensure the grounded state is true when idle
                break;

            case Constants.PlayerState.Running:
                anim.SetBool("run", true);   // Set "run" animation to true when running
                // anim.SetBool("climb", false); // Ensure climbing animation is off
                anim.SetBool("grounded", true); // Ensure the grounded state is true when running
                break;

            case Constants.PlayerState.Jumping:
                anim.SetTrigger("jump");   // Trigger the "jump" animation once
                anim.SetBool("run", false); // Stop the "run" animation while jumping
                // anim.SetBool("climb", false); // Ensure climbing animation is off
                anim.SetBool("grounded", false); // Set grounded to false while in the air
                break;

            case Constants.PlayerState.Climbing:
                // anim.SetBool("climb", true); // Set climbing animation to true when climbing
                anim.SetBool("run", false);  // Stop the "run" animation while climbing
                anim.SetBool("grounded", true); // Ensure the grounded state is true while climbing
                break;

            default:
                anim.SetBool("run", false);  // Stop running animation in case of undefined state
                // anim.SetBool("climb", false); // Stop climbing animation in case of undefined state
                anim.SetBool("grounded", false); // Set grounded to false in case of undefined state
                break;
        }
    }

    private float positionUpdateDelay = 0.5f; // Delay in seconds
    private float positionUpdateTimer = 0f;   // Timer to track delay

    private void FlipSprite()
    {
        if (!isLocal) // Only update for non-local players
        {
            Vector3 direction = transform.position - previousPosition; // Calculate movement direction
            
            Debug.Log($"Current Position: {transform.position}, Previous Position: {previousPosition}, Direction: {direction}");
            
            if (direction.x > 0) // Moving right
            {
                spriteRenderer.flipX = true;
                Debug.Log("Moving Right: spriteRenderer.flipX = false");
            }
            else if (direction.x < 0) // Moving left
            {
                spriteRenderer.flipX = false;
                Debug.Log("Moving Left: spriteRenderer.flipX = true");
            }

            // Increment the timer
            positionUpdateTimer += Time.deltaTime;

            // Update previous position only after the delay
            if (positionUpdateTimer >= positionUpdateDelay)
            {
                previousPosition = transform.position;
                positionUpdateTimer = 0f; // Reset the timer
            }
        }
    }

    [MessageHandler((ushort)ServerToClientId.playerSpawned)]
    private static void SpawnPlayer(Message message)
    {
        Spawn(message.GetUShort(), message.GetString(), message.GetVector3());
    }

    [MessageHandler((ushort)ServerToClientId.playerMovement)]
    private static void PlayerMovement(Message message)
    {
        ushort playerId = message.GetUShort(); // Get the player ID
        Vector2 position = message.GetVector2(); // Get the position
        int state = message.GetInt(); // Get the player state (Idle, Walking, etc.)

        if (list.TryGetValue(playerId, out Player player))
        {

            if (player.isLocal)
            {
                return;
            }
            
            // Update the player's position
            player.Move(position);

            // Update the player's state (like animations)
            player.UpdateState(state);
        }
    }
}