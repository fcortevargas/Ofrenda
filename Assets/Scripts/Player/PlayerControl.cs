using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class PlayerControl : MonoBehaviour
{
    #region COMPONENTS
    private Rigidbody2D _rigidbody2D;
    [FormerlySerializedAs("_groundLayers")] [SerializeField][Tooltip("Specify which layers should be treated as ground")] private LayerMask groundLayers;
    #endregion

    #region STATE CONSTANTS
    [FormerlySerializedAs("_groundThreshold")]
    [Header("State Constants")]
    [SerializeField] private float groundThreshold = 0.6f;
    [FormerlySerializedAs("_colliderOffset")] [SerializeField] private Vector3 colliderOffset;
    #endregion

    #region STATE VARIABLES
    [FormerlySerializedAs("_canPlayerMove")]
    [Header("State Variables")]
    [SerializeField][Tooltip("Can the player move?")] private bool canPlayerMove = true;
    [Tooltip("Is the player on the ground?")] private static bool _isOnGround;
    [FormerlySerializedAs("_isTurning")] [SerializeField][Tooltip("Is the player turning?")] private bool isTurning = false;
    [FormerlySerializedAs("_canJumpAgain")] [SerializeField][Tooltip("Can the player jump again?")] private bool canJumpAgain = false;
    [FormerlySerializedAs("_isJumping")] [SerializeField][Tooltip("Is the player jumping?")] private bool isJumping;
    #endregion

    #region MOVEMENT CONTROL CONSTANTS
    [FormerlySerializedAs("_maxSpeed")]
    [Header("Movement Control Constants")]
    [SerializeField, Range(0f, 20f)][Tooltip("Maximum movement speed")] private float maxSpeed = 12f;
    [FormerlySerializedAs("_maxAcceleration")] [SerializeField, Range(0f, 100f)][Tooltip("How fast to reach max speed")] private float maxAcceleration = 50f;
    [FormerlySerializedAs("_maxDecceleration")] [SerializeField, Range(0f, 100f)][Tooltip("How fast to stop when not changing direction")] private float maxDecceleration = 90f;
    [FormerlySerializedAs("_maxTurnSpeed")] [SerializeField, Range(0f, 100f)][Tooltip("How fast to stop when changing direction")] private float maxTurnSpeed = 40f;
    [FormerlySerializedAs("_maxAirAcceleration")] [SerializeField, Range(0f, 100f)][Tooltip("How fast to reach max speed in mid-air")] private float maxAirAcceleration = 50f;
    [FormerlySerializedAs("_maxAirDeceleration")] [SerializeField, Range(0f, 100f)][Tooltip("How fast to stop in mid-air when not changing direction")] private float maxAirDeceleration = 40f;
    [FormerlySerializedAs("_maxAirTurnSpeed")] [SerializeField, Range(0f, 100f)][Tooltip("How fast to stop in mid-air when changing direction")] private float maxAirTurnSpeed = 20f;
    [FormerlySerializedAs("_friction")] [SerializeField, Range(0f, 1f)][Tooltip("Friction to apply against movement")] private float friction = 0.9f;
    #endregion

    #region MOVEMENT CONTROL VARIABLES
    [FormerlySerializedAs("_velocity")]
    [Header("Movement Control Constants")]
    [SerializeField][Tooltip("The player's current velocity")] private Vector2 velocity;
    [FormerlySerializedAs("_targetVelocity")] [SerializeField][Tooltip("The player's target velocity")] private Vector2 targetVelocity;
    #endregion

    #region JUMP CONTROL CONSTANTS
    [FormerlySerializedAs("_maxJumpHeight")]
    [Header("Jump Control Constants")]
    [SerializeField, Range(2f, 5f)][Tooltip("Maximum jump height")] private float maxJumpHeight = 5f;
    [FormerlySerializedAs("_timeToJumpApex")] [SerializeField, Range(0.1f, 1.25f)][Tooltip("How long it takes to reach that height before coming back down")] private float timeToJumpApex = 0.42f;
    [FormerlySerializedAs("_upwardGravityMultiplier")] [SerializeField, Range(0f, 5f)][Tooltip("Gravity multiplier to apply when going up")] private float upwardGravityMultiplier = 1.2f;
    [FormerlySerializedAs("_downwardGravityMultiplier")] [SerializeField, Range(1f, 10f)][Tooltip("Gravity multiplier to apply when coming down")] private float downwardGravityMultiplier = 1.6f;
    [FormerlySerializedAs("_maxNumberJumps")] [SerializeField, Range(0, 1)][Tooltip("How many times can you jump in the air?")] private int maxNumberJumps = 0;
    [FormerlySerializedAs("_enableVariableJumpHeight")] [SerializeField][Tooltip("Should the character drop when you let go of jump?")] private bool enableVariableJumpHeight;
    [FormerlySerializedAs("_jumpCutOff")] [SerializeField, Range(1f, 10f)][Tooltip("Gravity multiplier when you let go of jump")] private float jumpCutOff = 3f;
    [FormerlySerializedAs("_fallSpeedLimit")] [SerializeField, Range(1f, 50f)][Tooltip("The fastest speed the character can fall")] private float fallSpeedLimit = 40f;
    [FormerlySerializedAs("_coyoteTime")] [SerializeField, Range(0f, 0.3f)][Tooltip("How long should coyote time last?")] private float coyoteTime = 0.15f;
    [FormerlySerializedAs("_jumpBuffer")] [SerializeField, Range(0f, 0.3f)][Tooltip("How far from ground should we cache your jump?")] private float jumpBuffer = 0f;
    #endregion

    #region JUMP CONTROL VARIABLES
    [FormerlySerializedAs("_jumpSpeed")]
    [Header("Jump Control Variables")]
    [SerializeField][Tooltip("The player's jump speed")] private float jumpSpeed;
    [FormerlySerializedAs("_defaultGravityScale")] [SerializeField][Tooltip("The default gravity scale")] private float defaultGravityScale = 1f;
    [FormerlySerializedAs("_gravityMultiplier")] [SerializeField][Tooltip("How much will the gravity scale be scaled by?")] private float gravityMultiplier;
    [FormerlySerializedAs("_executeJump")] [SerializeField][Tooltip("Switch to execute jump")] private bool executeJump;
    [FormerlySerializedAs("_jumpBufferCounter")] [SerializeField][Tooltip("Counter that tracks the time to determine if a jump should be queued")] private float jumpBufferCounter;
    [FormerlySerializedAs("_coyoteTimeCounter")] [SerializeField][Tooltip("Counter that tracks the time to determine if a coyote jump should be executed")] private float coyoteTimeCounter = 0;

    #endregion

    #region INPUT VARIABLES
    [FormerlySerializedAs("_pressingMove")]
    [Header("Input Variables")]
    [SerializeField] private bool pressingMove;
    [FormerlySerializedAs("_pressedJump")] [SerializeField] private bool pressedJump;
    [FormerlySerializedAs("_pressingJump")] [SerializeField] private bool pressingJump;
    [FormerlySerializedAs("_horizontalInput")] [SerializeField] private float horizontalInput;
    #endregion
    
    #region STATIC PROPERTIES

    public static bool IsOnGround => _isOnGround;

    #endregion

    private void Awake()
    {
        _rigidbody2D = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        // Used to stop movement when the character is playing certain animations
        if (!canPlayerMove)
        {
            horizontalInput = 0;
            pressedJump = false;
            pressingJump = false;
        }
        else
        {
            horizontalInput = Input.GetAxisRaw("Horizontal");
            pressingJump = Input.GetButton("Jump");
            if (Input.GetButtonDown("Jump"))
            {
                pressedJump = true;
            }
        }

        // Tells us that we are currently pressing a direction button
        if (horizontalInput != 0)
        {
            pressingMove = true;
        }
        else
        {
            pressingMove = false;
        }

        //Calculate's the character's desired velocity - which is the direction you are facing, multiplied by the character's maximum speed
        //Friction is not used in this game
        targetVelocity = new Vector2(horizontalInput, 0f) * (maxSpeed * (1 - Mathf.Lerp(0, friction, Convert.ToSingle(isTurning))));

        // Handle jump queuing for jump buffer assist
        HandleJumpBuffering();

        // Handle coyote jumping assist 
        HandleCoyoteJumping();
    }

    private void FixedUpdate()
    {
        //// Set the gravity scale of the player's rigid body
        SetGravityScale();

        _isOnGround = IsPlayerOnGround();

        velocity = _rigidbody2D.velocity;

        Run();

        //Keep trying to do a jump, for as long as _pressedJump is true
        if (pressedJump)
        {
            Jump();
            _rigidbody2D.velocity = velocity;

            // Skip gravity calculations this frame, so currentlyJumping doesn't turn off
            // This makes sure you can't do the coyote time double jump bug
            return;
        }

        // Compute the gravity multiplier of the player
        GetGravityMultiplier();
    }

    private void Run()
    {
        //Set our acceleration, deceleration, and turn speed stats, based on whether we're on the ground on in the air
        float acceleration = _isOnGround ? maxAcceleration : maxAirAcceleration;
        float deceleration = _isOnGround ? maxDecceleration : maxAirDeceleration;
        float turnSpeed = _isOnGround ? maxTurnSpeed : maxAirTurnSpeed;

        float maxSpeedChange;

        if (pressingMove)
        {
            //If the sign (i.e. positive or negative) of our input direction doesn't match our movement, it means we're turning around and so should use the turn speed.
            if (Mathf.Sign(horizontalInput) != Mathf.Sign(velocity.x))
            {
                maxSpeedChange = turnSpeed * Time.deltaTime;
                isTurning = true;
            }
            else
            {
                //If they match, it means we're simply running along and so should use the acceleration stat
                maxSpeedChange = acceleration * Time.deltaTime;
                isTurning = false;
            }

            // Flip the player
            if (horizontalInput > 0 && !isTurning)
            {
                transform.localScale = new Vector3(1, 1, 1);
            }
            if (horizontalInput < 0 && !isTurning)
            {
                transform.localScale = new Vector3(-1, 1, 1);
            }
        }
        else
        {
            //And if we're not pressing a direction at all, use the deceleration stat
            maxSpeedChange = deceleration * Time.deltaTime;
        }

        //Move our velocity towards the target velocity, at the rate of the number calculated above
        velocity.x = Mathf.MoveTowards(velocity.x, targetVelocity.x, maxSpeedChange);

        //Update the Rigidbody with this new velocity
        _rigidbody2D.velocity = velocity;
    }

    private void Jump()
    {
        if (_isOnGround || (coyoteTimeCounter > 0.03f && coyoteTimeCounter < coyoteTime) || canJumpAgain)
        {
            pressedJump = false;
            jumpBufferCounter = 0;
            coyoteTimeCounter = 0;

            //If we have double jump on, allow us to jump again (but only once)
            canJumpAgain = (maxNumberJumps == 1 && canJumpAgain == false);

            // Determine the power of the jump, based on our gravity and stats
            jumpSpeed = Mathf.Sqrt(-2f * Physics2D.gravity.y * _rigidbody2D.gravityScale * maxJumpHeight);

            // If player is moving up or down when it jumps (such as when doing a double jump), change the jumpSpeed;
            // This will ensure the jump is the exact same strength, no matter your velocity.
            if (velocity.y > 0f)
            {
                jumpSpeed = Mathf.Max(jumpSpeed - velocity.y, 0f);
            }
            else if (velocity.y < 0f)
            {
                jumpSpeed += Mathf.Abs(_rigidbody2D.velocity.y);
            }

            //Apply the new jumpSpeed to the velocity. It will be sent to the Rigidbody in FixedUpdate;
            velocity.y += jumpSpeed;
            isJumping = true;
        }

        if (jumpBuffer == 0)
        {
            //If we don't have a jump buffer, then turn off desiredJump immediately after hitting jumping
            pressedJump = false;
        }
    }

    private void SetGravityScale()
    {
        Vector2 gravity = new(0, -2 * maxJumpHeight / (timeToJumpApex * timeToJumpApex));
        _rigidbody2D.gravityScale = gravity.y / Physics2D.gravity.y * gravityMultiplier;
    }

    private void GetGravityMultiplier()
    {
        // If going up...
        if (_rigidbody2D.velocity.y > 0.01f)
        {
            if (_isOnGround)
            {
                // Don't change it if player is standing on something (such as a moving platform)
                gravityMultiplier = defaultGravityScale;
            }
            else
            {
                //If we're using variable jump height...)
                if (enableVariableJumpHeight)
                {
                    // Apply upward multiplier if player is rising and holding jump
                    if (pressingJump && isJumping)
                    {
                        gravityMultiplier = upwardGravityMultiplier;
                    }
                    // Apply a special downward multiplier if the player lets go of jump
                    else
                    {
                        gravityMultiplier = jumpCutOff;
                    }
                }
                else
                {
                    gravityMultiplier = upwardGravityMultiplier;
                }
            }
        }

        //Else if going down...
        else if (_rigidbody2D.velocity.y < -0.01f)
        {
            if (_isOnGround)
            // Don't change it if player is standing on something (such as a moving platform)
            {
                gravityMultiplier = defaultGravityScale;
            }
            else
            {
                // Otherwise, apply the downward gravity multiplier
                gravityMultiplier = downwardGravityMultiplier;
            }

        }
        //Else not moving vertically at all
        else
        {
            if (_isOnGround)
            {
                isJumping = false;
            }

            gravityMultiplier = defaultGravityScale;
        }

        //Set the character's Rigidbody's velocity
        //But clamp the Y variable within the bounds of the speed limit, for the terminal velocity assist option
        _rigidbody2D.velocity = new Vector3(velocity.x, Mathf.Max(velocity.y, -fallSpeedLimit));
    }

    private void HandleJumpBuffering()
    {
        //Jump buffer allows us to queue up a jump, which will play when we next hit the ground
        if (jumpBuffer > 0)
        {
            //Instead of immediately turning off _pressedJump, start counting up...
            //All the while, the Jump function will repeatedly be fired off
            if (pressedJump)
            {
                jumpBufferCounter += Time.deltaTime;

                if (jumpBufferCounter > jumpBuffer)
                {
                    //If time exceeds the jump buffer, turn off _pressedJump
                    pressedJump = false;
                    jumpBufferCounter = 0;
                }
            }
        }
    }

    private void HandleCoyoteJumping()
    {
        //If we're not on the ground and we're not currently jumping, that means we've stepped off the edge of a platform.
        //So, start the coyote time counter...
        if (!isJumping && !_isOnGround)
        {
            coyoteTimeCounter += Time.deltaTime;
        }
        else
        {
            //Reset it when we touch the ground, or jump
            coyoteTimeCounter = 0;
        }
    }

    private bool IsPlayerOnGround()
    {
        Vector3 origin = transform.position + new Vector3(0, 0.5f, 0);
        return Physics2D.Raycast(origin + colliderOffset, Vector2.down, groundThreshold, groundLayers) || Physics2D.Raycast(origin - colliderOffset, Vector2.down, groundThreshold, groundLayers);
    }

    private void OnDrawGizmos()
    {
        //Draw the ground colliders on screen for debug purposes
        Vector3 origin = transform.position + new Vector3(0, 0.5f, 0);
        if (_isOnGround) { Gizmos.color = Color.green; } else { Gizmos.color = Color.red; }
        Gizmos.DrawLine(origin + colliderOffset, origin + colliderOffset + Vector3.down * groundThreshold);
        Gizmos.DrawLine(origin - colliderOffset, origin - colliderOffset + Vector3.down * groundThreshold);
    }
}
