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

    //For Attacking
    private bool canSimplePathfind = false;
    private bool canAttack = false;
    private bool isAttacking = false;
    [Tooltip("This should be the same amount of time the attack animation takes")]
    public float attackLengthInSeconds;

    //For Fleeing
    [Tooltip("Amount of time the enemy will flee after attacking")]
    public float fleeTimer;
    private bool isFleeing;

    public float stopDistanceFromPlayer;

    public enum AIState
    {
        BaseState, // doing nothing
        Chasing, // pathing and moving towards player
        Attacking,  // doing attack
        Fleeing // moving away from the player , might not use
    }
    AIState aistate = AIState.BaseState;

    // Movement things
    [SerializeField] private float moveSpeed = 15f;
    private float startMoveSpeed;
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

        startMoveSpeed = moveSpeed;

        aistate = AIState.Chasing;


    }

    void Update()
    {
        CheckPlayerLocation();
        DeleteAllWaypoints(); // TESTING
        CheckCanSimplePathfind();
        if (aistate == AIState.Chasing)                 // Chase Code
        {


            moveSpeed = startMoveSpeed;

            if (canSimplePathfind)
            {
                SimplePathfind();
            }
            else
            {
                Pathfind();
            }

        }
        else if (aistate == AIState.Attacking)        // Attack Code
        {
            moveSpeed = 0;

            if (canAttack)
            {
                if (!isAttacking)
                {
                    // Trigger actual attack here
                    UseAttack();
                    StartCoroutine(WaitforAttackAnimation(attackLengthInSeconds));
                }
            }
        }
        else if (aistate == AIState.Fleeing)          // Flee Code
        {
            moveSpeed = startMoveSpeed/2;

            Flee();

            if (!isFleeing)
            {
                StartCoroutine(FleeTimer(fleeTimer));
            }

        }
        else if (aistate == AIState.BaseState)        // Do Nothing Code
        {
            moveSpeed = 0;
        }



        if (Vector2.Distance(rb.transform.position, player.transform.position) <= stopDistanceFromPlayer && !isFleeing)
        {

            aistate = AIState.Attacking;
            canAttack = true;

        }

    }

    void FixedUpdate()
    {
        lineRenderer.positionCount = (int)(distanceToPlayer / rayCastDistance); // draw more points for higher accuracy
        //UnityEngine.Debug.Log(lineRenderer.positionCount);
        pointsOnLine = lineRenderer.positionCount;
        if (!canSimplePathfind && aistate == AIState.Chasing)
        {
            MoveAlongWaypoints();
        }

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


    private void CheckPlayerLocation()
    {
        playerPosition = player.transform.position;

        distanceToPlayer = Vector2.Distance(rb.position, playerPosition);
        dirToPlayer = (playerPosition - rb.position).normalized;
    }


    //Pathfinding and Chasing// V V V V V
    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    private void MoveAlongWaypoints()
    {
        if (currentWaypointIndex >= wayPoints.Count) return;

        Vector2 targetPosition = wayPoints[currentWaypointIndex];

        // Move toward the current waypoint
        Vector2 moveDirection = (targetPosition - rb.position).normalized;

        rb.MovePosition(rb.position + moveDirection * moveSpeed * Time.fixedDeltaTime);

        // Check if the police AI reached the waypoint
        if (Vector2.Distance(rb.position, targetPosition) <= 0.2)
        {
            currentWaypointIndex++;
        }
    }



    private void CheckCanSimplePathfind()
    {

        CheckPlayerLocation();

        RaycastHit2D hit = Physics2D.Raycast(rb.position, playerPosition, distanceToPlayer);

        if (hit.collider != null && hit.collider.gameObject != player && hit.collider.gameObject != rb)
        {
            canSimplePathfind = false;
        }
        else
        {
            canSimplePathfind = true;
        }
    }

    private void SimplePathfind()
    {
        Vector2 targetPosition = player.transform.position;

        // Move toward the current waypoint
        Vector2 moveDirection = (targetPosition - rb.position).normalized;

        if (Vector2.Distance(rb.transform.position, player.transform.position) < stopDistanceFromPlayer)
        {
            rb.MovePosition(rb.position + moveDirection * moveSpeed * Time.fixedDeltaTime);
        }

    }

    private void Pathfind()
    {
        wayPoints.Clear(); // Clear the list of waypoints

        int wayPointsNeeded = (int)(distanceToPlayer / rayCastDistance); // Number of waypoints needed

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
                else if (!hitRight) // Only the right direction is clear
                {
                    rayCastEndpoint = possibleRightTurn;
                    wayPoints.Add(rayCastEndpoint);
                    foundClearPath = true;
                }
                else if (!hitLeft) // Only the left direction is clear
                {
                    rayCastEndpoint = possibleLeftTurn;
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
            //Debug.Log("Way Point #" + (wayPoints.Count-1));
            //Debug.Log("--------Way Point X: " + wayPoints[wayPoints.Count-1].x + "     Way Point Y: " + wayPoints[wayPoints.Count - 1].y);
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

    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


    //Attacking// V V V V V
    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    IEnumerator WaitforAttackAnimation(float delay)
    {
        isAttacking = true;
        yield return new WaitForSeconds(delay);
        aistate = AIState.Fleeing;
        isAttacking = false;
    }

    private void UseAttack()
    {
        WaterBucketAttack waterAttack = GetComponent<WaterBucketAttack>();
        waterAttack.SpillBucketAttack();
    }

    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



    //Fleeing// V V V V V
    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    IEnumerator FleeTimer(float fleeTime)
    {
        isFleeing = true;
        yield return new WaitForSeconds(fleeTime);
        aistate = AIState.Chasing;
        isFleeing = false;
    }

    private void Flee()
    {
        Pathfind();
        MoveAlongWaypointsOpposite();
    }

    private void MoveAlongWaypointsOpposite()
    {
        if (currentWaypointIndex >= wayPoints.Count) return;

        Vector2 targetPosition = wayPoints[currentWaypointIndex];

        // Move toward the current waypoint
        Vector2 moveDirection = (targetPosition - rb.position).normalized;

        rb.MovePosition(rb.position + -moveDirection * .10f * Time.fixedDeltaTime);

        // Check if the police AI reached the waypoint
        if (Vector2.Distance(rb.position, targetPosition) <= 0.2)
        {
            currentWaypointIndex++;
        }
    }

    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
}
