using System.Collections.Generic;
using UnityEngine;

public class PartSpawner : MonoBehaviour
{
    [Header("Part Prefabs")]
    [Tooltip("List of part prefabs to randomly spawn.")]
    [SerializeField] private List<GameObject> partPrefabs = new List<GameObject>();

    [Header("Spawn Distance Settings")]
    [SerializeField] private float minSpawnDistance = 40f;
    [SerializeField] private float maxSpawnDistance = 60f;

    [Header("Spawn Interval Settings")]
    [SerializeField] private float minSpawnInterval = 20f;
    [SerializeField] private float maxSpawnInterval = 40f;

    [Header("Physics Impulses")]
    [SerializeField] private float minImpulse = 0.05f;
    [SerializeField] private float maxImpulse = 0.15f;
    [SerializeField] private float minSpin = -10f;
    [SerializeField] private float maxSpin = 10f;

    [Header("Capacity Settings")]
    [SerializeField] private int maxUnattachedParts = 10;

    private List<GameObject> spawnedParts = new List<GameObject>();
    private float nextSpawnTime;
    private Transform playerTransform;

    private void Start()
    {
        Player player = Player.Instance;  
        if (player != null)
        {
            playerTransform = player.transform;
        }

        ScheduleNextSpawn();
    }

    private void Update()
    {
        // Lazy-search for player if not found in Start
        if (playerTransform == null && Player.Instance != null)
        {
            playerTransform = Player.Instance.transform;  
        }

        if (Time.time >= nextSpawnTime)
        {
            SpawnPart();
            ScheduleNextSpawn();
        }
    }

    private void SpawnPart()
    {
        if (partPrefabs == null || partPrefabs.Count == 0)
        {
            Debug.LogWarning("PartSpawner: No part prefabs assigned!", this);
            return;
        }

        Vector3 playerPos = playerTransform != null ? playerTransform.position : transform.position;

        // Cleanup and calculate unattached spawned parts
        List<GameObject> unattachedParts = GetUnattachedParts();

        // Destroy furthest unattached part if capacity is met/exceeded
        if (unattachedParts.Count >= maxUnattachedParts)
        {
            GameObject furthestPart = GetFurthestPart(unattachedParts, playerPos);
            if (furthestPart != null)
            {
                spawnedParts.Remove(furthestPart);
                Destroy(furthestPart);
            }
        }

        // Pick a random prefab
        GameObject randomPrefab = partPrefabs[Random.Range(0, partPrefabs.Count)];

        // Calculate random spawn position between min and max distance away from player
        Vector2 randomDir = Random.insideUnitCircle.normalized;
        float randomDistance = Random.Range(minSpawnDistance, maxSpawnDistance);
        Vector3 spawnPosition = playerPos + (Vector3)(randomDir * randomDistance);

        // Instantiate selected part
        GameObject newPart = Instantiate(randomPrefab, spawnPosition, Quaternion.Euler(0f, 0f, Random.Range(0f, 360f)));
        spawnedParts.Add(newPart);

        // Apply random impulse and spin
        Rigidbody2D rb = newPart.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            Vector2 impulseDirection = Random.insideUnitCircle.normalized;
            float impulseMagnitude = Random.Range(minImpulse, maxImpulse);
            rb.AddForce(impulseDirection * impulseMagnitude, ForceMode2D.Impulse);

            float randomTorque = Random.Range(minSpin, maxSpin);
            rb.AddTorque(randomTorque, ForceMode2D.Impulse);
        }
    }

    private List<GameObject> GetUnattachedParts()
    {
        List<GameObject> unattached = new List<GameObject>();

        for (int i = spawnedParts.Count - 1; i >= 0; i--)
        {
            GameObject partObj = spawnedParts[i];

            if (partObj == null)
            {
                spawnedParts.RemoveAt(i);
                continue;
            }

            // Check if attached to player via joint connection or hierarchy parenting
            if (!IsAttachedToPlayer(partObj))
            {
                unattached.Add(partObj);
            }
        }

        return unattached;
    }

    private bool IsAttachedToPlayer(GameObject partObj)
    {
        if (playerTransform == null) return false;

        // 1. Check if the object is parented under the Player hierarchy
        if (partObj.transform.IsChildOf(playerTransform)) return true;

        // 2. Check if any HingeJoint2D is connected to the Player's Rigidbody2D
        HingeJoint2D hinge = partObj.GetComponent<HingeJoint2D>();
        if (hinge != null && hinge.enabled && hinge.connectedBody != null)
        {
            if (hinge.connectedBody.transform.IsChildOf(playerTransform))
            {
                return true;
            }
        }

        // 3. Check JointSlot2D occupancy state
        JointSlot2D[] slots = partObj.GetComponentsInChildren<JointSlot2D>();
        foreach (var slot in slots)
        {
            if (slot.IsOccupied) return true; 
        }

        return false;
    }

    private GameObject GetFurthestPart(List<GameObject> parts, Vector3 referencePoint)
    {
        GameObject furthest = null;
        float maxDistanceSq = -1f;

        foreach (GameObject part in parts)
        {
            float distSq = (part.transform.position - referencePoint).sqrMagnitude;
            if (distSq > maxDistanceSq)
            {
                maxDistanceSq = distSq;
                furthest = part;
            }
        }

        return furthest;
    }

    private void ScheduleNextSpawn()
    {
        nextSpawnTime = Time.time + Random.Range(minSpawnInterval, maxSpawnInterval);
    }

    private void OnDrawGizmosSelected()
    {
        // Try finding the player reference in Edit Mode if null
        Vector3 centerPos = transform.position;
        if (playerTransform != null)
        {
            centerPos = playerTransform.position;
        }
        else if (Player.Instance != null)  
        {
            centerPos = Player.Instance.transform.position;  
        }
        else
        {
            // Search scene for Player component directly in Editor
            Player foundPlayer = FindFirstObjectByType<Player>();
            if (foundPlayer != null)
            {
                centerPos = foundPlayer.transform.position;
            }
        }

        // Draw Minimum Spawn Radius Wire Sphere
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(centerPos, minSpawnDistance);

        // Draw Maximum Spawn Radius Wire Sphere
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(centerPos, maxSpawnDistance);
    }
}