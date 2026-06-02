using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Assertions.Must;

public class EnemyAI : MonoBehaviour
{
    NavMeshAgent agent;
    Transform player;

    enum State { Patrol, Chase }
    State currentState = State.Patrol;

    [SerializeField] Transform[] patrolPoints;
    int currentPatrolIndex = 0;

    [SerializeField] float waitTime = 2f;
    float waitCounter = 0f;
    bool isWaiting = false;

    [SerializeField] float detectionRange = 5f;
    [SerializeField] float chaseRange = 8f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        player = GameObject.FindWithTag("Player").transform;
    }

    // Update is called once per frame
    void Update()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if(currentState == State.Patrol && distanceToPlayer < detectionRange)
            currentState = State.Chase;
        else if (currentState == State.Chase && distanceToPlayer > chaseRange)
            currentState = State.Patrol;

        if (currentState == State.Patrol)
            Patrol();
        else
            Chase();        
    }

    void Chase()
    {
        agent.SetDestination(player.position);
        if (agent.pathStatus == NavMeshPathStatus.PathInvalid)
            agent.ResetPath();
    }

    void Patrol()
    {
        if (patrolPoints.Length == 0) return;

        if (isWaiting)
        {
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

        if (agent.remainingDistance < 0.5f)
            isWaiting = true;
    }
}
