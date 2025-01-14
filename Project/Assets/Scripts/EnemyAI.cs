using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class EnemyAI : MonoBehaviour
{
    Transform target;
    Rigidbody2D rbEnemy;
    EnemyCombat combat;
    AIPlayerDetector playerDetector;

    public float speed;
    [SerializeField] float nextWaypointDistance;

    Path path;
    int currentWaypoint = 0;
    Seeker seeker;
    
    Patrol patrol;
    [SerializeField] bool isPatroller;
    bool isPatrolling = true;

    public bool canSeePlayer = false;
    bool isEnableCanSeePlayerCRRunning = false;

    void Start()
    {
        target = GameObject.Find("Player").GetComponent<Transform>();
        combat = GetComponent<EnemyCombat>();
        seeker = GetComponent<Seeker>();
        rbEnemy = GetComponent<Rigidbody2D>();
        playerDetector = GetComponent<AIPlayerDetector>();
        patrol = GetComponent<Patrol>();

        seeker.enabled = false;
        if (!isPatroller)
            StopPatrolling();
    }

    void FixedUpdate()
    {
        //  after the player has been seen
        // - prevent further callings of the CanSeePlayer function
        // - stop patrolling
        // - enable the seeker script
        if (!canSeePlayer)
        {
            if (!isEnableCanSeePlayerCRRunning)
            {
                canSeePlayer = playerDetector.CanSeePlayer();
                if (canSeePlayer)
                {
                    if (isPatrolling)
                        StopPatrolling();
                    EnableSeeker();
                }
            }
        }
        if (canSeePlayer)
        {
            ChasePlayer();
            if (IsPlayerInAttackRange())
                combat.Attack();
        }
    }

    void ChasePlayer()
    {
        if (path == null)
            return;
        if (currentWaypoint >= path.vectorPath.Count) // if reached the end of the path, a.k.a. the player
            return;

        Vector2 direction = ((Vector2)path.vectorPath[currentWaypoint] - rbEnemy.position).normalized;
        Vector2 force = direction * speed * Time.deltaTime;
        rbEnemy.AddForce(force);

        float distance = Vector2.Distance(rbEnemy.position, path.vectorPath[currentWaypoint]);
        if (distance < nextWaypointDistance)
        {
            currentWaypoint++;
        }

        Rotate(rbEnemy.velocity, 1800);
    }

    public void EnableSeeker()
    {
        seeker.enabled = true;
        InvokeRepeating("UpdatePath", 0f, .1f);
    }

    public void DisableSeeker()
    {
        seeker.enabled = false;
        CancelInvoke("UpdatePath");
    }

    public void StopPatrolling()
    {
        Destroy(patrol);
        isPatrolling = false;
    }

    void UpdatePath()
    {
        if (seeker.IsDone())
            seeker.StartPath(rbEnemy.position, target.position, OnPathComplete);
    }

    void OnPathComplete(Path p)
    {
        if (!p.error)
        {
            path = p;
            currentWaypoint = 0;
        }
    }

    public void Rotate(Vector3 moveDirection, int rotationSpeed)
    {
        Quaternion toRotation = Quaternion.LookRotation(Vector3.forward, moveDirection);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, toRotation, rotationSpeed * Time.deltaTime);
    }

    bool IsPlayerInAttackRange()
    {
        float distance = Vector2.Distance(transform.position, target.position);
        if (distance <= combat.attackRange)
            if (!combat.isAttacking)
                return true;
        return false;
    }

    public IEnumerator EnableCanSeePlayer()
    {
        isEnableCanSeePlayerCRRunning = true;

        yield return new WaitForSeconds(1);
        canSeePlayer = true;
        
        isEnableCanSeePlayerCRRunning = false;
    }
}
