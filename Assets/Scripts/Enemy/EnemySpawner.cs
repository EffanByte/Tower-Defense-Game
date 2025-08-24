using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public TDLevel level;
    public int spawnerIndex = 0;
    public GameObject enemyPrefab;
    public float spawnInterval = 1.0f;
    public int countPerWave = 10;

    [Header("Motion defaults")]
    public float enemySpeed = 3.5f;
    public float enemyY = 0.1f;

    IReadOnlyList<Vector3> _waypoints;  

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
        // TODO: apply base damage, then destroy or return to pool
        Destroy(agent.gameObject);
    }
}