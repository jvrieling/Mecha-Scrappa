using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class ImpactDamager : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private Rigidbody2D partRigidbody;

    [Header("Damage Settings")]
    [Tooltip("Minimum impact velocity needed to deal damage.")]
    [SerializeField] private float minimumImpactVelocity = 2.0f;

    [Tooltip("Damage multiplier applied to the relative impact speed.")]
    [SerializeField] private float damageMultiplier = 5.0f;

    [Header("VFX")]
    [SerializeField]
    private ParticleSystem sparkParticlePrefab;

    [Header("Debug")]
    public float DEBUG_LastHitVel;

    private void OnValidate()
    {
        if (partRigidbody == null)
        {
            partRigidbody = GetComponent<Rigidbody2D>();
        }
    }

    private void Awake()
    {
        if (partRigidbody == null)
        {
            throw new NullReferenceException($"partRigidbody on {gameObject.name} is missing or null!");
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        float impactSpeed = collision.relativeVelocity.magnitude;
        DEBUG_LastHitVel = impactSpeed;

        if (impactSpeed < minimumImpactVelocity)
        {
            return;
        }

        IDestructible destructible = collision.gameObject.GetComponent<IDestructible>();
        if (destructible == null)
        {
            destructible = collision.gameObject.GetComponentInParent<IDestructible>();
        }

        if (destructible != null)
        {
            float calculatedDamage = impactSpeed * damageMultiplier;
            destructible.TakeDamage(calculatedDamage);
            Instantiate(sparkParticlePrefab, collision.GetContact(0).point, Quaternion.identity);
        }
    }
}