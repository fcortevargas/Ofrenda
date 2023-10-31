using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float jumpForce = 400f;                            // Amount of force added when the player jumps
    [Range(0, 1)][SerializeField] private float crouchSpeed = .36f;             // Amount of maxSpeed applied to crouching movement. 1 = 100%
    [Range(0, 1)][SerializeField] private float midairSpeed = .5f;              // Amount of maxSpeed applied to movement midair. 1 = 100%
    [Range(0, .3f)][SerializeField] private float movementSmoothing = .05f;     // How much to smooth out the movement
    [SerializeField] private bool airControl = false;                           // Whether or not a player can steer while jumping
    [SerializeField] private LayerMask whatIsGround;                            // A mask determining what is ground to the character
    [SerializeField] private Transform groundCheck;                             // A position marking where to check if the player is grounded
    [SerializeField] private Transform ceilingCheck;                            // A position marking where to check for ceilings
    [SerializeField] private Collider2D crouchDisableCollider;                  // A collider that will be disabled when crouching

    const float groundedRadius = .2f;                                           // Radius of the overlap circle to determine if grounded
    [SerializeField] private bool isGrounded;                                   // Whether or not the player is grounded.
    const float ceilingRadius = .2f;                                            // Radius of the overlap circle to determine if the player can stand up
    private Rigidbody2D playerRigidbody2D;
    private SpriteRenderer playerSpriteRenderer;
    private Animator playerAnimator;
    private Player player;
    private bool isFacingRight = true;                                          // For determining which way the player is currently facing
    private Vector3 velocity = Vector3.zero;

    [Header("Events")]
    [Space]

    public UnityEvent OnLandEvent;

    [Serializable]
    public class BoolEvent : UnityEvent<bool> { }

    public BoolEvent OnCrouchEvent;
    private bool wasCrouching = false;

    private void Awake()
    {
        playerRigidbody2D = GetComponent<Rigidbody2D>();

        playerSpriteRenderer = GetComponent<SpriteRenderer>();

        playerAnimator = GetComponent<Animator>();

        player = new Player();

        OnLandEvent ??= new UnityEvent();

        OnCrouchEvent ??= new BoolEvent();
    }

    private void FixedUpdate()
    {
        bool wasGrounded = isGrounded;
        isGrounded = false;

        // The player is grounded if a circlecast to the groundcheck position hits anything designated as ground
        // This can be done using layers instead but Sample Assets will not overwrite your project settings.
        Collider2D[] colliders = Physics2D.OverlapCircleAll(groundCheck.position, groundedRadius, whatIsGround);
        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i].gameObject != gameObject)
            {
                isGrounded = true;
                if (!wasGrounded)
                    OnLandEvent.Invoke();
            }
        }
    }


    public void Move(float move, bool crouch, bool jump)
    {
        // If crouching, check to see if the character can stand up
        if (!crouch)
        {
            // If the character has a ceiling preventing them from standing up, keep them crouching
            if (Physics2D.OverlapCircle(ceilingCheck.position, ceilingRadius, whatIsGround))
            {
                crouch = true;
            }
        }

        // Only control the player if grounded or airControl is turned on
        if (isGrounded || airControl)
        {

            // If crouching
            if (crouch)
            {
                if (!wasCrouching)
                {
                    wasCrouching = true;
                    OnCrouchEvent.Invoke(true);
                }

                // Reduce the speed by the crouchSpeed multiplier
                move *= crouchSpeed;

                // Disable one of the colliders when crouching
                if (crouchDisableCollider != null)
                {
                    crouchDisableCollider.enabled = false;
                }
            }
            else
            {
                // Enable the collider when not crouching
                if (crouchDisableCollider != null)
                {
                    crouchDisableCollider.enabled = true;
                }

                if (wasCrouching)
                {
                    wasCrouching = false;
                    OnCrouchEvent.Invoke(false);
                }
            }

            // Move the character slowlier if it is in the air
            if (!isGrounded)
            {
                // Reduce the speed by the midairSpeed multiplier
                move *= midairSpeed;
            }

            // Move the character by finding the target velocity
            Vector3 targetVelocity = new Vector2(move * 10f, playerRigidbody2D.velocity.y);
            // And then smoothing it out and applying it to the character
            playerRigidbody2D.velocity = Vector3.SmoothDamp(playerRigidbody2D.velocity, targetVelocity, ref velocity, movementSmoothing);

            // If the input is moving the player right and the player is facing left...
            if (move > 0 && !isFacingRight)
            {
                // ... flip the player.
                Flip();
            }
            // Otherwise if the input is moving the player left and the player is facing right...
            else if (move < 0 && isFacingRight)
            {
                // ... flip the player.
                Flip();
            }
        }
        // If the player should jump...
        if (isGrounded && jump)
        {
            // Add a vertical force to the player.
            isGrounded = false;
            playerRigidbody2D.AddForce(new Vector2(0f, jumpForce));
        }
    }


    private void Flip()
    {
        // Switch the way the player is labelled as facing.
        isFacingRight = !isFacingRight;

        // Multiply the player's x local scale by -1.
        Vector3 playerLocalScale = transform.localScale;
        playerLocalScale.x *= -1;
        transform.localScale = playerLocalScale;
    }

    public void SwitchCharacter(string characterType)
    {
        // Only three character types allowed
        if (characterType == "fire" || characterType == "cempasuchil" || characterType == "xolo")
        {
            player.Type = characterType;
            Debug.Log(player.Type);
            Debug.Log(player.Animator);
            Debug.Log(player.Sprite);
        }
        else
        {
            Debug.LogError("Character type not accepted, please enter accepted character type.");
            return;
        }
    }
}

struct Player
{
    private string _type;
    private string _animator;
    private string _sprite;

    public string Type
    {
        readonly get => _type;

        set
        {
            _type = value;
            _animator = _type + "-animated";
            _sprite = _type + "-animated_Frame_0";
        }
    }

    public readonly string Animator
    {
        get => _animator;
    }

    public readonly string Sprite
    {
        get => _sprite;
    }
}