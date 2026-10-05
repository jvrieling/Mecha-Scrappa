using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

public class Part : MonoBehaviour, IDestructible
{
    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 50f;
    [SerializeField] private float currentHealth;

    [Header("Joint Connections")]
    public List joints = new List();

    private JointSlot2D connectedPartSlot;
    private JointSlot2D connectedBodySlot;
    private bool isDetached = false;

    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;

    private void OnValidate()
    {
        if (maxHealth < 0f) maxHealth = 0f;
    }

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    public void SetAttachmentConnection(JointSlot2D partSlot, JointSlot2D bodySlot)
    {
        connectedPartSlot = partSlot;
        connectedBodySlot = bodySlot;
        isDetached = false;
    }

    [ContextMenu("Detach")]
    public void Detach()
    {
        if (isDetached) return;
        isDetached = true;

        // Disable HingeJoint2D connecting this part
        HingeJoint2D hinge = GetComponent<HingeJoint2D>();
        if (hinge != null)
        {
            hinge.connectedBody = null;
            hinge.enabled = false;
        }

        transform.SetParent(null);

        // Clear slot occupation references
        if (connectedBodySlot != null)
        {
            connectedBodySlot.IsOccupied = false;
        }

        JointSlot2D[] mySlots = GetComponentsInChildren<JointSlot2D>();
        foreach (var slot in mySlots)
        {
            slot.IsOccupied = false;
            slot.SetManager(null);
        }

        connectedPartSlot = null;
        connectedBodySlot = null;

        // Apply a slight impulse force so the detached part pops off
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            Vector2 randomPop = UnityEngine.Random.insideUnitCircle.normalized * 2f;
            rb.AddForce(randomPop, ForceMode2D.Impulse);
            rb.AddTorque(UnityEngine.Random.Range(-5f, 5f), ForceMode2D.Impulse);
        }
    }

    public void TakeDamage(float damageAmount)
    {
        if (damageAmount <= 0f || isDetached) return;

        currentHealth -= damageAmount;
        Debug.Log($"Part '{gameObject.name}' took {damageAmount} damage. Health remaining: {currentHealth}", gameObject);

        CameraFollow.Shake();

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log($"Part '{gameObject.name}' destroyed! Detaching...", gameObject);
        CameraFollow.Shake(0.25f, 0.6f);
        Detach(); // Detaches rather than calling Destroy
    }
}