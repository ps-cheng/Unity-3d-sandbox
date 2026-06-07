using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    NavMeshAgent agent;
    Transform player;
    Animator animator;

    enum State { Patrol, Chase, Attack }
    State currentState = State.Patrol;

    [SerializeField] Transform[] patrolPoints;
    int currentPatrolIndex = 0;

    [SerializeField] float waitTime = 2f;
    float waitCounter = 0f;
    bool isWaiting = false;

    [SerializeField] float detectionRange = 5f;
    [SerializeField] float chaseRange = 8f;
    [SerializeField] float attackRange = 2f;
    [SerializeField] float attackCoolDown = 1.5f;
    float attackTimer = 0f;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        player = GameObject.FindWithTag("Player").transform;
        if (patrolPoints.Length > 0)
            agent.SetDestination(patrolPoints[0].position);
        agent.stoppingDistance = 0f;
    }

    // Update is called once per frame
    void Update()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (currentState == State.Patrol && distanceToPlayer < detectionRange)
            currentState = State.Chase;
        else if (currentState == State.Chase && distanceToPlayer > chaseRange)
        {
            currentState = State.Patrol;
            isWaiting = false;
            waitCounter = 0f;
            agent.SetDestination(patrolPoints[currentPatrolIndex].position);
        }
        else if (currentState == State.Chase && distanceToPlayer < attackRange)
        {
            currentState = State.Attack;
            isWaiting = false;
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
            agent.ResetPath();
        }
        else if (currentState == State.Attack && distanceToPlayer > attackRange + 0.4f)
        {
            currentState = State.Chase;
            isWaiting = false;
            agent.isStopped = false;
            attackTimer = 0;
        }

        if (currentState == State.Patrol)
            Patrol();
        else if (currentState == State.Chase)
            Chase();
        else if (currentState == State.Attack)
            Attack();
    }

    void Chase()
    {
        animator.SetFloat("Speed", 2f);
        agent.SetDestination(player.position);
        if (agent.pathStatus == NavMeshPathStatus.PathInvalid)
            agent.ResetPath();
        Debug.DrawLine(transform.position, agent.destination, Color.green);
    }

    void Patrol()
    {
        if (patrolPoints.Length == 0) return;

        if (isWaiting)
        {
            animator.SetFloat("Speed", 0f);
            waitCounter += Time.deltaTime;
            if (waitCounter > waitTime)
            {
                isWaiting = false;
                waitCounter = 0f;
                currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
                agent.SetDestination(patrolPoints[currentPatrolIndex].position);
            }
            return;
        }
        animator.SetFloat("Speed", 1f);
        if (agent.hasPath && agent.remainingDistance < 0.5f)
            isWaiting = true;
    }

    void Attack()
    {
        attackTimer += Time.deltaTime;
        if (attackTimer >= attackCoolDown)
        {
            animator.SetTrigger("Attack");
            attackTimer = 0f;
        }
    }
}