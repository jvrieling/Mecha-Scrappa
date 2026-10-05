using UnityEngine;

public class GunAttachment : MonoBehaviour
{
    [Header("Gun Settings")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float fireRate = 1.5f; // Shots per second
    [SerializeField] private float bulletSpeed = 15f;
    [SerializeField] private float bulletDamage = 10f;

    [Header("Aiming Settings")]
    [SerializeField] private float rotationSpeed = 5f;
    [SerializeField] private float maxTargetDistance = 30f;

    private float nextFireTime;
    private Transform targetTransform;
    private bool isEnemyWeapon = false;

    private void Update()
    {
        // 1. DO NOT fire or aim if the part is floating loose in space
        if (!IsAttachedToEntity())
        {
            return;
        }

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

    private bool IsAttachedToEntity()
    {
        // Checks if this weapon is attached to either a Player or an Enemy
        return GetComponentInParent<Player>() != null || GetComponentInParent<Enemy>() != null;
    }

    private void DetectOwner()
    {
        Enemy enemy = GetComponentInParent<Enemy>();
        isEnemyWeapon = (enemy != null);
    }

    private void FindTarget()
    {
        if (isEnemyWeapon)
        {
            // Enemy weapons target the player
            if (Player.Instance != null)
            {
                targetTransform = Player.Instance.transform;
            }
        }
        else
        {
            // Player weapons target the nearest enemy
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
            // -transform.right shoots along the left-facing direction of the sprite
            Vector2 fireDirection = firePoint != null ? -firePoint.right : -transform.right;
            projectile.Initialize(fireDirection * bulletSpeed, bulletDamage, gameObject);
        }
    }
}