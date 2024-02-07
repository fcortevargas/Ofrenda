using System;
using System.Linq;
using Pathfinding;
using UnityEngine;

namespace Soul
{
    public class Soul : MonoBehaviour
    {
        // The player GameObject that the soul is following
        public static GameObject Player { get; set; }

        // The portal GameObject that the soul will go to if close enough
        public GameObject portal; 

        // The minimum distance to consider a waypoint as reached
        private const float NextWaypointDistance = 1f; 
        // The maximum distances in the x and y direction in which the soul can "see" the player
        public Vector2 maximumDistanceToPlayer = new(20f, 8f);
        // The maximum distances in the x and y direction in which the soul can "see" the portal
        public Vector2 maximumDistanceToPortal = new(20f, 5f);

        [SerializeField] private float speed = 250;

        private Path _path; // The calculated path for the soul to follow
        private int _currentWaypoint; // The index of the current waypoint in the path
        // [SerializeField] private bool reachedEndOfPath = false; // Indicates if the soul has reached the end of its path

        private Seeker _seeker; // A component for pathfinding calculations
        private Rigidbody2D _rb; // The rigid body component for the soul's physics

        private Target _target;

        public LayerMask obstacleLayers;
        public LayerMask playerLayers;

        public class Target
        {
            public Transform Transform { get; set; }
            public float Distance { get; set; }
            public string Type { get; set; }

            // Static property for an invalid target
            public static Target Invalid => new() { Type = "Invalid" };

            // Method to check if the target is valid
            public bool IsValid => Type != "Invalid";
        }
    
        private void Awake()
        {
            _seeker = GetComponent<Seeker>(); // Get the Seeker component attached to this GameObject
            _rb = GetComponent<Rigidbody2D>(); // Get the Rigidbody2D component attached to this GameObject

            // Call UpdatePath method repeatedly with a delay of 0.2 seconds
            InvokeRepeating(nameof(UpdatePath), 0f, .2f);
        }

        private bool IsTargetInView(GameObject target, Vector2 sightRange)
        {
            if (target == null)
                return false;

            Vector2 observerPosition = transform.position;
            Vector2 targetPosition = target.transform.position;
            var distanceToTarget = targetPosition - observerPosition;

            // Check if the target is within the sight range along both x and y axes
            return Mathf.Abs(distanceToTarget.x) <= sightRange.x && Mathf.Abs(distanceToTarget.y) <= sightRange.y;
        }

        private bool IsPathValid()
        {
            var currentPosition = transform.position;
            var roundedCurrentPositionX = Mathf.Round(currentPosition.x);
            var velocityX = _rb.velocity.x;

            var isTileValidPosX = GameManager.Instance.ModifiedWorldTiles.Any(
                vec => Mathf.Approximately(vec.x, roundedCurrentPositionX));

            // var isTileValidPosY = Mathf.Abs(Player.transform.position.y + 0.6f - currentPosition.y) < 5f;
            //
            // var isTileValidVelX = GameManager.Instance.ModifiedWorldTiles.Any(vec =>
            // {
            //     var targetX = roundedCurrentPositionX + Math.Sign(velocityX);
            //     return Mathf.Approximately(vec.x, targetX);
            // });

            // return isTileValidPosX && isTileValidVelX && isTileValidPosY;
            return isTileValidPosX;
        }
        
        private Target CreateSelfTarget()
        {
            return new Target
            {
                Transform = transform,
                Distance = 0f,
                Type = "self"
            };
        }
        
        private Target GetClosestObstacleGhostTarget()
        {
            var position = transform.position;
            var obstacle = GameObject.Find("Obstacles");

            if (obstacle == null)
            {
                Debug.LogWarning("No obstacles found on the scene.");
                return Target.Invalid; // Return an invalid target
            }
            
            var leftGhost = obstacle.transform.Find("Left Ghost").gameObject;
            var rightGhost = obstacle.transform.Find("Right Ghost").gameObject;

            var distanceToLeft = Vector2.Distance(position, leftGhost.transform.position);
            var distanceToRight = Vector2.Distance(position, rightGhost.transform.position);

            var isLeftCloser = distanceToLeft < distanceToRight;

            var hit = Physics2D.Raycast(position, isLeftCloser ? Vector2.right : Vector2.left, 1.5f, obstacleLayers);

            if (hit.collider == null)
            {
                return Target.Invalid;
            }
            
            return new Target
            {
                Transform = isLeftCloser ? leftGhost.transform : rightGhost.transform,
                Distance = isLeftCloser ? distanceToLeft : distanceToRight,
                Type = "obstacle"
            };
        }
        
