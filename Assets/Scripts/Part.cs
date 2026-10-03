using System;
using System.Collections.Generic;
using UnityEngine;

public class Part : MonoBehaviour, IDestructible
{
    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 50f;
    [SerializeField] private float currentHealth;

    [Header("Joint Connections")]
    public List<JointSlot2D> joints = new List<JointSlot2D>();

    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;

    private void OnValidate()
    {
        if (maxHealth < 0f)
        {
            maxHealth = 0f;
        }
    }

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float damageAmount)
    {
        if (damageAmount <= 0f) return;

        currentHealth -= damageAmount;
        Debug.Log($"Part '{gameObject.name}' took {damageAmount} damage. Health remaining: {currentHealth}", gameObject);

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log($"Part '{gameObject.name}' destroyed!", gameObject);
        Destroy(gameObject);
    }
}