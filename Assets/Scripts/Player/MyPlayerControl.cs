using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyPlayerControl : MonoBehaviour
{
    #region COMPONENTS
    private Rigidbody2D _rigidbody2D;
    [SerializeField][Tooltip("Specify which layers should be treated as ground")] private LayerMask _groundLayers;
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
    [Header("Movement Control Variables")]
    [SerializeField][Tooltip("Can the player move?")] private bool _canPlayerMove = true;
    [SerializeField][Tooltip("Is the player on the ground?")] private bool _isOnGround;
    [SerializeField][Tooltip("Is the player turning?")] bool _isTurning = false;
    [SerializeField][Tooltip("The player's current velocity")] private Vector2 _velocity;
    [SerializeField][Tooltip("The player's target velocity")] private Vector2 _targetVelocity;
    [SerializeField] private float _groundThreshold = 0.95f;
    [SerializeField] private Vector3 _colliderOffset;
    #endregion

    #region INPUT CONTROL VARIABLES
    [SerializeField] private bool _pressingKey;
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
        }
        else
        {
            _horizontalInput = Input.GetAxisRaw("Horizontal");
        }

        //Used to flip the character's sprite when she changes direction
        //Also tells us that we are currently pressing a direction button
        if (_horizontalInput != 0)
        {
            transform.localScale = new Vector3(_horizontalInput > 0 ? 1 : -1, 1, 1);
            _pressingKey = true;
        }
        else
        {
            _pressingKey = false;
        }

        //Calculate's the character's desired velocity - which is the direction you are facing, multiplied by the character's maximum speed
        //Friction is not used in this game
        _targetVelocity = new Vector2(_horizontalInput, 0f) * (_maxSpeed * (1 - Mathf.Lerp(0, _friction, Convert.ToSingle(_isTurning))));
    }

    private void LateUpdate()
    {
        _isOnGround = IsPlayerOnGround();

        _velocity = _rigidbody2D.velocity;

        Run();
    }

    private void Run()
    {
        //Set our acceleration, deceleration, and turn speed stats, based on whether we're on the ground on in the air
        float acceleration = _isOnGround ? _maxAcceleration : _maxAirAcceleration;
        float deceleration = _isOnGround ? _maxDecceleration : _maxAirDeceleration;
        float turnSpeed = _isOnGround ? _maxTurnSpeed : _maxAirTurnSpeed;

        float maxSpeedChange;

        if (_pressingKey)
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

    private bool IsPlayerOnGround()
    {
        return Physics2D.Raycast(transform.position + _colliderOffset, Vector2.down, _groundThreshold, _groundLayers) || Physics2D.Raycast(transform.position - _colliderOffset, Vector2.down, _groundThreshold, _groundLayers);
    }

    private void OnDrawGizmos()
    {
        //Draw the ground colliders on screen for debug purposes
        if (_isOnGround) { Gizmos.color = Color.green; } else { Gizmos.color = Color.red; }
        Gizmos.DrawLine(transform.position + _colliderOffset, transform.position + _colliderOffset + Vector3.down * _groundThreshold);
        Gizmos.DrawLine(transform.position - _colliderOffset, transform.position - _colliderOffset + Vector3.down * _groundThreshold);
    }
}
