using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControl : MonoBehaviour
{
    #region COMPONENTS
    private Rigidbody2D _rigidbody2D;
    [SerializeField][Tooltip("Specify which layers should be treated as ground")] private LayerMask _groundLayers;
    #endregion

    #region STATE CONSTANTS
    [Header("State Constants")]
    [SerializeField] private float _groundThreshold = 0.95f;
    [SerializeField] private Vector3 _colliderOffset;
    #endregion

    #region STATE VARIABLES
    [Header("State Variables")]
    [SerializeField][Tooltip("Can the player move?")] private bool _canPlayerMove = true;
    [SerializeField][Tooltip("Is the player on the ground?")] private bool _isOnGround;
    [SerializeField][Tooltip("Is the player turning?")] bool _isTurning = false;
    [SerializeField][Tooltip("Can the player jump again?")] private bool _canJumpAgain = false;
    [SerializeField][Tooltip("Is the player jumping?")] private bool _isJumping;
    #endregion

    #region MOVEMENT CONTROL CONSTANTS
    [Header("Movement Control Constants")]
    [SerializeField, Range(0f, 20f)][Tooltip("Maximum movement speed")] private float _maxSpeed = 15f;
    [SerializeField, Range(0f, 100f)][Tooltip("How fast to reach max speed")] private float _maxAcceleration = 70f;
    [SerializeField, Range(0f, 100f)][Tooltip("How fast to stop when not changing direction")] private float _maxDecceleration = 90f;
    [SerializeField, Range(0f, 100f)][Tooltip("How fast to stop when changing direction")] private float _maxTurnSpeed = 20f;
    [SerializeField, Range(0f, 100f)][Tooltip("How fast to reach max speed in mid-air")] private float _maxAirAcceleration;
    [SerializeField, Range(0f, 100f)][Tooltip("How fast to stop in mid-air when not changing direction")] private float _maxAirDeceleration;
    [SerializeField, Range(0f, 100f)][Tooltip("How fast to stop in mid-air when changing direction")] private float _maxAirTurnSpeed = 80f;
    [SerializeField, Range(0f, 1f)][Tooltip("Friction to apply against movement")] private float _friction;
    #endregion

    #region MOVEMENT CONTROL VARIABLES
    [Header("Movement Control Constants")]
    [SerializeField][Tooltip("The player's current velocity")] private Vector2 _velocity;
    [SerializeField][Tooltip("The player's target velocity")] private Vector2 _targetVelocity;
    #endregion

    #region JUMP CONTROL CONSTANTS
    [Header("Jump Control Constants")]
    [SerializeField, Range(2f, 5f)][Tooltip("Maximum jump height")] private float _maxJumpHeight = 4f;
    [SerializeField, Range(0.1f, 1.25f)][Tooltip("How long it takes to reach that height before coming back down")] private float _timeToJumpApex = 0.2f;
    [SerializeField, Range(0f, 5f)][Tooltip("Gravity multiplier to apply when going up")] private float _upwardGravityMultiplier = 1f;
    [SerializeField, Range(1f, 10f)][Tooltip("Gravity multiplier to apply when coming down")] private float _downwardGravityMultiplier = 2f;
    [SerializeField, Range(0, 1)][Tooltip("How many times can you jump in the air?")] private int _maxNumberJumps = 0;
    [SerializeField][Tooltip("Should the character drop when you let go of jump?")] private bool _enableVariableJumpHeight;
    [SerializeField, Range(1f, 10f)][Tooltip("Gravity multiplier when you let go of jump")] private float _jumpCutOff;
    [SerializeField, Range(1f, 50f)][Tooltip("The fastest speed the character can fall")] private float _fallSpeedLimit;
    [SerializeField, Range(0f, 0.3f)][Tooltip("How long should coyote time last?")] private float _coyoteTime = 0.15f;
    [SerializeField, Range(0f, 0.3f)][Tooltip("How far from ground should we cache your jump?")] private float _jumpBuffer = 0.15f;
    #endregion

    #region JUMP CONTROL VARIABLES
    [Header("Jump Control Variables")]
    [SerializeField][Tooltip("The player's jump speed")] private float _jumpSpeed;
    [SerializeField][Tooltip("The default gravity scale")] private float _defaultGravityScale = 1f;
    [SerializeField][Tooltip("How much will the gravity scale be scaled by?")] private float _gravityMultiplier;
    [SerializeField][Tooltip("Switch to execute jump")] private bool _executeJump;
    [SerializeField][Tooltip("Counter that tracks the time to determine if a jump should be queued")] private float _jumpBufferCounter;
    [SerializeField][Tooltip("Counter that tracks the time to determine if a coyote jump should be executed")] private float _coyoteTimeCounter = 0;

    #endregion

    #region INPUT VARIABLES
    [Header("Input Variables")]
    [SerializeField] private bool _pressingMove;
    [SerializeField] private bool _pressedJump;
    [SerializeField] private bool _pressingJump;
    [SerializeField] private float _horizontalInput;
    #endregion

    private void Awake()
    {
        _rigidbody2D = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        // Used to stop movement when the character is playing certain animations
        if (!_canPlayerMove)
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
        _targetVelocity = new Vector2(_horizontalInput, 0f) * (_maxSpeed * (1 - Mathf.Lerp(0, _friction, Convert.ToSingle(_isTurning))));

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
        float acceleration = _isOnGround ? _maxAcceleration : _maxAirAcceleration;
        float deceleration = _isOnGround ? _maxDecceleration : _maxAirDeceleration;
        float turnSpeed = _isOnGround ? _maxTurnSpeed : _maxAirTurnSpeed;

        float maxSpeedChange;

        if (_pressingMove)
        {
            //If the sign (i.e. positive or negative) of our input direction doesn't match our movement, it means we're turning around and so should use the turn speed.
            if (Mathf.Sign(_horizontalInput) != Mathf.Sign(_velocity.x))
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
        if (_isOnGround || (_coyoteTimeCounter > 0.03f && _coyoteTimeCounter < _coyoteTime) || _canJumpAgain)
        {
            _pressedJump = false;
            _jumpBufferCounter = 0;
            _coyoteTimeCounter = 0;

            //If we have double jump on, allow us to jump again (but only once)
            _canJumpAgain = (_maxNumberJumps == 1 && _canJumpAgain == false);

            // Determine the power of the jump, based on our gravity and stats
            _jumpSpeed = Mathf.Sqrt(-2f * Physics2D.gravity.y * _rigidbody2D.gravityScale * _maxJumpHeight);

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

        if (_jumpBuffer == 0)
        {
            //If we don't have a jump buffer, then turn off desiredJump immediately after hitting jumping
            _pressedJump = false;
        }
    }

    private void SetGravityScale()
    {
        Vector2 gravity = new(0, -2 * _maxJumpHeight / (_timeToJumpApex * _timeToJumpApex));
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
                _gravityMultiplier = _defaultGravityScale;
            }
            else
            {
                //If we're using variable jump height...)
                if (_enableVariableJumpHeight)
                {
                    // Apply upward multiplier if player is rising and holding jump
                    if (_pressingJump && _isJumping)
                    {
                        _gravityMultiplier = _upwardGravityMultiplier;
                    }
                    // Apply a special downward multiplier if the player lets go of jump
                    else
                    {
                        _gravityMultiplier = _jumpCutOff;
                    }
                }
                else
                {
                    _gravityMultiplier = _upwardGravityMultiplier;
                }
            }
        }

        //Else if going down...
        else if (_rigidbody2D.velocity.y < -0.01f)
        {
            if (_isOnGround)
            // Don't change it if player is standing on something (such as a moving platform)
            {
                _gravityMultiplier = _defaultGravityScale;
            }
            else
            {
                // Otherwise, apply the downward gravity multiplier
                _gravityMultiplier = _downwardGravityMultiplier;
            }

        }
        //Else not moving vertically at all
        else
        {
            if (_isOnGround)
            {
                _isJumping = false;
            }

            _gravityMultiplier = _defaultGravityScale;
        }

        //Set the character's Rigidbody's velocity
        //But clamp the Y variable within the bounds of the speed limit, for the terminal velocity assist option
        _rigidbody2D.velocity = new Vector3(_velocity.x, Mathf.Max(_velocity.y, -_fallSpeedLimit));
    }

    private void HandleJumpBuffering()
    {
        //Jump buffer allows us to queue up a jump, which will play when we next hit the ground
        if (_jumpBuffer > 0)
        {
            //Instead of immediately turning off _pressedJump, start counting up...
            //All the while, the Jump function will repeatedly be fired off
            if (_pressedJump)
            {
                _jumpBufferCounter += Time.deltaTime;

                if (_jumpBufferCounter > _jumpBuffer)
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
        return Physics2D.Raycast(origin + _colliderOffset, Vector2.down, _groundThreshold, _groundLayers) || Physics2D.Raycast(origin - _colliderOffset, Vector2.down, _groundThreshold, _groundLayers);
    }

    private void OnDrawGizmos()
    {
        //Draw the ground colliders on screen for debug purposes
        Vector3 origin = transform.position + new Vector3(0, 0.5f, 0);
        if (_isOnGround) { Gizmos.color = Color.green; } else { Gizmos.color = Color.red; }
        Gizmos.DrawLine(origin + _colliderOffset, origin + _colliderOffset + Vector3.down * _groundThreshold);
        Gizmos.DrawLine(origin - _colliderOffset, origin - _colliderOffset + Vector3.down * _groundThreshold);
    }
}