        private Target GetClosestPlayerGhostTarget()
        {
            var position = transform.position;
            
            var leftGhost = Player.transform.Find("Left Ghost").gameObject;
            var rightGhost = Player.transform.Find("Right Ghost").gameObject;

            var distanceToLeft = Vector2.Distance(position, leftGhost.transform.position);
            var distanceToRight = Vector2.Distance(position, rightGhost.transform.position);

            return new Target
            {
                Transform = distanceToLeft < distanceToRight ? leftGhost.transform : rightGhost.transform,
                Distance = Math.Min(distanceToLeft, distanceToRight),
                Type = "player"
            };
        }
        
        private Target CreatePortalTarget()
        {
            var distanceToPortal = Vector2.Distance(transform.position, portal.transform.position);

            return new Target
            {
                Transform = portal.transform,
                Distance = distanceToPortal,
                Type = "portal"
            };
        }

        // Select the closest player ghost (left or right) for the soul to follow or the portal if it is close enough
        private Target SelectTarget()
        {
            if (Player == null || !IsPathValid() || !IsTargetInView(Player, maximumDistanceToPlayer))
            {
                return CreateSelfTarget();
            }

            var closestObstacleTarget = GetClosestObstacleGhostTarget();
            if (closestObstacleTarget.IsValid)
            {
                // Perform a ray cast towards the player to check if they are now in view
                var position = transform.position;
                Vector2 directionToPlayer = (Player.transform.position - position).normalized;
                var hit = Physics2D.Raycast(position, directionToPlayer, maximumDistanceToPlayer.magnitude, playerLayers);

                // If the ray cast hits the player, it means the player is in view and should be considered as the closest target
                if (hit.collider != null && hit.collider.gameObject == Player)
                {
                    return GetClosestPlayerGhostTarget(); // Recalculate and return the player as the closest target
                }

                // Otherwise, return the previously selected obstacle target
                return closestObstacleTarget;
            }

            // If no obstacle target was selected or it's invalid, proceed with the rest of the target selection logic...
            var closestPlayerGhostTarget = GetClosestPlayerGhostTarget();
            if (closestPlayerGhostTarget.IsValid && !IsTargetInView(portal, maximumDistanceToPortal))
            {
                return closestPlayerGhostTarget;
            }

            return CreatePortalTarget();
        }


        // Update the path for the soul to follow
        private void UpdatePath()
        {
            _target = SelectTarget(); // Select the closest target based on current conditions

            if (_seeker.IsDone()) // Ensure the previous path calculation is complete
            {
                _seeker.StartPath(_rb.position, _target.Transform.position, OnPathComplete); // Request a new path to the target
            }
        }

        // Callback function called when path calculation is complete
        private void OnPathComplete(Path path)
        {
            // Ensure there are no errors with the path
            if (path.error) 
                return; 
            _path = path; // Update the soul's path
            _currentWaypoint = 0; // Reset waypoint index for new path
        }

        private void FixedUpdate()
        {
            if (_path == null || _currentWaypoint >= _path.vectorPath.Count) return; // Check for a valid path and waypoints

            var direction = ((Vector2)_path.vectorPath[_currentWaypoint] - _rb.position).normalized; // Direction to the next waypoint
            var force = direction * (speed * Time.fixedDeltaTime); // Calculate force to apply based on speed and direction

            _rb.AddForce(force); // Apply movement force

            // Check if the soul has reached the current waypoint
            if (Vector2.Distance(_rb.position, _path.vectorPath[_currentWaypoint]) < NextWaypointDistance)
            {
                _currentWaypoint++; // Move to the next waypoint
            }
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            // Check for collision with the portal to trigger some action, like ending the level or teleporting the soul
            if (collision.gameObject.CompareTag("Portal"))
                Destroy(gameObject);
        }
    }
}