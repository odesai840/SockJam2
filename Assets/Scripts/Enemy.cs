using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.EventSystems.EventTrigger;

public class Enemy : MonoBehaviour
{
    private GameObject player;
    private Rigidbody2D rb;
    private LineRenderer lineRenderer; 

    Vector2 playerPosition;
    float distanceToPlayer;
    Vector2 dirToPlayer; // direction to player
    public List<Vector2> wayPoints = new List<Vector2>(); // List of all points on path

    [SerializeField] private int maxIterations = 30; // Max iterations for waypoint generation
    private int currentIteration = 0;

    [Tooltip("Lower Number = Higher Accuracy")]
    public float rayCastDistance = 1;
    int pointsOnLine;
    [Tooltip("Radius of circle that police AI will check for collisions")]
    public float checkRadius = 0.5f;

    // Movement things
    [SerializeField] private float moveSpeed = 15f;
    private int currentWaypointIndex = 0;

    // Collision things
    //[Tooltip("Damage dealt to player")]
    //[SerializeField] private float damageToOthers = 10f;
    //[Tooltip("Wait time for damage over time")]
    //[SerializeField] private float DotWaitTime = 0.5f;
    //private Coroutine damageCoroutine;

    // Colliders to avoid
    private Collider2D playerCollider;
    private Collider2D selfCollider;

    // TESTING
    public GameObject waypointCircle;
    private List<GameObject> waypointObjects = new List<GameObject>();

    void Start()
    {
        player = GameObject.FindWithTag("Player");
        rb = GetComponent<Rigidbody2D>();
        lineRenderer = GetComponent<LineRenderer>();

        // Colliders to avoid
        playerCollider = player.GetComponent<Collider2D>();
        selfCollider = GetComponent<Collider2D>();

        CheckPlayerLocation();
        //DeleteAllWaypoints(); // TESTING
        Pathfind();
    }

    void Update()
    {
        //CheckPlayerLocation();
        //DeleteAllWaypoints(); // TESTING
        //Pathfind();
    }

    void FixedUpdate()
    {
        lineRenderer.positionCount = (int)(distanceToPlayer / rayCastDistance); // draw more points for higher accuracy
        //UnityEngine.Debug.Log(lineRenderer.positionCount);
        pointsOnLine = lineRenderer.positionCount;
        //MoveAlongWaypoints();
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        /*
        if (other.collider.CompareTag("Player"))
        {
            PlayerHealth playerHealth = other.collider.GetComponent<PlayerHealth>();
            if (playerHealth && damageCoroutine == null)
            {
                damageCoroutine = StartCoroutine(DealDamageOverTime(playerHealth));
            }
        }
        */
    }

    private void OnCollisionExit2D(Collision2D other)
    {
        /*
        if (other.collider.CompareTag("Player"))
        {
            PlayerHealth playerHealth = other.collider.GetComponent<PlayerHealth>();
            if (playerHealth && damageCoroutine != null)
            {
                StopCoroutine(damageCoroutine);
                damageCoroutine = null;
            }
        }
        */
    }

    /*
    private IEnumerator DealDamageOverTime(PlayerHealth playerHealth)
    {
        while (true)
        {
            playerHealth.TakeDamage(damageToOthers);
            yield return new WaitForSeconds(DotWaitTime);
        }
    }
    */

    private void MoveAlongWaypoints()
    {
        if (currentWaypointIndex >= wayPoints.Count) return;

        Vector2 targetPosition = wayPoints[currentWaypointIndex];

        // Move toward the current waypoint
        Vector2 moveDirection = (targetPosition - rb.position).normalized;
        rb.MovePosition(rb.position + moveDirection * moveSpeed * Time.fixedDeltaTime);

        // Rotate the police AI to face the player
        float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
        rb.rotation = angle - 90f;  // Adjust for sprite rotation

        // Check if the police AI reached the waypoint
        if (Vector2.Distance(rb.position, targetPosition) <= 0.1)
        {
            currentWaypointIndex++;
        }
    }

    private void CheckPlayerLocation()
    {
        playerPosition = player.transform.position;

        distanceToPlayer = Vector2.Distance(rb.position, playerPosition);
        dirToPlayer = (playerPosition - rb.position).normalized;
    }

