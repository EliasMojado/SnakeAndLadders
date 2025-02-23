using Riptide;
// using Unity.VisualScripting.Dependencies.Sqlite;
using System.Collections;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private Rigidbody2D body;
    private Vector3 characterScale = new Vector3(2, 2, 2);
    private Vector3 respawnPoint;
    private Animator anim;
    private float speed = 8;
    private float climbingSpeed = 4;
    private bool grounded;
    private bool isFalling = false;
    private bool isCollidingWithLeftWall;
    private bool isCollidingWithRightWall;
    private bool isClimbing;
    private bool[] inputs;
    private Constants.PlayerState currentState;

    private AudioSource footstepAudioSource;
    private AudioSource jumpAudioSource;
    private AudioSource climbingAudioSource;
    private AudioSource bgMusicAudioSource;
    private AudioSource snakeAudioSource;

    public AudioClip footstepClip;
    public AudioClip jumpClip1; // jump music for level 1
    public AudioClip jumpClip2; // jump music for level 2
    public AudioClip jumpClip3; // jump music for level 3
    public AudioClip jumpClip4; // jump music for level 4
    public AudioClip jumpClip5; // jump music for level 5
    public AudioClip climbingClip;
    public AudioClip bgMusicClip1; // bg music for levels 1 & 2; default
    public AudioClip bgMusicClip3; // bg music for levels 3 & 4
    public AudioClip bgMusicClip5; // bg music for level 5/final level
    public AudioClip snakeClip;

    private float level2YThreshold = 7f;
    private float level3YThreshold = 19f;
    private float level4YThreshold = 29f;
    private float level5YThreshold = 41f;
    private float lastY = 0f;

    private float audioFade = 1f;

    private void Start()
    {
        inputs = new bool[5];
        currentState = Constants.PlayerState.Idle;

        setBackgroundMusic(bgMusicClip1);
    }

    private void Awake()
    {
        // grab reference of rigid body
        body = GetComponent<Rigidbody2D>();
        // grab reference of animator
        anim = GetComponent<Animator>();

        // set up audio sources
        footstepAudioSource = gameObject.AddComponent<AudioSource>();
        footstepAudioSource.clip = footstepClip;
        footstepAudioSource.loop = true;

        jumpAudioSource = gameObject.AddComponent<AudioSource>();

        climbingAudioSource = gameObject.AddComponent<AudioSource>();
        climbingAudioSource.clip = climbingClip;
        climbingAudioSource.loop = true;

        bgMusicAudioSource = gameObject.AddComponent<AudioSource>();

        snakeAudioSource = gameObject.AddComponent<AudioSource>();
        snakeAudioSource.clip = snakeClip;
        snakeAudioSource.playOnAwake = false;
    }

    private void Update()
    {
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        // audio: update player's vertical position
        float currentY = transform.position.y;

        // check if player has moved to a new level
        if (currentY >= level5YThreshold && lastY < level5YThreshold)
        {
            StartCoroutine(FadeMusic(bgMusicClip5));
        }
        else if (currentY >= level3YThreshold && lastY < level3YThreshold)
        {
            StartCoroutine(FadeMusic(bgMusicClip3));
        }
        else if (currentY < level3YThreshold && lastY >= level3YThreshold)
        {
            StartCoroutine(FadeMusic(bgMusicClip1));
        }
        else if (currentY < level5YThreshold && lastY >= level5YThreshold)
        {
            StartCoroutine(FadeMusic(bgMusicClip3));
        }

        lastY = currentY;

        HandleMovement(horizontalInput, verticalInput);
    }

    private void HandleMovement(float horizontalInput, float verticalInput)
    {
        if (Input.GetKey(KeyCode.Space) && grounded){
            Jump();
            currentState = Constants.PlayerState.Jumping;
            if (climbingAudioSource.isPlaying){
                climbingAudioSource.Stop();
            }
        } else if (isClimbing){
            grounded = true;
            // Allow vertical movement when climbing
            body.velocity = new Vector2(body.velocity.x, verticalInput * climbingSpeed);
            // Disable horizontal movement while climbing
            anim.SetBool("run", false);
            currentState = Constants.PlayerState.Climbing;
            if (isClimbing) {
                if (!climbingAudioSource.isPlaying) {
                    climbingAudioSource.Play();
                }
            } else {
                if (climbingAudioSource.isPlaying) {
                    climbingAudioSource.Stop();
                }
            }
        } else if (grounded){
            currentState = Constants.PlayerState.Idle;
            if (climbingAudioSource.isPlaying){
                climbingAudioSource.Stop();
            }
        }
        if (!isCollidingWithRightWall && horizontalInput > 0.01f)
        {
            body.velocity = new Vector2(horizontalInput * speed, body.velocity.y);
            isCollidingWithLeftWall = false;

            // flip the sprite to the right
            transform.localScale = characterScale;

            if (currentState != Constants.PlayerState.Jumping)
                currentState = Constants.PlayerState.Running;
        }
        else if (!isCollidingWithLeftWall && horizontalInput < -0.01f)
        {
            body.velocity = new Vector2(horizontalInput * speed, body.velocity.y);
            isCollidingWithRightWall = false;

            // flip the sprite to the right
            transform.localScale = new Vector3(-characterScale.x, characterScale.y, characterScale.z);

            if (currentState != Constants.PlayerState.Jumping)
                currentState = Constants.PlayerState.Running;
        }

        // Play footstep audio when running
        if (horizontalInput != 0 && grounded){
            if (!footstepAudioSource.isPlaying){
                footstepAudioSource.Play();
            }
        } else {
            footstepAudioSource.Stop();
        }

        // if horizontalInput is not 0 (stationary), trigger run animation
        anim.SetBool("run", horizontalInput != 0);

        // if colliding with floor, set grounded
        anim.SetBool("grounded", grounded);

        // Get and use the player's position
        Vector3 playerPosition = transform.position;
        SendInput(new Vector2(playerPosition.x, playerPosition.y), currentState);
    }

    #region Messages

    private void SendInput(Vector2 position, Constants.PlayerState state)
    {
        Message message = Message.Create(MessageSendMode.Unreliable, ClientToServerId.input);
        message.AddBools(inputs, false);
        message.AddVector2(position);
        message.AddInt((int)state); // Send player state as an integer
        NetworkManager.Singleton.Client.Send(message);
    }

    #endregion
    private void Jump()
    {
        if (transform.position.y >= level5YThreshold)
        {
            jumpAudioSource.clip = jumpClip5; // rooftop
        }
        else if (transform.position.y >= level4YThreshold)
        {
            jumpAudioSource.clip = jumpClip4; // vent
        }
        else if (transform.position.y >= level3YThreshold)
        {
            jumpAudioSource.clip = jumpClip3; // street
        }
        else if (transform.position.y >= level2YThreshold)
        {
            jumpAudioSource.clip = jumpClip2; // sewer
        }
        else
        {
            jumpAudioSource.clip = jumpClip1; // underground
        }
        body.velocity = new Vector2(body.velocity.x, speed * 1.2f);
        anim.SetTrigger("jump");
        grounded = false;
        jumpAudioSource.Play();
    }

    private void setBackgroundMusic(AudioClip clip)
    {
        if (bgMusicAudioSource.clip != clip)
        {
            bgMusicAudioSource.Stop();
            bgMusicAudioSource.clip = clip;
            bgMusicAudioSource.loop = true;
            bgMusicAudioSource.Play();
        }
    }

    private IEnumerator FadeMusic(AudioClip newClip)
    {
        // Fade out the current music
        float startVolume = bgMusicAudioSource.volume;
        float elapsedTime = 0f;

        while (elapsedTime < audioFade)
        {
            bgMusicAudioSource.volume = Mathf.Lerp(startVolume, 0f, elapsedTime / audioFade);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        bgMusicAudioSource.volume = 0f;
        bgMusicAudioSource.Stop();

        // Now switch the track and fade it in
        setBackgroundMusic(newClip);

        // Fade in the new music
        elapsedTime = 0f;
        while (elapsedTime < audioFade)
        {
            bgMusicAudioSource.volume = Mathf.Lerp(0f, 0.8f, elapsedTime / audioFade);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        if (newClip == bgMusicClip3)
        {
            bgMusicAudioSource.volume = 0.3f;
        }
        else
        {
            bgMusicAudioSource.volume = 0.8f;
        }
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

    private void Respawn()
    {
        // After falling, respawn the player at the respawn point
        transform.position = respawnPoint;

        // Reset the player state, collider, and velocity if necessary
        grounded = true;  // Assume the player is grounded when respawned
        body.velocity = Vector2.zero;  // Stop any ongoing velocity

        isFalling = false; // Reset the falling state
    }

    // When entering the ladder trigger area
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Ladder"))
        {
            // Enable climbing when player touches ladder
            isClimbing = true;
        }else if (other.CompareTag("Snake")){
            Debug.Log("Snake Collision!");
            snakeAudioSource.Play();
            Respawn();
        }else if (other.CompareTag("Void")){
            Debug.Log("You fell into the void!");
            Respawn();
        }else if (other.CompareTag("Respawn")){
            Debug.Log("Respawn Point Set!");
            respawnPoint = other.transform.position;
        }else if (other.CompareTag("cheese")){
            Debug.Log("Cheese Collected!");
            CaptureCheese(other.transform.position);
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

    public void CaptureCheese(Vector3 cheesePosition)
    {
        Message message = Message.Create(MessageSendMode.Reliable, (ushort)ClientToServerId.cheeseCaptured);
        message.AddUShort(1); // Send the player's ID
        message.AddVector3(cheesePosition); // Send the cheese position

        NetworkManager.Singleton.Client.Send(message);

        Debug.Log("Cheese captured event sent to the server.");
    }

}
