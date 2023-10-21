using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private Rigidbody2D playerRigidbody;            // Reference to the Rigidbody2D component of the player.
    private SpriteRenderer playerSpriteRenderer;    // Reference to the SpriteRenderer component of the player.
    private Animator playerAnimator;                // Reference to the Animator component of the player.

    public float moveSpeed = 8.0f;            // Player movement speed.
    public float jumpForce = 200.0f;          // Force applied to the player for jumping.
    public float xBoundary = 11.8f;           // Horizontal boundary for constraining player movement.

    public float doubleJumpForceRatio = 0.5f;       // Relative intensity of the force for the second jump.

    public bool isOnSurface;                        // Boolean to check if the player is on the ground.
    public bool doubleJumpUsed;                     // Boolean to check if the player has jumped twice already.

    private float horizontalInput;                  // Stores the horizontal input value.

    // Start is called before the first frame update
    void Start()
    {
        // Get the reference to the Rigidbody2D component attached to the player object.
        playerRigidbody = GetComponent<Rigidbody2D>();

        // Get the reference to the SpriteRenderer component attached to the player object.
        playerSpriteRenderer = GetComponent<SpriteRenderer>();

        // Get the reference to the Animator component attached to the player object.
        playerAnimator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        // Call the method to handle player movement.
        HandlePlayerMovement();

        // Call the method to keep the player within the specified boundary.
        ConstrainPlayerPosition();
    }

    // Method to handle player movement.
    void HandlePlayerMovement()
    {
        // Get the horizontal input axis value (A/D keys or Left/Right arrow keys).
        horizontalInput = Input.GetAxis("Horizontal");

        // Flip player's sprite based on horizontal input.
        FlipSprite();

        // Check if the player is on the surface and adjust movement speed accordingly.
        if (isOnSurface)
        {
            MovePlayer(moveSpeed);
        }
        else if (!isOnSurface)
        {
            MovePlayer(moveSpeed * 0.5f);
        }

        // Check for jump input and perform appropriate jump action.
        if (GetJumpKeyPressed())
        {
            if (isOnSurface)
            {
                Jump(jumpForce);
                isOnSurface = false;
                doubleJumpUsed = false;
            }
            else if (!isOnSurface && !doubleJumpUsed)
            {
                doubleJumpUsed = true;
                Jump(doubleJumpForceRatio * jumpForce);
            }
        }
    }

    // Check if any of the jump keys (Space, UpArrow, W) are pressed.
    bool GetJumpKeyPressed()
    {
        return Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W);
    }

    // Move the player based on the input and speed.
    void MovePlayer(float speed)
    {
        float horizontalMotion = horizontalInput * speed * Time.deltaTime;
        float movingThreshold = Math.Abs(horizontalMotion / Time.deltaTime);

        // Handle player animations.
        if (movingThreshold > 0.1)
        {
            playerAnimator.SetBool("IsMoving", true);
        }
        else
        {
            playerAnimator.SetBool("IsMoving", false);
        }

        playerAnimator.SetFloat("Speed", movingThreshold);

        transform.Translate(horizontalMotion * Vector2.right);
    }

    // Apply a vertical force to make the player jump.
    void Jump(float force)
    {
        playerRigidbody.AddForce(force * Vector2.up);
    }

    // Method to constrain the player's position within the horizontal boundary.
    void ConstrainPlayerPosition()
    {
        if (transform.position.x > xBoundary)
        {
            // If the player moves beyond the right boundary, set its position to the boundary's limit.
            transform.position = new Vector2(xBoundary, transform.position.y);
        }

        if (transform.position.x < -xBoundary)
        {
            // If the player moves beyond the left boundary, set its position to the boundary's limit.
            transform.position = new Vector2(-xBoundary, transform.position.y);
        }
    }

    // Flip the player sprite to face the appropriate direction.
    void FlipSprite()
    {
        if (horizontalInput < 0)
        {
            // Flip to face left.
            playerSpriteRenderer.flipX = true;
        }
        else if (horizontalInput > 0)
        {
            // Flip to face right.
            playerSpriteRenderer.flipX = false;
        }
    }

    // Method to handle collision with other objects.
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Check if the player is colliding with a surface.
        if (collision.gameObject.CompareTag("Surface"))
        {
            isOnSurface = true;
        }
    }
}