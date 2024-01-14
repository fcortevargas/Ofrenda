using System;
using UnityEngine;

public class PlayerControl : MonoBehaviour
{
    #region COMPONENTS
    // Player's rigidbody for physics calculations
    private Rigidbody2D _rigidbody2D;
    // Specify which layers should be treated as ground
    [SerializeField] private LayerMask groundLayers;
    #endregion

    #region COMPUTING CONSTANTS
    private const float Tolerance = 0.001f;
    #endregion

    #region STATE CONSTANTS
    // Header("State Constants")
    // Threshold to determine if the player is on the ground
    [SerializeField] private float groundThreshold = 0.6f;
    // Offset of the collider used in ground detection
    [SerializeField] private Vector3 colliderOffset;
    #endregion

    #region STATE VARIABLES
    // Can the player move?
    private static bool _canMove = true;
    // Is the player on the ground?
    private static bool _isOnGround;
    // Is the player turning?
    private static bool _isTurning;
    // Can the player jump again?
    private static bool _canJumpAgain;
    // Is the player jumping?
    private static bool _isJumping;
    #endregion

    #region MOVEMENT CONTROL CONSTANTS
    // Maximum movement speed
    private const float MaxSpeed = 12f;
    // How fast to reach max speed
    private const float MaxAcceleration = 50f;
    // How fast to stop when not changing direction
    private const float MaxDecceleration = 90f;
    // How fast to stop when changing direction
    private const float MaxTurnSpeed = 40f;
    // How fast to reach max speed in mid-air
    private const float MaxAirAcceleration = 50f;
    // How fast to stop in mid-air when not changing direction
    private const float MaxAirDeceleration = 40f;
    // How fast to stop in mid-air when changing direction
    private const float MaxAirTurnSpeed = 20f;
    // Friction to apply against movement
    private const float Friction = 0.9f;
    #endregion

    #region MOVEMENT CONTROL VARIABLES
    // The player's current velocity
    private Vector2 _velocity;
    // The player's target velocity
    private Vector2 _targetVelocity;
    #endregion

    #region JUMP CONTROL CONSTANTS
    // Maximum jump height
    private const float MaxJumpHeight = 5f;
    // How long it takes to reach that height before coming back down
    private const float TimeToJumpApex = 0.42f;
    // The default gravity scale
    private const float DefaultGravityScale = 1f;
    // Gravity multiplier to apply when going up
    private const float UpwardGravityMultiplier = 1.2f;
    // Gravity multiplier to apply when coming down
    private const float DownwardGravityMultiplier = 1.6f;
    // How many times can you jump in the air?
    private const int MaxNumberJumps = 0;
    // Gravity multiplier when you let go of jump
    private const float JumpCutOff = 3f;
    // The fastest speed the character can fall
    private const float FallSpeedLimit = 40f;
    // How long should coyote time last?
    private const float CoyoteTime = 0.15f;
    #endregion

    #region JUMP CONTROL VARIABLES
    // The player's jump speed
    private float _jumpSpeed;
    // Should the character drop when you let go of jump?
    public bool enableVariableJumpHeight;
    // How much will the gravity scale be scaled by?
    private float _gravityMultiplier;
    // Switch to execute jump
    private bool _executeJump;
    // How far from ground should we cache your jump?
    public float jumpBuffer;
    // Counter that tracks the time to determine if a jump should be queued
    private float _jumpBufferCounter;
    // Counter that tracks the time to determine if a coyote jump should be executed
    private float _coyoteTimeCounter;
    #endregion

    #region INPUT VARIABLES
    private bool _pressingMove;
    private bool _pressedJump;
    private bool _pressingJump;
    private float _horizontalInput;
    #endregion
    
    #region STATIC PROPERTIES
    // Static properties for usage in other scripts
    public static bool IsOnGround => _isOnGround;
    public static bool CanMove => _canMove;
    public static bool IsTurning => _isTurning;
    public static bool CanJumpAgain => _canJumpAgain;
    public static bool IsJumping => _isJumping;
    #endregion