    private void Pathfind()
    {
        wayPoints.Clear(); // Clear the list of waypoints

        int wayPointsNeeded = (int)(distanceToPlayer / rayCastDistance); // Number of waypoints needed
        Debug.Log("Waypoints needed: " + wayPointsNeeded);
        Vector2 rayCastEndpoint = rb.position + dirToPlayer * rayCastDistance; // Start position
        wayPoints.Add(rayCastEndpoint); // Add start position to the list

        float angleStep = 10.0f; // The angle to add after a failed collision check

        currentIteration = 0;

        for (int i = 0; i < wayPointsNeeded; i++)
        {
            CheckPlayerLocation();

            bool foundClearPath = false;
            float angleOffsetLeft = 0.0f;
            float angleOffsetRight = 0.0f;

            while (!foundClearPath && currentIteration < maxIterations)
            {
                Vector2 previousPoint = rayCastEndpoint; // store current endpoint if need to reverse

                // Calculate possible endpoints by rotating left and right
                Vector2 possibleLeftTurn = rayCastEndpoint + (Vector2)(Quaternion.Euler(0, 0, angleOffsetLeft) * dirToPlayer) * rayCastDistance;
                Vector2 possibleRightTurn = rayCastEndpoint + (Vector2)(Quaternion.Euler(0, 0, angleOffsetRight) * dirToPlayer) * rayCastDistance;

                // Check for collisions for both possible directions
                Collider2D hitLeft = Physics2D.OverlapCircle(possibleLeftTurn, checkRadius);
                Collider2D hitRight = Physics2D.OverlapCircle(possibleRightTurn, checkRadius);

                if (hitLeft && hitLeft != playerCollider && hitLeft != selfCollider)
                {
                    // If there's a collision on the left, increment the left angle offset
                    angleOffsetLeft += angleStep;
                }
                else if (hitRight && hitRight != playerCollider && hitRight != selfCollider)
                {
                    // If there's a collision on the right, decrement the right angle offset
                    angleOffsetRight -= angleStep;
                }

                // Decide which side to turn based off which side wouldnt collide
                if (!hitLeft && !hitRight)
                {
                    // chose closest direction if both good
                    if (Mathf.Abs(angleOffsetLeft) <= Mathf.Abs(angleOffsetRight))
                    {
                        rayCastEndpoint = possibleLeftTurn;
                    }
                    else
                    {
                        rayCastEndpoint = possibleRightTurn;
                    }
                    wayPoints.Add(rayCastEndpoint);
                    foundClearPath = true;
                }
                else if (!hitLeft) // Only the left direction is clear
                {
                    rayCastEndpoint = possibleLeftTurn;
                    wayPoints.Add(rayCastEndpoint);
                    foundClearPath = true;
                }
                else if (!hitRight) // Only the right direction is clear
                {
                    rayCastEndpoint = possibleRightTurn;
                    wayPoints.Add(rayCastEndpoint);
                    foundClearPath = true;
                }
                else // If both directions are blocked, continue adjusting angles
                {
                    angleOffsetLeft += angleStep;
                    angleOffsetRight -= angleStep;
                }

                currentIteration++; // Prevent excessive waypoint generation

                // FOR VISUAL TESTING //
                if (foundClearPath)
                {
                    //wayPoints.Add(rayCastEndpoint); //Moved up into individual else ifs
                    GameObject waypoint = Instantiate(waypointCircle, rayCastEndpoint, Quaternion.identity);
                    waypointObjects.Add(waypoint);
                }
            }
            Debug.Log("Way Point #" + (wayPoints.Count-1));
            Debug.Log("--------Way Point X: " + wayPoints[wayPoints.Count-1].x + "     Way Point Y: " + wayPoints[wayPoints.Count - 1].y);
        }
    }

    // for visual testing, delete visual way points so game doesnt crash instantly
    private void DeleteAllWaypoints()
    {
        foreach (GameObject waypoint in waypointObjects)
        {
            Destroy(waypoint);
        }

        waypointObjects.Clear();
    }
}
