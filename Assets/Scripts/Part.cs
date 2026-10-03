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

    // Stores references to the slots involved in attaching this part to the player
    private JointSlot2D connectedPartSlot;
    private JointSlot2D connectedBodySlot;

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

    /// <summary>
    /// Registers the joint slots used when attaching to the player.
    /// Call this from PlayerJointManager.AttachArm when a part is attached.
    /// </summary>
    public void SetAttachmentConnection(JointSlot2D partSlot, JointSlot2D bodySlot)
    {
        connectedPartSlot = partSlot;
        connectedBodySlot = bodySlot;
    }

    [ContextMenu("Detach")]
    public void Detach()
    {
        // Disable HingeJoint2D connecting this part to the player
        HingeJoint2D hinge = GetComponent<HingeJoint2D>();
        if (hinge != null)
        {
            hinge.connectedBody = null;
            hinge.enabled = false;
        }

        // Unparent if it was parented to the player
        transform.SetParent(null);

        // Keep the player body joint occupied so it cannot be reused
        if (connectedBodySlot != null)
        {
            connectedBodySlot.IsOccupied = true;
        }

        // Allow any unused joints on this part to remain usable, 
        // but free up the specific slot that was attached if needed
        if (connectedPartSlot != null)
        {
            connectedPartSlot.IsOccupied = false;
        }

        connectedPartSlot = null;
        connectedBodySlot = null;
    }

    private void OnDestroy()
    {
        // Safe check to detach and keep parent body slot occupied upon destruction
        Detach();
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