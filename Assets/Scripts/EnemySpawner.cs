using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Enemy Prefabs")]
    [SerializeField] private GameObject[] enemyPrefabs;

    [Header("Spawn Timing")]
    [SerializeField] private float minSpawnInterval = 20f;
    [SerializeField] private float maxSpawnInterval = 35f;

    [Header("Spawn Distance Range")]
    [SerializeField] private float minSpawnDistance = 20f;
    [SerializeField] private float maxSpawnDistance = 40f;

    private Transform playerTransform;
    private float nextSpawnTime;

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
        if (Time.time >= nextSpawnTime)
        {
            SpawnEnemy();
            ScheduleNextSpawn();
        }
    }

    private void SpawnEnemy()
    {
        if (enemyPrefabs == null || enemyPrefabs.Length == 0) return;

        // Locate player if transform reference was lost
        if (playerTransform == null)
        {
            Player player = Player.Instance;
            if (player != null) playerTransform = player.transform;
            else return;
        }

        // Choose random prefab from list
        GameObject chosenPrefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];

        // Select random point between 20m and 40m away from player
        Vector2 spawnDir = Random.insideUnitCircle.normalized;
        float spawnDist = Random.Range(minSpawnDistance, maxSpawnDistance);
        Vector3 spawnPosition = playerTransform.position + (Vector3)(spawnDir * spawnDist);

        Instantiate(chosenPrefab, spawnPosition, Quaternion.identity);
    }

    private void ScheduleNextSpawn()
    {
        nextSpawnTime = Time.time + Random.Range(minSpawnInterval, maxSpawnInterval);
    }
}