using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public TDLevel level;
    [SerializeField] private int spawnerIndex = 0;

    // These fields are for testing/default behaviour. WaveManager will override them.
    private GameObject enemyPrefab;
    private EnemyKind enemyName = EnemyKind.Grunt;
    private float spawnInterval = 1.0f;
    private int countPerWave = 10;
    [Header("Motion defaults")]
    private float enemySpeed = 3.5f;
    private float enemyY = 0.1f;

    IReadOnlyList<Vector3> _waypoints;

    [Header("Leak settings")]
    [Min(1)] public int leakDamage = 1;

    public event System.Action<EnemyPathAgent> EnemySpawned;
    public event System.Action<EnemyPathAgent> EnemyRemoved;

    void Start()
    {
        if (!level) { Debug.LogError("[EnemySpawner] TDLevel missing."); return; }
        var wps = level.GetWaypoints(spawnerIndex);
        if (wps == null || wps.Count == 0) { Debug.LogError("[EnemySpawner] No waypoints for spawner " + spawnerIndex); return; }
        _waypoints = wps;

        // This test spawn now requires a default health value to compile correctly.
        if (enemyPrefab)
            StartCoroutine(SpawnBatchRoutine(enemyPrefab, countPerWave, spawnInterval, enemySpeed, enemyY, enemyName, 10)); // <<< MODIFIED: Added default health for testing
    }
    public Coroutine SpawnBatch(GameObject prefab, int count, float interval, float speed, float y, EnemyKind enemyName, int healthOverride)
    {
        return StartCoroutine(SpawnBatchRoutine(prefab, count, interval, speed, y, enemyName, healthOverride));
    }

    IEnumerator SpawnBatchRoutine(GameObject prefab, int count, float interval, float speed, float y, EnemyKind enemyType, int healthOverride)
    {
        if (_waypoints == null) yield break;

        for (int i = 0; i < count; i++)
        {
            var go = Instantiate(prefab, this.transform);

            // <<< NEW: This block applies the scaled health to the new enemy. >>>
            var health = go.GetComponent<EnemyHealth>();
            if (health != null)
            {
                health.Init(healthOverride);
            }
            else
            {
                Debug.LogWarning($"[EnemySpawner] Prefab '{prefab.name}' is missing an EnemyHealth component!");
            }

            var anim = go.GetComponent<Animator>();
            if (anim)
            {
                if (prefab.CompareTag("Light"))
                    anim.SetInteger("MoveType", 1);
                else if (prefab.CompareTag("Heavy"))
                    anim.SetInteger("MoveType", 0);
            }
            var agent = go.GetComponent<EnemyPathAgent>();
            if (!agent) agent = go.AddComponent<EnemyPathAgent>();
            
            agent.speed = speed;
            agent.yFixed = y;
            agent.Init(_waypoints);

            EnemySpawned?.Invoke(agent);
            agent.OnReachedEnd += OnEnemyReachedEnd;

            yield return (interval > 0f) ? new WaitForSeconds(interval) : null;
        }
    }

    void OnEnemyReachedEnd(EnemyPathAgent agent)
    {
        var hm = HealthManager.GetHealthManager();
        if (hm) hm.Damage(leakDamage);
        else Debug.LogWarning("[EnemySpawner] No HealthManager in scene.");

        EnemyRemoved?.Invoke(agent);
        Destroy(agent.gameObject);
    }

    // This method is called from your combat code (e.g., EnemyHealth) when an enemy dies mid-path
    public void NotifyEnemyKilled(EnemyPathAgent agent)
    {
        // --- NEW: Increment the total enemies killed stat ---
        int totalKills = PlayerPrefs.GetInt("TotalKills", 0);
        totalKills++;
        PlayerPrefs.SetInt("TotalKills", totalKills);
        PlayerPrefs.Save(); // Use Save() if you want to write to disk immediately.

        // Notify WaveManager to update the active enemy count
        EnemyRemoved?.Invoke(agent);
    }
}