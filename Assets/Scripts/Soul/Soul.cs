using System;
using UnityEngine;
using Pathfinding;

public class Soul : MonoBehaviour
{
    public GameObject player; // The player GameObject that the spirit is following
    public GameObject portal; // The portal GameObject that the spirit will go to if close enough

    private const float NextWaypointDistance = 1f; // The minimum distance to consider a waypoint as reached
    private const float YDistanceToPortalThreshold = 1f; // The distance to determine if the spirit can "see" the portal
    private const float DistanceThreshold = 5f; // The distance threshold to trigger the faster speed

    private const float NormalSpeed = 250; // The normal movement speed of the spirit
    private const float FastSpeed = 400; // The faster movement speed of the spirit

    [SerializeField] private float speed;

    private Path _path; // The calculated path for the spirit to follow
    private int _currentWaypoint; // The index of the current waypoint in the path
    // [SerializeField] private bool reachedEndOfPath = false; // Indicates if the spirit has reached the end of its path

    private Seeker _seeker; // A component for pathfinding calculations
    private Rigidbody2D _rb; // The Rigidbody2D component for the spirit's physics

    private Target _target;

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

        // Call UpdatePath method repeatedly with a delay of 0.5 seconds
        InvokeRepeating(nameof(UpdatePath), 0f, .5f);
    }

    private bool IsPortalInView()
    {
        // Get the y distance between the spirit and the portal
        var yDistanceToPortal = Math.Abs(transform.position.y - portal.transform.position.y);

        // If that distance is small enough, then the portal is in view
        return yDistanceToPortal < YDistanceToPortalThreshold;
    }

    // Select the closest player ghost (left or right) for the spirit to follow or the portal if it is close enough
    private Target SelectClosestTarget()
    {
        Target closestTarget = new();

        var leftGhost = player.transform.Find("Left Ghost"); // Find the left player ghost
        var rightGhost = player.transform.Find("Right Ghost"); // Find the right player ghost

        // Calculate distances to both ghosts and the portal
        var position = transform.position;
        var distanceToLeftGhost = Vector2.Distance(position, leftGhost.position);
        var distanceToRightGhost = Vector2.Distance(position, rightGhost.position);

        // Determine the closest player ghost based on distances
        closestTarget.Transform = (distanceToLeftGhost < distanceToRightGhost) ? leftGhost : rightGhost;
        closestTarget.Distance = Math.Min(distanceToLeftGhost, distanceToRightGhost);
        closestTarget.Type = "player";

        var distanceToPortal = Vector2.Distance(transform.position, portal.transform.position);

        // If the portal is closer than a certain 
        if (!(distanceToPortal < DistanceThreshold) || !IsPortalInView()) return closestTarget; 
        
        closestTarget.Transform = portal.transform;
        closestTarget.Distance = distanceToPortal;
        closestTarget.Type = "portal";

        return closestTarget;
    }

    // Update the path for the spirit to follow
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

        if (_currentWaypoint >= _path.vectorPath.Count) // Check if the spirit has reached the end of the path.
        {
            return;
        }
        
        // Calculate the direction and force needed to move towards the current waypoint
        var direction = ((Vector2)_path.vectorPath[_currentWaypoint] - _rb.position).normalized;
        speed = UpdateSpeedToTarget();

        var force = speed * Time.deltaTime * direction;

        var distance = Vector2.Distance(_rb.position, _path.vectorPath[_currentWaypoint]);

        _rb.AddForce(force); // Apply the calculated force to move the spirit

        if (distance < NextWaypointDistance) // Check if the spirit has reached the current waypoint
        {
            _currentWaypoint++; // Move to the next waypoint in the path
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // If spirit collides with portal, destroy game object
        if (collision.gameObject.CompareTag("Portal"))
        {
            Destroy(gameObject);
        }
    }

    private float UpdateSpeedToTarget()
    {
        // If the portal is the closest target
        if (_target.Type == "portal" && _target.Distance < DistanceThreshold)
        {
            return FastSpeed;
        }

        // If the player is the closest target and the spirit is far away
        return _target is { Type: "player", Distance: > DistanceThreshold } ? FastSpeed : NormalSpeed;
    }
}