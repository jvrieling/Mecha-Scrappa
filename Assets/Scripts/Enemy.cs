using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Enemy : MonoBehaviour, IDestructible
{
    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float currentHealth;
    [SerializeField] private GameObject deathPrefab; // Explode particle/prefab

    [Header("Targeting Settings")]
    [Tooltip("Minimum distance from current position when picking a target point.")]
    [SerializeField] private float minTargetDistance = 20f;
    [Tooltip("Maximum distance from current position when picking a target point.")]
    [SerializeField] private float maxTargetDistance = 100f;
    [Tooltip("Distance threshold to consider target reached.")]
    [SerializeField] private float arrivalDistance = 5f;

    [Header("Movement Physics Settings")]
    [Tooltip("Force applied to propel the enemy forward toward its target.")]
    [SerializeField] private float engineForce = 15f;
    [Tooltip("Torque applied to rotate the enemy toward its target point.")]
    [SerializeField] private float rotationSpeed = 10f;
    [Tooltip("Dot product threshold required to apply forward propulsion (e.g., 0.8 means within ~36° of facing target).")]
    [Range(0f, 1f)]
    [SerializeField] private float facingThreshold = 0.8f;

    [Header("Junk Spawning Settings")]
    [SerializeField] private GameObject junkPrefab;
    [SerializeField] private float minSpawnInterval = 3f;
    [SerializeField] private float maxSpawnInterval = 5f;

    private Rigidbody2D rb;
    private Vector2 currentTargetPoint;
    private float nextSpawnTime;

    // IDestructible Interface Implementation
    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        currentHealth = maxHealth;

        // Ensure 0G environment damping settings
        rb.gravityScale = 0f;
        rb.linearDamping = 1f;
        rb.angularDamping = 1f;
    }

    private void Start()
    {
        PickNewTarget();
        ScheduleNextJunkSpawn();
    }

    private void Update()
    {
        // Handle Junk spawning logic
        if (Time.time >= nextSpawnTime)
        {
            SpawnJunk();
            ScheduleNextJunkSpawn();
        }
    }

    private void FixedUpdate()
    {
        float distanceToTarget = Vector2.Distance(rb.position, currentTargetPoint);

        // Pick a new target if within arrival threshold
        if (distanceToTarget <= arrivalDistance)
        {
            PickNewTarget();
            return;
        }

        // 1. Calculate direction to target
        Vector2 directionToTarget = (currentTargetPoint - rb.position).normalized;

        // 2. Smoothly rotate toward target position
        float targetAngle = Mathf.Atan2(directionToTarget.y, directionToTarget.x) * Mathf.Rad2Deg - 90f;
        float angleDifference = Mathf.DeltaAngle(rb.rotation, targetAngle);

        rb.AddTorque(angleDifference * rotationSpeed * Time.fixedDeltaTime);

        // 3. Propel forward ONLY if mostly facing the target point
        // Using Vector3.Dot to check alignment between facing direction (transform.up) and directionToTarget
        float alignment = Vector3.Dot(transform.up, directionToTarget);

        if (alignment >= facingThreshold)
        {
            // Always pushes transform.up (forward), so if thrown off course by collisions it accelerates in its current facing direction
            rb.AddForce(transform.up * engineForce, ForceMode2D.Force);
        }
    }

    // IDestructible Interface Implementation
    public void TakeDamage(float damageAmount)
    {
        if (damageAmount <= 0f) return;

        currentHealth -= damageAmount;

        // Trigger screen shake on damage
        CameraFollow.Shake();

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    private void Die()
    {
        if (deathPrefab != null)
        {
            Instantiate(deathPrefab, transform.position, transform.rotation);
        }

        Destroy(gameObject);
    }

    private void PickNewTarget()
    {
        Vector2 randomDirection = Random.insideUnitCircle.normalized;
        float randomDistance = Random.Range(minTargetDistance, maxTargetDistance);

        currentTargetPoint = rb.position + (randomDirection * randomDistance);
    }

    private void SpawnJunk()
    {
        if (junkPrefab != null)
        {
            Instantiate(junkPrefab, transform.position, transform.rotation);
        }
    }

    private void ScheduleNextJunkSpawn()
    {
        nextSpawnTime = Time.time + Random.Range(minSpawnInterval, maxSpawnInterval);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(currentTargetPoint, arrivalDistance);
        Gizmos.DrawLine(transform.position, currentTargetPoint);
    }
}