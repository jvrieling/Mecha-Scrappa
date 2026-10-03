using System;
using UnityEngine;

public class Player : MonoBehaviour, IDestructible
{
    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 200f;
    [SerializeField] private float currentHealth;

    [Header("Movement Settings")]
    [Tooltip("Strength of the impulse force applied on key press.")]
    [SerializeField] private float pushForce = 10f;

    public Rigidbody2D rb; 
    public ParticleSystem leftBoost, rightBoost, upBoost, downBoost; 

    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;

    private void OnValidate()
    {
        if (maxHealth < 0f)
        {
            maxHealth = 0f;
        }

        if (rb == null)
        {
            rb = GetComponent<Rigidbody2D>();
        }
    }

    private void Awake()
    {
        if (rb == null)
        {
            throw new NullReferenceException($"rb on {gameObject.name} is missing or null!");
        }

        currentHealth = maxHealth;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow)) 
        {
            ApplyPush(Vector2.up); 
            if (upBoost != null) upBoost.Play(); 
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow)) 
        {
            ApplyPush(Vector2.down); 
            if (downBoost != null) downBoost.Play(); 
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow)) 
        {
            ApplyPush(Vector2.left); 
            if (leftBoost != null) leftBoost.Play(); 
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow)) 
        {
            ApplyPush(Vector2.right); 
            if (rightBoost != null) rightBoost.Play(); 
        }
    }

    private void ApplyPush(Vector2 direction)
    {
        rb.AddForce(direction * pushForce, ForceMode2D.Impulse); 
    }

    public void TakeDamage(float damageAmount)
    {
        if (damageAmount <= 0f) return;

        currentHealth -= damageAmount;
        Debug.Log($"Player took {damageAmount} damage. Remaining health: {currentHealth}", gameObject);

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log("Player was destroyed!", gameObject);
        Destroy(gameObject);
    }
}