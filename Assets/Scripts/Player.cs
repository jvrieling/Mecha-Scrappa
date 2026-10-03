using UnityEngine;

public class Player : MonoBehaviour
{
    public static Player Instance;

    [Header("Movement Settings")]
    [Tooltip("Strength of the impulse force applied on key press.")]
    [SerializeField] private float pushForce = 10f;
    [Tooltip("Torque impulse force applied when pressing Left/Right arrow keys.")]
    [SerializeField] private float torqueForce = 5f;

    [Header("Boost Particles Settings")]
    [Tooltip("Parent object containing leftBoost, rightBoost, upBoost, downBoost particle systems.")]
    [SerializeField] private Transform boostParticlesParent;

    public Rigidbody2D rb;

    public ParticleSystem leftBoost, rightBoost, upBoost, downBoost;

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        // Rotate Left (counter-clockwise)
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            rb.AddTorque(torqueForce, ForceMode2D.Impulse);
            OrientBoostParticles(Vector2.left);
            leftBoost.Play();
        }
        // Rotate Right (clockwise)
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            rb.AddTorque(-torqueForce, ForceMode2D.Impulse);
            OrientBoostParticles(Vector2.right);
            rightBoost.Play();
        }
        // Push Forward (relative to player transform)
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            ApplyPush(transform.up);
            OrientBoostParticles(Vector2.up);
            upBoost.Play();
        }
        // Push Backward (relative to player transform)
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            ApplyPush(-transform.up);
            OrientBoostParticles(Vector2.down);
            downBoost.Play();
        }
    }

    private void ApplyPush(Vector2 direction)
    {
        // Apply immediate force impulse taking mass into account
        rb.AddForce(direction * pushForce, ForceMode2D.Impulse);
    }

    /// <summary>
    /// Rotates the parent object of the boost particles to face the intended direction.
    /// </summary>
    private void OrientBoostParticles(Vector2 direction)
    {
        if (boostParticlesParent == null) return;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
        boostParticlesParent.localRotation = Quaternion.Euler(0f, 0f, angle);
    }
}