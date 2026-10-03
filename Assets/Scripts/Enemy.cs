using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Enemy : MonoBehaviour, IDestructible
{
    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float currentHealth;

    [Header("Movement Settings")]
    [SerializeField] private Rigidbody2D enemyRigidbody;
    [Tooltip("Strength of the impulse force applied during a boost.")]
    [SerializeField] private float pushForce = 10f;

    [Header("Boost Timing")]
    [Tooltip("Minimum time in seconds between random boosts.")]
    [SerializeField] private float minBoostInterval = 1.0f;
    [Tooltip("Maximum time in seconds between random boosts.")]
    [SerializeField] private float maxBoostInterval = 3.5f;

    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;

    private void OnValidate()
    {
        if (maxHealth < 0f)
        {
            maxHealth = 0f;
        }

        if (enemyRigidbody == null)
        {
            enemyRigidbody = GetComponent<Rigidbody2D>();
        }
    }

    private void Awake()
    {
        if (enemyRigidbody == null)
        {
            throw new NullReferenceException($"enemyRigidbody on {gameObject.name} is missing or null!");
        }

        currentHealth = maxHealth;
    }

    private void Start()
    {
        StartCoroutine(RandomBoostRoutine());
    }

    private IEnumerator RandomBoostRoutine()
    {
        while (true)
        {
            float waitTime = UnityEngine.Random.Range(minBoostInterval, maxBoostInterval);
            yield return new WaitForSeconds(waitTime);

            Vector2 randomCardinalDirection = GetRandomCardinalDirection();
            ApplyPush(randomCardinalDirection);
        }
    }

    private Vector2 GetRandomCardinalDirection()
    {
        int randomIndex = UnityEngine.Random.Range(0, 4);
        switch (randomIndex)
        {
            case 0: return Vector2.up;
            case 1: return Vector2.down;
            case 2: return Vector2.left;
            case 3: return Vector2.right;
            default: return Vector2.up;
        }
    }

    private void ApplyPush(Vector2 direction)
    {
        enemyRigidbody.AddForce(direction * pushForce, ForceMode2D.Impulse);
    }

    public void TakeDamage(float damageAmount)
    {
        if (damageAmount <= 0f) return;

        currentHealth -= damageAmount;
        Debug.Log($"Enemy '{gameObject.name}' took {damageAmount} damage. Health remaining: {currentHealth}", gameObject);

        CameraFollow.Shake();

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log($"Enemy '{gameObject.name}' destroyed!", gameObject);
        CameraFollow.Shake(0.25f, 0.6f);
        Destroy(gameObject);
    }
}