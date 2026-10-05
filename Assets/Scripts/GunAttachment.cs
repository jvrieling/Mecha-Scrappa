using UnityEngine;

public class GunAttachment : MonoBehaviour
{
    [Header("Gun Settings")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float fireRate = 1.5f;
    [SerializeField] private float bulletSpeed = 15f;
    [SerializeField] private float bulletDamage = 10f;

    [Header("Aiming Settings")]
    [SerializeField] private float rotationSpeed = 5f;
    [SerializeField] private float maxTargetDistance = 30f;

    private float nextFireTime;
    private Transform targetTransform;
    private bool isEnemyWeapon = false;
    private GameObject rootOwner;

    private void Update()
    {
        rootOwner = GetRootOwner();

        // 1. Do NOT fire or aim if floating loose in space
        if (rootOwner == null)
        {
            return;
        }

        IgnoreOwnerCollisions();
        DetectOwner();
        FindTarget();
        AimAtTarget();

        if (Time.time >= nextFireTime && targetTransform != null)
        {
            float distance = Vector2.Distance(transform.position, targetTransform.position);
            if (distance <= maxTargetDistance)
            {
                Shoot();
                nextFireTime = Time.time + (1f / fireRate);
            }
        }
    }

    /// 
    /// Traces both Transform hierarchy AND HingeJoint2D physics chains to find the true root entity.
    /// 
    private GameObject GetRootOwner()
    {
        // Check Transform hierarchy first
        Player p = GetComponentInParent<Player>();
        if (p != null) return p.gameObject;

        Enemy e = GetComponentInParent<Enemy>();
        if (e != null) return e.gameObject;

        // Check HingeJoint2D physics connections
        HingeJoint2D hinge = GetComponent<HingeJoint2D>();
        if (hinge != null && hinge.enabled && hinge.connectedBody != null)
        {
            Player connectedPlayer = hinge.connectedBody.GetComponentInParent<Player>();
            if (connectedPlayer != null) return connectedPlayer.gameObject;

            Enemy connectedEnemy = hinge.connectedBody.GetComponentInParent<Enemy>();
            if (connectedEnemy != null) return connectedEnemy.gameObject;
        }

        return null;
    }

    private void IgnoreOwnerCollisions()
    {
        if (rootOwner == null) return;

        Collider2D[] myColliders = GetComponentsInChildren<Collider2D>();
        Collider2D[] ownerColliders = rootOwner.GetComponentsInChildren <Collider2D> ();

        foreach (var myCol in myColliders)
        {
            foreach (var ownerCol in ownerColliders)
            {
                if (myCol != null && ownerCol != null)
                {
                    Physics2D.IgnoreCollision(myCol, ownerCol, true);
                }
            }
        }
    }

    private void DetectOwner()
    {
        if (rootOwner != null)
        {
            isEnemyWeapon = rootOwner.GetComponent<Enemy>() != null || rootOwner.GetComponentInParent<Player>() != null;
        }
    }

    private void FindTarget()
    {
        if (isEnemyWeapon)
        {
            if (Player.Instance != null)
            {
                targetTransform = Player.Instance.transform;
            }
        }
        else
        {
            Enemy[] enemies = FindObjectsByType<Enemy>();
            float closestDist = maxTargetDistance;
            Transform closestEnemy = null;

            foreach (Enemy e in enemies)
            {
                float dist = Vector2.Distance(transform.position, e.transform.position);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closestEnemy = e.transform;
                }
            }

            targetTransform = closestEnemy;
        }
    }

    private void AimAtTarget()
    {
        if (targetTransform == null) return;

        Vector2 direction = (targetTransform.position - transform.position).normalized;

        // +180 degrees compensates for left-facing artwork
        float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg + 180f;
        Quaternion targetRotation = Quaternion.Euler(0f, 0f, targetAngle);

        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }

    private void Shoot()
    {
        if (bulletPrefab == null) return;

        Vector3 spawnPos = firePoint != null ? firePoint.position : transform.position;
        Quaternion spawnRot = firePoint != null ? firePoint.rotation : transform.rotation;

        GameObject bullet = Instantiate(bulletPrefab, spawnPos, spawnRot);

        Projectile2D projectile = bullet.GetComponent<Projectile2D>();
        if (projectile != null)
        {
            Vector2 fireDirection = firePoint != null ? -firePoint.right : -transform.right;
            // Pass rootOwner as the shooter so bullets ignore the entire ship
            projectile.Initialize(fireDirection * bulletSpeed, bulletDamage, rootOwner);
        }
    }
}