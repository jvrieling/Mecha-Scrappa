using UnityEngine;
using System;

[RequireComponent(typeof(Rigidbody2D))]
public class Enemy : MonoBehaviour, IDestructible
{
    // Static Event for Damage Reporting
    public static event Action<IDestructible, float> OnAnyEnemyDamaged;

    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float currentHealth;
    [SerializeField] private GameObject deathPrefab;

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
    [Range(0f, 1f)]
    [SerializeField] private float facingThreshold = 0.8f;

    [Header("Junk Spawning Settings")]
    [SerializeField] private GameObject junkPrefab;
    [SerializeField] private GameObject debrisPrefab;
    [SerializeField] private float minSpawnInterval = 3f;
    [SerializeField] private float maxSpawnInterval = 5f;

    private Rigidbody2D rb;
    private Vector2 currentTargetPoint;
    private float nextSpawnTime;
    private Transform playerTransform;

    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        currentHealth = maxHealth;

        rb.gravityScale = 0f;
        rb.linearDamping = 1f;
        rb.angularDamping = 1f;
    }

    private void Start()
    {
        Player player = Player.Instance;
        if (player != null)
        {
            playerTransform = player.transform;
        }

        // Auto-bind any child parts configured on this enemy prefab
        InitializeChildParts();

        PickNewTarget();
        ScheduleNextJunkSpawn();
    }

    /// 
    /// Finds all Part components attached as children in the prefab editor and grounds their joints to the Enemy.
    /// 
    private void InitializeChildParts()
    {
        Part[] childParts = GetComponentsInChildren<Part>();
        Rigidbody2D enemyRb = GetComponent<Rigidbody2D>();

        foreach (Part part in childParts)
        {
            // Skip if it's the main enemy body itself
            if (part.gameObject == gameObject) continue;

            // Ensure proper layer assignment so parts don't collide with other enemy parts
            part.gameObject.layer = gameObject.layer;

            // Link HingeJoint2D to the enemy Rigidbody
            HingeJoint2D hinge = part.GetComponent<HingeJoint2D>();
            if (hinge != null)
            {
                hinge.connectedBody = enemyRb;
                hinge.autoConfigureConnectedAnchor = false;

                // Set anchors based on current transform positions set in the editor
                hinge.anchor = Vector2.zero;
                hinge.connectedAnchor = enemyRb.transform.InverseTransformPoint(part.transform.position);
                hinge.enabled = true;
            }

            // Mark all slots on this child part as occupied so loose parts don't pull into them
            JointSlot2D[] slots = part.GetComponentsInChildren<JointSlot2D>();
            foreach (var slot in slots)
            {
                slot.IsOccupied = true;
                slot.SetManager(null);
            }
        }
    }

    private void Update()
    {
        if (Time.time >= nextSpawnTime)
        {
            SpawnJunk();
            ScheduleNextJunkSpawn();
        }

        if (playerTransform != null)
        {
            float distToPlayer = Vector2.Distance(transform.position, playerTransform.position);
            if (distToPlayer > 1000f)
            {
                Destroy(gameObject);
            }
        }
    }

    private void FixedUpdate()
    {
        float distanceToTarget = Vector2.Distance(rb.position, currentTargetPoint);

        if (distanceToTarget <= arrivalDistance)
        {
            PickNewTarget();
            return;
        }

        Vector2 directionToTarget = (currentTargetPoint - rb.position).normalized;
        float targetAngle = Mathf.Atan2(directionToTarget.y, directionToTarget.x) * Mathf.Rad2Deg - 90f;
        float angleDifference = Mathf.DeltaAngle(rb.rotation, targetAngle);

        rb.AddTorque(angleDifference * rotationSpeed * Time.fixedDeltaTime);

        float alignment = Vector3.Dot(transform.up, directionToTarget);

        if (alignment >= facingThreshold)
        {
            rb.AddForce(transform.up * engineForce, ForceMode2D.Force);
        }
    }

    public void TakeDamage(float damageAmount)
    {
        if (damageAmount <= 0f) return;

        currentHealth -= damageAmount;

        // Trigger the damage event
        OnAnyEnemyDamaged?.Invoke(this, damageAmount);

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

        if (junkPrefab != null)
        {
            int junkCount = UnityEngine.Random.Range(3, 7);
            for (int i = 0; i < junkCount; i++)
            {
                GameObject junkObj = Instantiate(debrisPrefab, transform.position, Quaternion.identity);
                Rigidbody2D junkRb = junkObj.GetComponent<Rigidbody2D>();
                if (junkRb != null)
                {
                    Vector2 impulseDir = UnityEngine.Random.insideUnitCircle.normalized;
                    junkRb.AddForce(impulseDir * 0.4f, ForceMode2D.Impulse);
                }
            }
        }

        Destroy(gameObject);
    }

    private void PickNewTarget()
    {
        Vector2 moveDirection;

        if (playerTransform != null)
        {
            Vector2 dirToPlayer = ((Vector2)playerTransform.position - rb.position).normalized;
            moveDirection = Vector2.Lerp(dirToPlayer, UnityEngine.Random.insideUnitCircle.normalized, 0.4f).normalized;
        }
        else
        {
            moveDirection = UnityEngine.Random.insideUnitCircle.normalized;
        }

        float randomDistance = UnityEngine.Random.Range(minTargetDistance, maxTargetDistance);
        currentTargetPoint = rb.position + (moveDirection * randomDistance);
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
        nextSpawnTime = Time.time + UnityEngine.Random.Range(minSpawnInterval, maxSpawnInterval);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(currentTargetPoint, arrivalDistance);
        Gizmos.DrawLine(transform.position, currentTargetPoint);
    }
}