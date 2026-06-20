using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Manages enemy spawning with wave system
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    [System.Serializable]
    public class Wave
    {
        [SerializeField] public int enemyCount = 5;
        [SerializeField] public float spawnDelay = 1f;
        [SerializeField] public float waveDelay = 3f;
    }

    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private float spawnRadius = 2f;
    [SerializeField] private List<Wave> waves = new List<Wave>();

    [Header("Settings")]
    [SerializeField] private bool autoStart = true;
    [SerializeField] private int maxEnemiesActive = 20;

    private int currentWaveIndex = 0;
    private int enemiesSpawned = 0;
    private bool isSpawning = false;
    private List<GameObject> activeEnemies = new List<GameObject>();
    private Transform playerTransform;

    // Events
    public delegate void WaveEventHandler(int waveNumber);
    public event WaveEventHandler OnWaveStart;
    public event WaveEventHandler OnWaveComplete;
    public delegate void SpawnEventHandler();
    public event SpawnEventHandler OnAllWavesComplete;

    private void Start()
    {
        // Find player reference
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }

        if (autoStart && waves.Count > 0)
        {
            StartSpawning();
        }
    }

    private void Update()
    {
        // Clean up destroyed enemies from list
        activeEnemies.RemoveAll(enemy => enemy == null);
    }

    /// <summary>
    /// Start spawning waves
    /// </summary>
    public void StartSpawning()
    {
        if (isSpawning) return;
        if (waves.Count == 0)
        {
            Debug.LogWarning("No waves configured for EnemySpawner");
            return;
        }

        currentWaveIndex = 0;
        StartCoroutine(SpawnWavesCoroutine());
    }

    /// <summary>
    /// Spawn waves coroutine
    /// </summary>
    private IEnumerator SpawnWavesCoroutine()
    {
        isSpawning = true;

        for (int i = 0; i < waves.Count; i++)
        {
            currentWaveIndex = i;
            OnWaveStart?.Invoke(i + 1);

            yield return StartCoroutine(SpawnWaveCoroutine(waves[i]));

            OnWaveComplete?.Invoke(i + 1);

            // Wait before next wave
            if (i < waves.Count - 1)
            {
                yield return new WaitForSeconds(waves[i].waveDelay);
            }
        }

        isSpawning = false;
        OnAllWavesComplete?.Invoke();
    }

    /// <summary>
    /// Spawn a single wave
    /// </summary>
    private IEnumerator SpawnWaveCoroutine(Wave wave)
    {
        for (int i = 0; i < wave.enemyCount; i++)
        {
            // Wait for active enemies to drop below max if exceeded
            while (activeEnemies.Count >= maxEnemiesActive)
            {
                yield return new WaitForSeconds(0.5f);
            }

            SpawnEnemy();
            enemiesSpawned++;

            yield return new WaitForSeconds(wave.spawnDelay);
        }
    }

    /// <summary>
    /// Spawn a single enemy
    /// </summary>
    private void SpawnEnemy()
    {
        if (enemyPrefab == null)
        {
            Debug.LogError("Enemy prefab not assigned to EnemySpawner");
            return;
        }

        if (spawnPoint == null)
        {
            Debug.LogError("Spawn point not assigned to EnemySpawner");
            return;
        }

        // Random spawn position within radius
        Vector2 randomOffset = Random.insideUnitCircle * spawnRadius;
        Vector3 spawnPosition = spawnPoint.position + new Vector3(randomOffset.x, randomOffset.y, 0);

        // Instantiate enemy
        GameObject spawnedEnemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
        activeEnemies.Add(spawnedEnemy);

        // Set player reference if available
        if (playerTransform != null)
        {
            Enemy enemyController = spawnedEnemy.GetComponent<Enemy>();
            if (enemyController != null)
            {
                enemyController.SetPlayerTransform(playerTransform);
            }
        }

        // Listen to enemy death
        EnemyHealth enemyHealth = spawnedEnemy.GetComponent<EnemyHealth>();
        if (enemyHealth != null)
        {
            enemyHealth.OnDeath += () => OnEnemyDeath(spawnedEnemy);
        }
    }

    /// <summary>
    /// Handle enemy death
    /// </summary>
    private void OnEnemyDeath(GameObject enemy)
    {
        activeEnemies.Remove(enemy);
    }

    /// <summary>
    /// Stop spawning
    /// </summary>
    public void StopSpawning()
    {
        StopAllCoroutines();
        isSpawning = false;
    }

    /// <summary>
    /// Clear all active enemies
    /// </summary>
    public void ClearAllEnemies()
    {
        foreach (GameObject enemy in activeEnemies)
        {
            if (enemy != null)
            {
                Destroy(enemy);
            }
        }
        activeEnemies.Clear();
    }

    /// <summary>
    /// Add a wave
    /// </summary>
    public void AddWave(int enemyCount, float spawnDelay, float waveDelay)
    {
        waves.Add(new Wave
        {
            enemyCount = enemyCount,
            spawnDelay = spawnDelay,
            waveDelay = waveDelay
        });
    }

    // Getters
    public int GetCurrentWaveIndex() => currentWaveIndex;
    public int GetTotalWaves() => waves.Count;
    public int GetEnemiesSpawned() => enemiesSpawned;
    public int GetActiveEnemyCount() => activeEnemies.Count;
    public bool IsSpawning() => isSpawning;
    public List<GameObject> GetActiveEnemies() => new List<GameObject>(activeEnemies);
}
