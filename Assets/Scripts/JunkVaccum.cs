using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class JunkVacuum : MonoBehaviour
{
    [Header("Vacuum Settings")]
    [Tooltip("Radius within which junk will be sucked in toward the player.")]
    [SerializeField] private float vacuumRadius = 5f;

    [Tooltip("Distance threshold at which junk is collected and destroyed.")]
    [SerializeField] private float collectRadius = 0.5f;

    [Tooltip("Speed/force applied to pull junk toward the vacuum center.")]
    [SerializeField] private float pullSpeed = 10f;

    [Tooltip("Layer mask assigned to floating junk objects.")]
    [SerializeField] private LayerMask junkLayer;

    [Header("Collection Effects")]
    [Tooltip("Particle system prefab to instantiate when junk is collected.")]
    [SerializeField] private GameObject collectParticlePrefab;

    [Tooltip("Audio clip to play when junk is collected.")]
    [SerializeField] private AudioClip collectSound;

    private AudioSource audioSource;

    /// <summary>
    /// Total amount of junk collected by the player.
    /// Public read access with a private setter.
    /// </summary>
    public int JunkCount { get; private set; }

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private void FixedUpdate()
    {
        VacuumJunk();
    }

    private void VacuumJunk()
    {
        // Find all colliders on the Junk layer within vacuum range
        Collider2D[] junkColliders = Physics2D.OverlapCircleAll(transform.position, vacuumRadius, junkLayer);

        foreach (Collider2D hit in junkColliders)
        {
            Vector2 direction = (Vector2)transform.position - (Vector2)hit.transform.position;
            float distance = direction.magnitude;

            // Collect the junk if it reaches the inner radius
            if (distance <= collectRadius)
            {
                CollectJunk(hit.gameObject);
            }
            else
            {
                // Pull the junk toward the player using its Rigidbody2D
                Rigidbody2D junkRb = hit.attachedRigidbody;
                if (junkRb != null)
                {
                    Vector2 pullForce = direction.normalized * pullSpeed;
                    junkRb.AddForce(pullForce, ForceMode2D.Force);
                }
            }
        }
    }

    private void CollectJunk(GameObject junkObject)
    {
        // Increment tracked score
        JunkCount++;

        GameManager.Instance.AddScrap(1);

        // Instantiate collection particle effect
        if (collectParticlePrefab != null)
        {
            Instantiate(collectParticlePrefab, junkObject.transform.position, Quaternion.identity);
        }

        // Play audio clip using attached AudioSource component
        if (collectSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(collectSound);
        }

        // Destroy collected junk object
        Destroy(junkObject);
    }

    private void OnDrawGizmosSelected()
    {
        // Visualize pull and collection ranges in the Scene view
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, vacuumRadius);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, collectRadius);
    }
}