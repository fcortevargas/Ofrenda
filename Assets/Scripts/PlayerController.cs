using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private Rigidbody2D playerRigidBody;    // Reference to the Rigidbody2D component of the player.

    private const float speed = 5.0f;       // Player movement speed.
    private const float jumpForce = 150.0f; // Force applied to the player for jumping.
    private const float xBoundary = 11.8f;  // Horizontal boundary for constraining player movement.

    private float horizontalInput;          // Stores the horizontal input value.

    // Start is called before the first frame update
    void Start()
    {
        // Get the reference to the Rigidbody2D component attached to the player object.
        playerRigidBody = GetComponent<Rigidbody2D>();  
    }

    // Update is called once per frame
    void Update()
    {
        // Call the method to handle player movement.
        MovePlayer();

        // Call the method to keep the player within the specified boundary.
        ConstrainPlayerPosition();  
    }

    // Method to handle player movement.
    void MovePlayer()
    {
        // Get the horizontal input axis value (A/D keys or Left/Right arrow keys).
        horizontalInput = Input.GetAxis("Horizontal");  

        // Translate the player's position based on the horizontal input and the movement speed.
        transform.Translate(horizontalInput * speed * Time.deltaTime * Vector3.right);

        // Check if the jump input (Space key, Up Arrow key, or W key) is pressed.
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
        {
            // If so, apply an upward force to the player to make it jump.
            playerRigidBody.AddForce(Vector3.up * jumpForce);
        }
    }

    // Method to constrain the player's position within the horizontal boundary.
    void ConstrainPlayerPosition()
    {
        if (transform.position.x > xBoundary)
        {
            // If the player moves beyond the right boundary, set its position to the boundary's limit.
            transform.position = new Vector3(xBoundary, transform.position.y);
        }

        if (transform.position.x < -xBoundary)
        {
            // If the player moves beyond the left boundary, set its position to the boundary's limit.
            transform.position = new Vector3(-xBoundary, transform.position.y);
        }
    }
}