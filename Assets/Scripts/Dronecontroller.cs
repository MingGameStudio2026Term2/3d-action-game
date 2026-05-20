using UnityEngine;

public class Dronecontroller : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform playerTarget;
    [SerializeField] private bool autoFindPlayerByTag = true;
    [SerializeField] private string playerTag = "Player";

    [Header("Follow Settings")]
    [SerializeField] private float followDistance = 3.5f;
    [SerializeField] private float leftRightOffset = 1.5f;
    [SerializeField] private float height = 2.5f;
    [SerializeField] private float positionSmoothTime = 0.15f;

    [Header("Floating")]
    [SerializeField] private float floatAmplitude = 0.35f;
    [SerializeField] private float floatFrequency = 1.8f;

    [Header("Look")]
    [SerializeField] private bool lookAtPlayer = true;
    [SerializeField] private float rotationLerpSpeed = 8f;

    [Header("Shooting")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform bulletSpawnPoint;
    [SerializeField] private float shootInterval = 0.5f;
    [SerializeField] private float maxShootDistance = 25f;

    [Header("Targeting")]
    [SerializeField] private bool autoFindNearestZombie = true;
    [SerializeField] private float retargetInterval = 0.5f;
    [SerializeField] private Transform shootTarget;

    private Vector3 currentVelocity;
    private float shootTimer;
    private float retargetTimer;

    private void Awake()
    {
        if (playerTarget == null && autoFindPlayerByTag)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag(playerTag);
            if (playerObject != null)
            {
                playerTarget = playerObject.transform;
            }
        }
    }

    private void LateUpdate()
    {
        if (playerTarget == null)
        {
            return;
        }

        UpdateShootTarget();
        TryShootAtTarget();

        Vector3 targetPosition = GetDesiredPosition();
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref currentVelocity, positionSmoothTime);

        if (lookAtPlayer)
        {
            Vector3 lookDirection = playerTarget.position - transform.position;
            lookDirection.y = 0f;

            if (lookDirection.sqrMagnitude > 0.001f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(lookDirection.normalized, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationLerpSpeed * Time.deltaTime);
            }
        }
    }

    private void UpdateShootTarget()
    {
        if (!autoFindNearestZombie)
        {
            return;
        }

        retargetTimer -= Time.deltaTime;
        if (retargetTimer > 0f)
        {
            return;
        }

        retargetTimer = retargetInterval;

        ZombieNavMeshController[] zombies = FindObjectsOfType<ZombieNavMeshController>();
        float closestDistanceSqr = float.MaxValue;
        Transform closestZombie = null;

        for (int i = 0; i < zombies.Length; i++)
        {
            if (zombies[i] == null)
            {
                continue;
            }

            Transform zombieTransform = zombies[i].transform;
            float distanceSqr = (zombieTransform.position - transform.position).sqrMagnitude;
            if (distanceSqr < closestDistanceSqr)
            {
                closestDistanceSqr = distanceSqr;
                closestZombie = zombieTransform;
            }
        }

        shootTarget = closestZombie;
    }

    private void TryShootAtTarget()
    {
        if (bulletPrefab == null || shootTarget == null)
        {
            return;
        }

        shootTimer -= Time.deltaTime;
        if (shootTimer > 0f)
        {
            return;
        }

        Vector3 spawnPosition = bulletSpawnPoint != null ? bulletSpawnPoint.position : transform.position;
        float distanceToTarget = Vector3.Distance(spawnPosition, shootTarget.position);
        if (distanceToTarget > maxShootDistance)
        {
            return;
        }

        ShootAtTarget();
        shootTimer = shootInterval;
    }

    public void ShootAtTarget()
    {
        if (bulletPrefab == null || shootTarget == null)
        {
            return;
        }

        Vector3 spawnPosition = bulletSpawnPoint != null ? bulletSpawnPoint.position : transform.position;
        Vector3 direction = (shootTarget.position - spawnPosition).normalized;
        if (direction.sqrMagnitude <= 0.0001f)
        {
            direction = transform.forward;
        }

        Quaternion bulletRotation = Quaternion.LookRotation(direction, Vector3.up);
        Instantiate(bulletPrefab, spawnPosition, bulletRotation);
    }

    private Vector3 GetDesiredPosition()
    {
        Vector3 behind = -playerTarget.forward * followDistance;
        Vector3 side = playerTarget.right * leftRightOffset;
        float bobOffset = Mathf.Sin(Time.time * floatFrequency) * floatAmplitude;
        Vector3 up = Vector3.up * (height + bobOffset);

        return playerTarget.position + behind + side + up;
    }

    public void SetPlayerTarget(Transform newTarget)
    {
        playerTarget = newTarget;
    }

    public void SetShootTarget(Transform newTarget)
    {
        shootTarget = newTarget;
    }
}
