using System;
using UnityEngine;
using Pathfinding;
using UnityEngine.Serialization;

public class Soul : MonoBehaviour
{
    // The player GameObject that the soul is following
    private static GameObject _player;
    
    public static GameObject Player
    {
        get => _player;
        set => _player = value;
    }
    // The portal GameObject that the soul will go to if close enough
    public GameObject portal; 

    // The minimum distance to consider a waypoint as reached
    private const float NextWaypointDistance = 1f; 
    // The distance threshold to trigger the faster speed in case the soul is either close enough to the portal
    // or far enough from the player
    private const float FastSpeedDistanceThreshold = 5f; 
    // The maximum distance in which the soul can "see" the player
    public float maximumDistanceToPlayer = 10f;
    // The maximum distance in which the soul can "see" the portal
    public float maximumDistanceToPortal = 5f;

    private const float NormalSpeed = 250; // The normal movement speed of the soul
    private const float FastSpeed = 400; // The faster movement speed of the soul

    [SerializeField] private float speed;

    private Path _path; // The calculated path for the soul to follow
    private int _currentWaypoint; // The index of the current waypoint in the path
    // [SerializeField] private bool reachedEndOfPath = false; // Indicates if the soul has reached the end of its path

    private Seeker _seeker; // A component for pathfinding calculations
    private Rigidbody2D _rb; // The Rigidbody2D component for the soul's physics

    private Target _target;
    
    // Variables to store line points for drawing gizmos
    public LayerMask obstacleLayers;

    private struct Target
    {
        public Transform Transform;
        public float Distance;
        public string Type;
    }
    
    private void Awake()
    {
        _seeker = GetComponent<Seeker>(); // Get the Seeker component attached to this GameObject
        _rb = GetComponent<Rigidbody2D>(); // Get the Rigidbody2D component attached to this GameObject

        // Call UpdatePath method repeatedly with a delay of 0.2 seconds
        InvokeRepeating(nameof(UpdatePath), 0f, .2f);
    }

    private bool IsTargetInView(GameObject target, float sightRange)
    {
        if (target == null)
            return false;
        
        Vector2 observerPosition = transform.position;
        Vector2 targetPosition = target.transform.position;

        // Check distance
        if (Vector2.Distance(observerPosition, targetPosition) < sightRange)
            return true;

        // Check line of sight
        Vector2 directionToTarget = (targetPosition - observerPosition).normalized;
        RaycastHit2D hit = Physics2D.Raycast(observerPosition, directionToTarget, sightRange, obstacleLayers);

        if (hit.collider != null && hit.collider.gameObject == target)
        {
            return Vector2.Distance(observerPosition, targetPosition) > sightRange;
        }

        return false;
    }

    // Select the closest player ghost (left or right) for the soul to follow or the portal if it is close enough
    private Target SelectClosestTarget()
    {
        // Default closest target is the soul itself
        Target closestTarget = new()
        {
            Transform = transform,
            Distance = 0f,
            Type = "self"
        };

        // If player game object is not defined, return an empty target
        if (_player == null) 
            return closestTarget;
        
        var leftGhost = _player.transform.Find("Left Ghost").gameObject; // Find the left player ghost
        var rightGhost = _player.transform.Find("Right Ghost").gameObject; // Find the right player ghost

        // Calculate distances to both ghosts and the portal
        var position = transform.position;
        var distanceToLeftGhost = Vector2.Distance(position, leftGhost.transform.position);
        var distanceToRightGhost = Vector2.Distance(position, rightGhost.transform.position);
        var minGhostDistance = Math.Min(distanceToLeftGhost, distanceToRightGhost);
        var closestGhost = distanceToLeftGhost < distanceToRightGhost ? leftGhost : rightGhost;
        
        if (!IsTargetInView(_player, maximumDistanceToPlayer))
            return closestTarget;

        // Determine the closest player ghost based on distances
        closestTarget.Transform = closestGhost.transform;
        closestTarget.Distance = minGhostDistance;
        closestTarget.Type = "player";

        var distanceToPortal = Vector2.Distance(transform.position, portal.transform.position);
        
        // If the portal is not closer than a certain distance threshold and is not in view, go towards player
        if (!IsTargetInView(portal, maximumDistanceToPortal)) 
            return closestTarget; 
        
        closestTarget.Transform = portal.transform;
        closestTarget.Distance = distanceToPortal;
        closestTarget.Type = "portal";

        return closestTarget;
    }

    // Update the path for the soul to follow
    private void UpdatePath()
    {
        _target = SelectClosestTarget(); // Select the closest target

        if (_seeker.IsDone()) // Check if the Seeker component is not currently calculating a path
            _seeker.StartPath(_rb.position, _target.Transform.position, OnPathComplete); // Start calculating a path to the player
    }

    // Callback function called when path calculation is complete
    private void OnPathComplete(Path p)
    {
        if (p.error) return; // Check if there are no errors in the calculated path
        _path = p; // Store the calculated path
        _currentWaypoint = 0; // Reset the current waypoint index to the start
    }

    private void FixedUpdate()
    {
        if (_path == null) // Check if there is no path to follow.
        {
            return;
        }

        if (_currentWaypoint >= _path.vectorPath.Count) // Check if the soul has reached the end of the path.
        {
            return;
        }
        
        // Calculate the direction and force needed to move towards the current waypoint
        var direction = ((Vector2)_path.vectorPath[_currentWaypoint] - _rb.position).normalized;
        speed = UpdateSpeedToTarget();

        var force = speed * Time.deltaTime * direction;

        var distance = Vector2.Distance(_rb.position, _path.vectorPath[_currentWaypoint]);

        _rb.AddForce(force); // Apply the calculated force to move the soul

        if (distance < NextWaypointDistance) // Check if the soul has reached the current waypoint
        {
            _currentWaypoint++; // Move to the next waypoint in the path
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // If soul collides with portal, destroy game object
        if (collision.gameObject.CompareTag("Portal"))
        {
            Destroy(gameObject);
        }
    }

    private float UpdateSpeedToTarget()
    {
        // Return fast speed if the player is the closest target and the soul is far away, or if the portal is close
        return _target is { Type: "player", Distance: > FastSpeedDistanceThreshold } or
            { Type: "portal", Distance: < FastSpeedDistanceThreshold }
            ? FastSpeed
            : NormalSpeed;
    }
}