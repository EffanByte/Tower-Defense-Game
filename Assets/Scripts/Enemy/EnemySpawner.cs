using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public TDLevel level;
    [SerializeField] private int spawnerIndex = 0;

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

    // NEW: events so WaveManager can count active enemies
    public event System.Action<EnemyPathAgent> EnemySpawned;
    public event System.Action<EnemyPathAgent> EnemyRemoved;

    void Start()
    {
        if (!level) { Debug.LogError("[EnemyManager] TDLevel missing."); return; }
        var wps = level.GetWaypoints(spawnerIndex);
        if (wps == null || wps.Count == 0) { Debug.LogError("[EnemyManager] No waypoints for spawner " + spawnerIndex); return; }
        _waypoints = wps;

        if (enemyPrefab)
            StartCoroutine(SpawnBatchRoutine(enemyPrefab, countPerWave, spawnInterval, enemySpeed, enemyY, enemyName));
    }

    // NEW: public API for WaveManager
    public Coroutine SpawnBatch(GameObject prefab, int count, float interval, float speed, float y, EnemyKind enemyName)
    {
        return StartCoroutine(SpawnBatchRoutine(prefab, count, interval, speed, y, enemyName));
    }

    IEnumerator SpawnBatchRoutine(GameObject prefab, int count, float interval, float speed, float y, EnemyKind enemyType)
    {
        if (_waypoints == null) yield break;

        for (int i = 0; i < count; i++)
        {
            var go = Instantiate(prefab, this.transform);
            var anim = go.GetComponent<Animator>();
            if (anim)
            {
                // convention: 0 = walk, 1 = run
                if (prefab.CompareTag("Light"))
                    anim.SetInteger("MoveType", 1);   // running
                else if (prefab.CompareTag("Heavy"))
                    anim.SetInteger("MoveType", 0);   // walking
            }
            var agent = go.GetComponent<EnemyPathAgent>();
            if (!agent) agent = go.AddComponent<EnemyPathAgent>();
            
            agent.speed = speed;
            agent.yFixed = y;
            agent.Init(_waypoints);

            // notify wave manager
            EnemySpawned?.Invoke(agent);

            // handle end-of-path
            agent.OnReachedEnd += OnEnemyReachedEnd;

            yield return (interval > 0f) ? new WaitForSeconds(interval) : null;
        }
    }

    void OnEnemyReachedEnd(EnemyPathAgent agent)
    {
        // Apply leak damage
        var hm = HealthManager.GetHealthManager();
        if (hm) hm.Damage(leakDamage);
        else Debug.LogWarning("[EnemyManager] No HealthManager in scene.");

        // notify removal and cleanup
        EnemyRemoved?.Invoke(agent);
        Destroy(agent.gameObject);
    }

    // (Optional) call this from your combat code when an enemy dies mid-path
    public void NotifyEnemyKilled(EnemyPathAgent agent)
    {
        EnemyRemoved?.Invoke(agent);
        if (agent) Destroy(agent.gameObject);
    }
}
