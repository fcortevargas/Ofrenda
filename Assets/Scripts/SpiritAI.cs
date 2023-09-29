using System;
using UnityEngine;
using Pathfinding;

public class SpiritAI : MonoBehaviour
{
    public GameObject player; // The player GameObject that the spirit is following.
    public GameObject portal; // The portal GameObject that the spirit will go to if close enough.

    public float speed; // The movement speed of the spirit.
    public float nextWaypointDistance = 3f; // The minimum distance to consider a waypoint as reached.
    public float distanceToPortalThreshold; // The distance threshold to consider the portal as "close enough".
    public float yDistanceToPortalThreshold; // The distance to determine if the spirit can "see" the portal.
    public float speedToPortal; // The movement speed once the spirit "sees" the portal

    Path path; // The calculated path for the spirit to follow.
    int currentWaypoint = 0; // The index of the current waypoint in the path.
    //bool reachedEndOfPath = false; // Indicates if the spirit has reached the end of its path.

    Seeker seeker; // A component for pathfinding calculations.
    Rigidbody2D rb; // The Rigidbody2D component for the spirit's physics.

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
    public Transform SelectTarget()
    {
        Transform leftGhost = player.transform.Find("Left Ghost"); // Find the left player ghost.
        Transform rightGhost = player.transform.Find("Right Ghost"); // Find the right player ghost.

        // Calculate distances to both ghosts and the portal.
        float distanceToLeftGhost = Vector2.Distance(transform.position, leftGhost.position);
        float distanceToRightGhost = Vector2.Distance(transform.position, rightGhost.position);
        float distanceToPortal = Vector2.Distance(transform.position, portal.transform.position);

        // Determine the closest player ghost based on distances.
        Transform closestTarget = (distanceToLeftGhost < distanceToRightGhost) ? leftGhost : rightGhost;

        // If the portal is closer than a certain 
        if (distanceToPortal < distanceToPortalThreshold && IsPortalInView())
        {
            closestTarget = portal.transform;
            speed = speedToPortal;
        }

        return closestTarget;
    }

    // Update the path for the spirit to follow.
    void UpdatePath()
    {
        Transform targetTransform = SelectTarget(); // Select the closest target.

        if (seeker.IsDone()) // Check if the Seeker component is not currently calculating a path.
            seeker.StartPath(rb.position, targetTransform.position, OnPathComplete); // Start calculating a path to the player.
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
            //reachedEndOfPath = true;
            return;
        }
        else
        {
            //reachedEndOfPath = false;
        }

        // Calculate the direction and force needed to move towards the current waypoint.
        Vector2 direction = ((Vector2)path.vectorPath[currentWaypoint] - rb.position).normalized;
        Vector2 force = speed * Time.deltaTime * direction;

        rb.AddForce(force); // Apply the calculated force to move the spirit.

        float distance = Vector2.Distance(rb.position, path.vectorPath[currentWaypoint]);

        if (distance < nextWaypointDistance) // Check if the spirit has reached the current waypoint.
        {
            currentWaypoint++; // Move to the next waypoint in the path.
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // If spirit collides with portal, destroy gameobject.
        if (collision.gameObject.CompareTag("Portal"))
        {
            Destroy(gameObject);
        }
    }
}