    private void Awake()
    {
        _rigidbody2D = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        // Used to stop movement when the character is playing certain animations
        if (!_canMove)
        {
            _horizontalInput = 0;
            _pressedJump = false;
            _pressingJump = false;
        }
        else
        {
            _horizontalInput = Input.GetAxisRaw("Horizontal");
            _pressingJump = Input.GetButton("Jump");
            if (Input.GetButtonDown("Jump"))
            {
                _pressedJump = true;
            }
        }

        // Tells us that we are currently pressing a direction button
        if (_horizontalInput != 0)
        {
            _pressingMove = true;
        }
        else
        {
            _pressingMove = false;
        }

        //Calculate's the character's desired velocity - which is the direction you are facing, multiplied by the character's maximum speed
        //Friction is not used in this game
        _targetVelocity = new Vector2(_horizontalInput, 0f) * (MaxSpeed * (1 - Mathf.Lerp(0, Friction, Convert.ToSingle(_isTurning))));

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

        _velocity = _rigidbody2D.velocity;

        Run();

        //Keep trying to do a jump, for as long as _pressedJump is true
        if (_pressedJump)
        {
            Jump();
            _rigidbody2D.velocity = _velocity;

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
        float acceleration = _isOnGround ? MaxAcceleration : MaxAirAcceleration;
        float deceleration = _isOnGround ? MaxDecceleration : MaxAirDeceleration;
        float turnSpeed = _isOnGround ? MaxTurnSpeed : MaxAirTurnSpeed;

        float maxSpeedChange;

        if (_pressingMove)
        {
            //If the sign (i.e. positive or negative) of our input direction doesn't match our movement, it means we're turning around and so should use the turn speed.
            if (Math.Abs(Mathf.Sign(_horizontalInput) - Mathf.Sign(_velocity.x)) > Tolerance)
            {
                maxSpeedChange = turnSpeed * Time.deltaTime;
                _isTurning = true;
            }
            else
            {
                //If they match, it means we're simply running along and so should use the acceleration stat
                maxSpeedChange = acceleration * Time.deltaTime;
                _isTurning = false;
            }

            // Flip the player
            if (_horizontalInput > 0 && !_isTurning)
            {
                transform.localScale = new Vector3(1, 1, 1);
            }
            if (_horizontalInput < 0 && !_isTurning)
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
        _velocity.x = Mathf.MoveTowards(_velocity.x, _targetVelocity.x, maxSpeedChange);

        //Update the Rigidbody with this new velocity
        _rigidbody2D.velocity = _velocity;
    }

    private void Jump()
    {
        if (_isOnGround || (_coyoteTimeCounter > 0.03f && _coyoteTimeCounter < CoyoteTime) || _canJumpAgain)
        {
            _pressedJump = false;
            _jumpBufferCounter = 0;
            _coyoteTimeCounter = 0;

            //If we have double jump on, allow us to jump again (but only once)
            _canJumpAgain = MaxNumberJumps == 1 && !_canJumpAgain;

            // Determine the power of the jump, based on our gravity and stats
            _jumpSpeed = Mathf.Sqrt(-2f * Physics2D.gravity.y * _rigidbody2D.gravityScale * MaxJumpHeight);

            // If player is moving up or down when it jumps (such as when doing a double jump), change the jumpSpeed;
            // This will ensure the jump is the exact same strength, no matter your velocity.
            if (_velocity.y > 0f)
            {
                _jumpSpeed = Mathf.Max(_jumpSpeed - _velocity.y, 0f);
            }
            else if (_velocity.y < 0f)
            {
                _jumpSpeed += Mathf.Abs(_rigidbody2D.velocity.y);
            }

            //Apply the new jumpSpeed to the velocity. It will be sent to the Rigidbody in FixedUpdate;
            _velocity.y += _jumpSpeed;
            _isJumping = true;
        }

        if (jumpBuffer == 0)
        {
            //If we don't have a jump buffer, then turn off desiredJump immediately after hitting jumping
            _pressedJump = false;
        }
    }

    private void SetGravityScale()
    {
        Vector2 gravity = new(0, -2 * MaxJumpHeight / (TimeToJumpApex * TimeToJumpApex));
        _rigidbody2D.gravityScale = gravity.y / Physics2D.gravity.y * _gravityMultiplier;
    }

    private void GetGravityMultiplier()
    {
        // If going up...
        if (_rigidbody2D.velocity.y > 0.01f)
        {
            if (_isOnGround)
            {
                // Don't change it if player is standing on something (such as a moving platform)
                _gravityMultiplier = DefaultGravityScale;
            }
            else
            {
                //If we're using variable jump height...)
                if (enableVariableJumpHeight)
                {
                    // Apply upward multiplier if player is rising and holding jump
                    if (_pressingJump && _isJumping)
                    {
                        _gravityMultiplier = UpwardGravityMultiplier;
                    }
                    // Apply a special downward multiplier if the player lets go of jump
                    else
                    {
                        _gravityMultiplier = JumpCutOff;
                    }
                }
                else
                {
                    _gravityMultiplier = UpwardGravityMultiplier;
                }
            }
        }

        //Else if going down...
        else if (_rigidbody2D.velocity.y < -0.01f)
        {
            if (_isOnGround)
            // Don't change it if player is standing on something (such as a moving platform)
            {
                _gravityMultiplier = DefaultGravityScale;
            }
            else
            {
                // Otherwise, apply the downward gravity multiplier
                _gravityMultiplier = DownwardGravityMultiplier;
            }

        }
        //Else not moving vertically at all
        else
        {
            if (_isOnGround)
            {
                _isJumping = false;
            }

            _gravityMultiplier = DefaultGravityScale;
        }

        //Set the character's Rigidbody's velocity
        //But clamp the Y variable within the bounds of the speed limit, for the terminal velocity assist option
        _rigidbody2D.velocity = new Vector3(_velocity.x, Mathf.Max(_velocity.y, -FallSpeedLimit));
    }

    private void HandleJumpBuffering()
    {
        //Jump buffer allows us to queue up a jump, which will play when we next hit the ground
        if (jumpBuffer > 0)
        {
            //Instead of immediately turning off _pressedJump, start counting up...
            //All the while, the Jump function will repeatedly be fired off
            if (_pressedJump)
            {
                _jumpBufferCounter += Time.deltaTime;

                if (_jumpBufferCounter > jumpBuffer)
                {
                    //If time exceeds the jump buffer, turn off _pressedJump
                    _pressedJump = false;
                    _jumpBufferCounter = 0;
                }
            }
        }
    }

    private void HandleCoyoteJumping()
    {
        //If we're not on the ground and we're not currently jumping, that means we've stepped off the edge of a platform.
        //So, start the coyote time counter...
        if (!_isJumping && !_isOnGround)
        {
            _coyoteTimeCounter += Time.deltaTime;
        }
        else
        {
            //Reset it when we touch the ground, or jump
            _coyoteTimeCounter = 0;
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
