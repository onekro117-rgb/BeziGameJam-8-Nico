using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class WaveManager : MonoBehaviour
{
    public static WaveManager Instance { get; private set; }

    [Header("Configuration")]
    [SerializeField] private WaveSystemConfig waveConfig;
    
    [Header("Spawn Settings")]
    [SerializeField] private Transform spawnPointsParent;
    
    [Header("References")]
    [SerializeField] private Transform playerRespawnPoint;
    [SerializeField] private Transform player;
    [SerializeField] private GameObject goalObject;
    
    [Header("Events")]
    public UnityEvent<int> OnWaveStart;
    public UnityEvent<int> OnWaveComplete;
    public UnityEvent OnAllWavesComplete;

    private int currentWaveIndex = 0;
    private int enemiesSpawnedThisWave = 0;
    private int enemiesToSpawnThisWave = 0;
    private List<GameObject> activeEnemies = new List<GameObject>();
    private List<Transform> spawnPoints = new List<Transform>();
    private List<Transform> availableSpawnPoints = new List<Transform>();
    private Coroutine spawnCoroutine;
    private bool isWaveActive = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        InitializeSpawnPoints();
    }

    private void Start()
    {
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
        }

        if (playerRespawnPoint == null)
        {
            GameObject respawnObj = GameObject.Find("Respawn Point");
            if (respawnObj != null)
            {
                playerRespawnPoint = respawnObj.transform;
            }
        }

        StartWave(0);
    }

    private void InitializeSpawnPoints()
    {
        if (spawnPointsParent == null)
        {
            Debug.LogError("WaveManager: Spawn Points Parent no asignado!");
            return;
        }

        spawnPoints.Clear();
        foreach (Transform child in spawnPointsParent)
        {
            spawnPoints.Add(child);
        }

        if (spawnPoints.Count == 0)
        {
            Debug.LogError("WaveManager: No hay puntos de spawn configurados!");
        }
    }

    public void StartWave(int waveIndex)
    {
        if (waveConfig == null || waveConfig.Waves.Count == 0)
        {
            Debug.LogError("WaveManager: No hay configuración de oleadas!");
            return;
        }

        if (waveIndex >= waveConfig.Waves.Count)
        {
            CompleteAllWaves();
            return;
        }

        currentWaveIndex = waveIndex;
        WaveData currentWave = waveConfig.Waves[currentWaveIndex];
        
        enemiesToSpawnThisWave = currentWave.GetTotalEnemyCount();
        enemiesSpawnedThisWave = 0;
        activeEnemies.Clear();
        isWaveActive = true;

        OnWaveStart?.Invoke(currentWaveIndex + 1);
        Debug.Log($"=== OLEADA {currentWaveIndex + 1} INICIADA === ({enemiesToSpawnThisWave} enemigos)");

        if (goalObject != null)
        {
            goalObject.SetActive(false);
        }

        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
        }
        spawnCoroutine = StartCoroutine(SpawnWaveEnemies(currentWave));
    }

    private IEnumerator SpawnWaveEnemies(WaveData wave)
    {
        int initialCount = Mathf.Min(waveConfig.InitialSpawnCount, enemiesToSpawnThisWave);
        SpawnEnemies(wave, initialCount);

        while (enemiesSpawnedThisWave < enemiesToSpawnThisWave)
        {
            yield return new WaitForSeconds(waveConfig.SpawnInterval);

            int spawnCount = Mathf.Min(waveConfig.ContinuousSpawnCount, enemiesToSpawnThisWave - enemiesSpawnedThisWave);
            if (spawnCount > 0)
            {
                SpawnEnemies(wave, spawnCount);
            }
        }
    }

    private void SpawnEnemies(WaveData wave, int count)
    {
        List<EnemySpawnInfo> availableEnemies = new List<EnemySpawnInfo>();
        foreach (var enemyInfo in wave.Enemies)
        {
            availableEnemies.Add(enemyInfo);
        }

        availableSpawnPoints.Clear();
        availableSpawnPoints.AddRange(spawnPoints);

        for (int i = 0; i < count; i++)
        {
            if (availableEnemies.Count == 0) break;

            EnemySpawnInfo selectedInfo = availableEnemies[Random.Range(0, availableEnemies.Count)];
            
            if (selectedInfo.enemyPrefab != null && availableSpawnPoints.Count > 0)
            {
                int randomIndex = Random.Range(0, availableSpawnPoints.Count);
                Transform spawnPoint = availableSpawnPoints[randomIndex];
                availableSpawnPoints.RemoveAt(randomIndex);
                
                GameObject enemy = Instantiate(selectedInfo.enemyPrefab, spawnPoint.position, Quaternion.identity);
                
                HealthEnemies enemyHealth = enemy.GetComponent<HealthEnemies>();
                if (enemyHealth != null)
                {
                    enemyHealth.OnDeath += () => OnEnemyDied(enemy);
                }

                activeEnemies.Add(enemy);
                enemiesSpawnedThisWave++;

                int remainingOfThisType = 0;
                foreach (var info in wave.Enemies)
                {
                    if (info.enemyPrefab == selectedInfo.enemyPrefab)
                    {
                        remainingOfThisType = info.count - CountEnemiesOfType(selectedInfo.enemyPrefab);
                        break;
                    }
                }

                if (remainingOfThisType <= 0)
                {
                    availableEnemies.Remove(selectedInfo);
                }
            }
            else if (availableSpawnPoints.Count == 0)
            {
                Debug.LogWarning($"WaveManager: No hay suficientes spawn points disponibles. {count - i} enemigos no spawneados en este batch.");
                break;
            }
        }

        Debug.Log($"Spawneados {Mathf.Min(count, spawnPoints.Count)} enemigos. Total spawneado: {enemiesSpawnedThisWave}/{enemiesToSpawnThisWave}");
    }

    private int CountEnemiesOfType(GameObject prefab)
    {
        int count = 0;
        foreach (var enemy in activeEnemies)
        {
            if (enemy != null && enemy.name.Contains(prefab.name))
            {
                count++;
            }
        }
        return count;
    }

    private void OnEnemyDied(GameObject enemy)
    {
        if (activeEnemies.Contains(enemy))
        {
            activeEnemies.Remove(enemy);
        }

        CheckWaveCompletion();
    }

    private void CheckWaveCompletion()
    {
        activeEnemies.RemoveAll(enemy => enemy == null);

        if (activeEnemies.Count == 0 && enemiesSpawnedThisWave >= enemiesToSpawnThisWave && isWaveActive)
        {
            CompleteWave();
        }
    }

    private void CompleteWave()
    {
        isWaveActive = false;
        
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
        }

        OnWaveComplete?.Invoke(currentWaveIndex + 1);
        Debug.Log($"=== OLEADA {currentWaveIndex + 1} COMPLETADA ===");

        if (goalObject != null)
        {
            goalObject.SetActive(true);
            
            GoalTrigger goalTrigger = goalObject.GetComponent<GoalTrigger>();
            if (goalTrigger != null)
            {
                goalTrigger.ResetTrigger();
            }
        }
    }

    public void OnPlayerReachedGoal()
    {
        Debug.Log("=== PLAYER ALCANZÓ EL GOAL ===");
        
        Time.timeScale = 0f;

        if (UIManager.Instance != null)
        {
            ModifierManager modifierManager = FindFirstObjectByType<ModifierManager>();
            GameManager gameManager = FindFirstObjectByType<GameManager>();
            
            if (modifierManager != null && gameManager != null)
            {
                ModifierOffer[] offers = modifierManager.GenerateOffers(3);
                UIManager.Instance.ShowModifierChoices(offers, (selectedIndex) => OnModifierSelected(selectedIndex, offers[selectedIndex], modifierManager, gameManager));
            }
        }
    }

    private void OnModifierSelected(int index, ModifierOffer offer, ModifierManager modifierManager, GameManager gameManager)
    {
        modifierManager.ApplyOffer(offer, gameManager);
        PrepareNextWave();
    }

    private void PrepareNextWave()
    {
        Time.timeScale = 1f;

        if (goalObject != null)
        {
            goalObject.SetActive(false);
        }

        if (player != null && playerRespawnPoint != null)
        {
            player.position = playerRespawnPoint.position;
        }

        StartWave(currentWaveIndex + 1);
    }

    private void CompleteAllWaves()
    {
        Debug.Log("=== TODAS LAS OLEADAS COMPLETADAS ===");
        OnAllWavesComplete?.Invoke();
    }

    public void RestartWaves()
    {
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
        }

        foreach (var enemy in activeEnemies)
        {
            if (enemy != null)
            {
                Destroy(enemy);
            }
        }
        activeEnemies.Clear();

        Time.timeScale = 1f;

        if (player != null && playerRespawnPoint != null)
        {
            player.position = playerRespawnPoint.position;
        }

        StartWave(0);
    }

    public int GetCurrentWaveNumber()
    {
        return currentWaveIndex + 1;
    }

    public int GetTotalWaves()
    {
        return waveConfig != null ? waveConfig.Waves.Count : 0;
    }
}
