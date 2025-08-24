using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public TDLevel level;
    [SerializeField] private int spawnerIndex = 0;
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private float spawnInterval = 1.0f;
    [SerializeField] private int countPerWave = 10;

    [Header("Motion defaults")]
    [SerializeField] private float enemySpeed = 3.5f;
    [SerializeField] private float enemyY = 0.1f;

    IReadOnlyList<Vector3> _waypoints;

    [Header("Leak settings")]
    [Min(1)] public int leakDamage = 1;   // how much health to remove per escaped enemy

    void Start()
    {
        if (!level) { Debug.LogError("[EnemySpawner] TDLevel missing."); return; }
        var wps = level.GetWaypoints(spawnerIndex);
        if (wps == null || wps.Count == 0) { Debug.LogError("[EnemySpawner] No waypoints for spawner " + spawnerIndex); return; }
        _waypoints = wps;
        if (_waypoints == null) return;
        StartCoroutine(SpawnWave());
    }

    IEnumerator SpawnWave()
    {
        for (int i = 0; i < countPerWave; i++)
        {
            var go = Instantiate(enemyPrefab);
            var agent = go.GetComponent<EnemyPathAgent>();
            if (!agent) agent = go.AddComponent<EnemyPathAgent>();
            agent.speed = enemySpeed;
            agent.yFixed = enemyY;
            agent.Init(_waypoints);

            // handle end-of-path
            agent.OnReachedEnd += OnEnemyReachedEnd;

            yield return new WaitForSeconds(spawnInterval);
        }
    }

    void OnEnemyReachedEnd(EnemyPathAgent agent)
    {
        // Apply leak damage, then clean up the enemy
        var hm = HealthManager.Instance;
        if (hm)
        {
            hm.Damage(leakDamage);
            Debug.Log($"[EnemySpawner] Enemy leaked. -{leakDamage} HP → {hm.Current}/{hm.maxHealth}");
        }
        else
        {
            Debug.LogWarning("[EnemySpawner] No HealthManager in scene.");
        }

        Destroy(agent.gameObject);
    }
}