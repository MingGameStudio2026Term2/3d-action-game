using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class ZombieNavMeshController : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform targetPosition;

    [Header("Animation")]
    [SerializeField] private Animator animator;
    [SerializeField] private string speedParameterName = "speed";
    [SerializeField] private string attackTriggerName = "attack";

    [Header("Movement")]
    [SerializeField] private bool updateDestinationContinuously = true;
    [SerializeField] private float destinationUpdateInterval = 0.25f;

    [Header("Attack")]
    [SerializeField] private float attackStartDistance = 1.75f;
    [SerializeField] private float attackInterval = 1f;

    private NavMeshAgent agent;
    private float updateTimer;
    private float attackTimer;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();

        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }
    }

    private void Start()
    {
        SetDestinationToTarget();
    }

    private void Update()
    {
        if (targetPosition == null)
        {
            UpdateAnimatorSpeed();
            return;
        }

        attackTimer -= Time.deltaTime;

        float distanceToTarget = Vector3.Distance(transform.position, targetPosition.position);
        bool isInAttackRange = distanceToTarget <= attackStartDistance;

        if (isInAttackRange)
        {
            if (agent.isOnNavMesh)
            {
                agent.isStopped = true;
            }

            TryAttackTarget();
        }
        else
        {
            if (agent.isOnNavMesh)
            {
                agent.isStopped = false;
            }

            UpdateDestinationIfNeeded();
        }

        UpdateAnimatorSpeed();
    }

    private void UpdateDestinationIfNeeded()
    {
        if (!updateDestinationContinuously)
        {
            return;
        }

        updateTimer -= Time.deltaTime;
        if (updateTimer <= 0f)
        {
            SetDestinationToTarget();
            updateTimer = destinationUpdateInterval;
        }
    }

    private void UpdateAnimatorSpeed()
    {
        if (animator == null)
        {
            return;
        }

        float speed = 0f;
        if (agent != null && agent.isOnNavMesh)
        {
            speed = agent.velocity.magnitude;
        }

        animator.SetFloat(speedParameterName, speed);
    }

    private void TryAttackTarget()
    {
        if (attackTimer > 0f)
        {
            return;
        }

        AttackTargetGameObject();
        attackTimer = attackInterval;
    }

    public void AttackTargetGameObject()
    {
        if (animator == null)
        {
            return;
        }

        Vector3 lookDirection = targetPosition.position - transform.position;
        lookDirection.y = 0f;
        if (lookDirection.sqrMagnitude > 0.0001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 0.35f);
        }

        animator.SetTrigger(attackTriggerName);
    }

    public void SetTarget(Transform newTarget)
    {
        targetPosition = newTarget;
        SetDestinationToTarget();
    }

    public void SetDestinationToTarget()
    {
        if (agent == null || targetPosition == null)
        {
            return;
        }

        if (!agent.isOnNavMesh)
        {
            Debug.LogWarning($"{name}: NavMeshAgent is not on a baked NavMesh.", this);
            return;
        }

        agent.SetDestination(targetPosition.position);
    }

    private void OnDrawGizmosSelected()
    {
        if (targetPosition == null)
        {
            return;
        }

        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, targetPosition.position);
        Gizmos.DrawSphere(targetPosition.position, 0.2f);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackStartDistance);
    }
}
