using System;
using UnityEngine;
using Pathfinding;

public class SoulControl : MonoBehaviour
{
    public GameObject player; // The player GameObject that the spirit is following.
    public GameObject portal; // The portal GameObject that the spirit will go to if close enough.

    private const float nextWaypointDistance = 1f; // The minimum distance to consider a waypoint as reached.
    private const float yDistanceToPortalThreshold = 1f; // The distance to determine if the spirit can "see" the portal.
    private const float distanceThreshold = 5f; // The distance threshold to trigger the faster speed

    private const float normalSpeed = 250; // The normal movement speed of the spirit.
    private const float fastSpeed = 400; // The faster movement speed of the spirit.

    [SerializeField] private float speed;

    private Path path; // The calculated path for the spirit to follow.
    private int currentWaypoint = 0; // The index of the current waypoint in the path.
    // [SerializeField] private bool reachedEndOfPath = false; // Indicates if the spirit has reached the end of its path.

    Seeker seeker; // A component for pathfinding calculations.
    Rigidbody2D rb; // The Rigidbody2D component for the spirit's physics.

    private Target target;

    private struct Target
    {
        public Transform transform;
        public float distance;
        public string type;
    }

    // Start is called before the first frame update
    void Start()
    {
        seeker = GetComponent<Seeker>(); // Get the Seeker component attached to this GameObject.
        rb = GetComponent<Rigidbody2D>(); // Get the Rigidbody2D component attached to this GameObject.

        // Call UpdatePath method repeatedly with a delay of 0.5 seconds.
        InvokeRepeating(nameof(UpdatePath), 0f, .5f);
    }

    public bool IsPortalInView()
    {
        // Get the y distance between the spirit and the portal.
        float yDistanceToPortal = Math.Abs(transform.position.y - portal.transform.position.y);

        // If that distance is small enough, then the portal is in view.
        if (yDistanceToPortal < yDistanceToPortalThreshold)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    // Select the closest player ghost (left or right) for the spirit to follow or the portal if it is close enough.
    private Target SelectClosestTarget()
    {
        Target closestTarget = new();

        Transform leftGhost = player.transform.Find("Left Ghost"); // Find the left player ghost.
        Transform rightGhost = player.transform.Find("Right Ghost"); // Find the right player ghost.

        // Calculate distances to both ghosts and the portal.
        float distanceToLeftGhost = Vector2.Distance(transform.position, leftGhost.position);
        float distanceToRightGhost = Vector2.Distance(transform.position, rightGhost.position);

        // Determine the closest player ghost based on distances.
        closestTarget.transform = (distanceToLeftGhost < distanceToRightGhost) ? leftGhost : rightGhost;
        closestTarget.distance = Math.Min(distanceToLeftGhost, distanceToRightGhost);
        closestTarget.type = "player";

        float distanceToPortal = Vector2.Distance(transform.position, portal.transform.position);

        // If the portal is closer than a certain 
        if (distanceToPortal < distanceThreshold && IsPortalInView())
        {
            closestTarget.transform = portal.transform;
            closestTarget.distance = distanceToPortal;
            closestTarget.type = "portal";
        }

        return closestTarget;
    }

    // Update the path for the spirit to follow.
    void UpdatePath()
    {
        target = SelectClosestTarget(); // Select the closest target.

        if (seeker.IsDone()) // Check if the Seeker component is not currently calculating a path.
            seeker.StartPath(rb.position, target.transform.position, OnPathComplete); // Start calculating a path to the player.
    }

    // Callback function called when path calculation is complete.
    void OnPathComplete(Path p)
    {
        if (!p.error) // Check if there are no errors in the calculated path.
        {
            path = p; // Store the calculated path.
            currentWaypoint = 0; // Reset the current waypoint index to the start.
        }
    }

    void FixedUpdate()
    {
        if (path == null) // Check if there is no path to follow.
        {
            return;
        }

        if (currentWaypoint >= path.vectorPath.Count) // Check if the spirit has reached the end of the path.
        {
            // reachedEndOfPath = true;
            return;
        }
        else
        {
            // reachedEndOfPath = false;
        }

        // Calculate the direction and force needed to move towards the current waypoint.
        Vector2 direction = ((Vector2)path.vectorPath[currentWaypoint] - rb.position).normalized;
        speed = UpdateSpeedToTarget();

        Vector2 force = speed * Time.deltaTime * direction;

        float distance = Vector2.Distance(rb.position, path.vectorPath[currentWaypoint]);

        rb.AddForce(force); // Apply the calculated force to move the spirit.

        if (distance < nextWaypointDistance) // Check if the spirit has reached the current waypoint.
        {
            currentWaypoint++; // Move to the next waypoint in the path.
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // If spirit collides with portal, destroy gameobject.
        if (collision.gameObject.CompareTag("Portal"))
        {
            Destroy(gameObject);
        }
    }

    private float UpdateSpeedToTarget()
    {
        // If the portal is the closest target.
        if (target.type == "portal" && target.distance < distanceThreshold)
        {
            return fastSpeed;
        }
        // If the player is the closest target and the spirit is far away.
        else if (target.type == "player" && target.distance > distanceThreshold)
        {
            return fastSpeed;
        }
        else
        {
            return normalSpeed;
        }
    }
}