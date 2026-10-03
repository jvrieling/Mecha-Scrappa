using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class Junk : MonoBehaviour
{
    [Header("Sprite Variation")]
    [Tooltip("List of possible sprites to randomly choose from.")]
    [SerializeField] private List<Sprite> junkSprites;

    [Header("Linear Drift Settings")]
    [Tooltip("Minimum force applied at start.")]
    [SerializeField] private float minImpulseForce = 0.05f;
    [Tooltip("Maximum force applied at start.")]
    [SerializeField] private float maxImpulseForce = 0.2f;

    [Header("Angular Spin Settings")]
    [Tooltip("Minimum rotational velocity applied at start (degrees/sec).")]
    [SerializeField] private float minAngularVelocity = -15f;
    [Tooltip("Maximum rotational velocity applied at start (degrees/sec).")]
    [SerializeField] private float maxAngularVelocity = 15f;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        RandomizeSprite();
        ApplyFloatingMovement();
    }

    private void RandomizeSprite()
    {
        if (junkSprites != null && junkSprites.Count > 0)
        {
            int randomIndex = Random.Range(0, junkSprites.Count);
            spriteRenderer.sprite = junkSprites[randomIndex];
        }
    }

    private void ApplyFloatingMovement()
    {
        // Pick a random unit vector direction
        Vector2 randomDirection = Random.insideUnitCircle.normalized;

        // Determine random force and rotational velocity within specified range
        float randomForce = Random.Range(minImpulseForce, maxImpulseForce);
        float randomSpin = Random.Range(minAngularVelocity, maxAngularVelocity);

        // Apply impulse force for initial drift
        rb.AddForce(randomDirection * randomForce, ForceMode2D.Impulse);

        // Apply torque/spin velocity
        rb.angularVelocity = randomSpin;
    }
}