using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("Strength of the impulse force applied on key press.")]
    [SerializeField] private float pushForce = 10f;

    public Rigidbody2D rb;

    public ParticleSystem leftBoost, rightBoost, upBoost, downBoost;

    private void Update()
    {
        // Check for individual arrow key presses on the frame they are pressed down
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            ApplyPush(Vector2.up);
            upBoost.Play();
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            ApplyPush(Vector2.down);
            downBoost.Play();
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            ApplyPush(Vector2.left);
            leftBoost.Play();
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            ApplyPush(Vector2.right);   
            rightBoost.Play();
        }
    }

    private void ApplyPush(Vector2 direction)
    {
        // Apply immediate force impulse taking mass into account
        rb.AddForce(direction * pushForce, ForceMode2D.Impulse);
    }
}