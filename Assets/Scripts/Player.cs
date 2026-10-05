using UnityEngine;

public class Player : MonoBehaviour, IDestructible
{
    public static Player Instance;

    [Header("Movement Settings")]
    [SerializeField] private float pushForce = 10f;
    [SerializeField] private float torqueForce = 5f;

    [Header("Boost Particles Settings")]
    [SerializeField] private Transform boostParticlesParent;

    [Header("Health & Death Settings")]
    public float maxHP = 100f;
    public float currentHP { get; private set; }

    // IDestructible Interface Properties
    public float CurrentHealth => currentHP;
    public float MaxHealth => maxHP;

    public SpriteRenderer playerSprite;
    public ParticleSystem explosionParticles;

    [HideInInspector] public Rigidbody2D rb;
    public ParticleSystem leftBoost, rightBoost, upBoost, downBoost;

    private bool isDead = false;

    private void Awake()
    {
        Instance = this;
        rb = GetComponent<Rigidbody2D>();
        currentHP = maxHP;
    }

    private void Start()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.UpdateHPUI(currentHP, maxHP);
        }
    }

    private void Update()
    {
        if (isDead) return;

        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            rb.AddTorque(torqueForce, ForceMode2D.Impulse);
            OrientBoostParticles(Vector2.left);
            leftBoost.Play();
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            rb.AddTorque(-torqueForce, ForceMode2D.Impulse);
            OrientBoostParticles(Vector2.right);
            rightBoost.Play();
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            ApplyPush(transform.up);
            OrientBoostParticles(Vector2.up);
            upBoost.Play();
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            ApplyPush(-transform.up);
            OrientBoostParticles(Vector2.down);
            downBoost.Play();
        }
    }

    private void ApplyPush(Vector2 direction)
    {
        rb.AddForce(direction * pushForce, ForceMode2D.Impulse);
    }

    private void OrientBoostParticles(Vector2 direction)
    {
        if (boostParticlesParent == null) return;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
        boostParticlesParent.localRotation = Quaternion.Euler(0f, 0f, angle);
    }

    public void TakeDamage(float damageAmount)
    {
        if (isDead) return;

        currentHP -= damageAmount;
        currentHP = Mathf.Max(currentHP, 0f);

        Debug.Log("Player took " + damageAmount + " damage!");

        if (GameManager.Instance != null)
        {
            GameManager.Instance.UpdateHPUI(currentHP, maxHP);
        }

        if (currentHP <= 0f)
        {
            Die();
        }
    }

    private void Die()
    {
        isDead = true;
        if (GameManager.Instance != null)
        {
            GameManager.Instance.TriggerDeathScene(this);
        }
    }
}