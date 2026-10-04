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
        HingeJoint2D hinge = GetComponent<HingeJoint2D>();
        if (hinge != null)
        {
            hinge.connectedBody = null;
            hinge.enabled = false;
        }

        transform.SetParent(null);

        // Keep the original logic: the body slot remains occupied 
        if (connectedBodySlot != null)
        {
            connectedBodySlot.IsOccupied = true;
        }

        // NEW: Prevent THIS detached part from instantly being pulled again or pulling others
        JointSlot2D[] mySlots = GetComponentsInChildren<JointSlot2D>();
        foreach (var slot in mySlots)
        {
            slot.IsOccupied = true; // Makes it an invalid target for magnets
            slot.SetManager(null);  // Stops it from running its own magnet logic
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

        CameraFollow.Shake();

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log($"Part '{gameObject.name}' destroyed!", gameObject);
        CameraFollow.Shake(0.25f, 0.6f);
        Destroy(gameObject);
    }
}