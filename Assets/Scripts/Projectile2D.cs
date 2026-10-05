using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class Projectile2D : MonoBehaviour
{
    [SerializeField] private float lifetime = 4f;
    [SerializeField] private GameObject hitEffectPrefab;

    private Rigidbody2D rb;
    private float damage;
    private GameObject owner;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        Destroy(gameObject, lifetime);
    }

    public void Initialize(Vector2 velocity, float damageAmount, GameObject shooter)
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        rb.linearVelocity = velocity;
        damage = damageAmount;
        owner = shooter;

        if (owner != null)
        {
            Collider2D bulletCol = GetComponent<Collider2D>();
            Collider2D[] ownerCols = owner.GetComponentsInChildren<Collider2D>();
            foreach (var col in ownerCols)
            {
                if (col != null) Physics2D.IgnoreCollision(bulletCol, col, true);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        HandleImpact(collision.gameObject, collision.transform.position);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        HandleImpact(collision.gameObject, collision.GetContact(0).point);
    }

    private void HandleImpact(GameObject target, Vector3 impactPoint)
    {
        if (owner != null && (target == owner || target.transform.IsChildOf(owner.transform)))
        {
            return;
        }

        IDestructible destructible = target.GetComponent<IDestructible>() ?? target.GetComponentInParent<IDestructible>();
        if (destructible != null)
        {
            destructible.TakeDamage(damage);
        }

        if (hitEffectPrefab != null)
        {
            Instantiate(hitEffectPrefab, impactPoint, Quaternion.identity);
        }

        Destroy(gameObject);
    }
}