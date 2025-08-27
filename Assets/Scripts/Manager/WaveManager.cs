using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class WaveManager : MonoBehaviour
{
    [Header("Lanes / Spawners")]
    [Tooltip("One EnemyManager per lane. Index here should match each manager's spawnerIndex.")]
    public EnemySpawner[] lanes;

    [Header("Enemy Prefabs + default speeds")]
    [SerializeField] private GameObject gruntPrefab;
    [SerializeField] private float gruntSpeed = 3.6f;
    [SerializeField] GameObject heavyPrefab;
    [SerializeField] private float heavySpeed = 2.2f;
    [SerializeField] private GameObject stealthPrefab;
    [SerializeField] private float stealthSpeed = 3.3f;
    [SerializeField] private GameObject bossPrefab;
    [SerializeField] float bossSpeed = 2.6f;

    [Header("Global placement")]
    [SerializeField] private float enemyY = 0.1f;


    [System.Serializable]
    public class WaveEntry
    {
        public string label = "Grunts Left";
        public EnemyKind kind = EnemyKind.Grunt;
        public int laneIndex = 0;           // which EnemyManager to use
        public int count = 10;
        public float interval = 0.8f;       // seconds between spawns
        public float speedOverride = -1f;   // <0 = use default for kind
    }

    [System.Serializable]
    public class Wave
    {
        public string name = "Wave";
        public WaveEntry[] entries;
        public float delayAfterWave = 3f;   // pause before next wave
    }

    [Header("Waves")]
    public List<Wave> waves = new List<Wave>();

    int _activeEnemies = 0;
    int _waveIndex = -1;
    bool _running;

    void OnEnable()
    {
        foreach (var lane in lanes)
        {
            if (!lane) continue;
            lane.EnemySpawned += OnEnemySpawned;
            lane.EnemyRemoved += OnEnemyRemoved;
        }
    }

    void OnDisable()
    {
        foreach (var lane in lanes)
        {
            if (!lane) continue;
            lane.EnemySpawned -= OnEnemySpawned;
            lane.EnemyRemoved -= OnEnemyRemoved;
        }
    }

    void Start()
    {
        // Ensure EnemyManagers do NOT auto-start their own waves.
        if (!_running) StartCoroutine(RunWaves());
    }

    IEnumerator RunWaves()
    {
        _running = true;

        for (_waveIndex = 0; _waveIndex < waves.Count; _waveIndex++)
        {
            var wave = waves[_waveIndex];
            Debug.Log($"[WaveManager] Starting Wave {_waveIndex + 1}: {wave.name}");

            // kick off all entries for this wave
            foreach (var spawn in wave.entries)
            {
                if (spawn.laneIndex < 0 || spawn.laneIndex >= lanes.Length || lanes[spawn.laneIndex] == null)
                {
                    Debug.LogWarning($"[WaveManager] Missing lane {spawn.laneIndex} for entry '{spawn.label}'. Skipping.");
                    continue;
                }

                var (prefab, speed) = ResolveArchetype(spawn.kind);
                if (!prefab)
                {
                    Debug.LogWarning($"[WaveManager] No prefab for {spawn.kind}. Skipping entry '{spawn.label}'.");
                    continue;
                }

                float useSpeed = (spawn.speedOverride > 0f) ? spawn.speedOverride : speed;
                lanes[spawn.laneIndex].SpawnBatch(prefab, spawn.count, spawn.interval, useSpeed, enemyY, spawn.kind);
            }

            // wait until all enemies are gone
            yield return new WaitUntil(() => _activeEnemies == 0);

            // small inter-wave delay
            if (wave.delayAfterWave > 0f)
                yield return new WaitForSeconds(wave.delayAfterWave);
        }

        Debug.Log("[WaveManager] All waves complete.");
        _running = false;
    }

    (GameObject prefab, float speed) ResolveArchetype(EnemyKind kind)
    {
        switch (kind)
        {
            case EnemyKind.Grunt: return (gruntPrefab, gruntSpeed);
            case EnemyKind.Heavy: return (heavyPrefab, heavySpeed);
            case EnemyKind.Stealth: return (stealthPrefab, stealthSpeed);
            case EnemyKind.Boss: return (bossPrefab, bossSpeed);
        }
        return (null, 0f);
    }

    // ---- callbacks from EnemyManager ----
    void OnEnemySpawned(EnemyPathAgent agent)
    {
        _activeEnemies++;
    }

    void OnEnemyRemoved(EnemyPathAgent agent)
    {
        _activeEnemies = Mathf.Max(0, _activeEnemies - 1);
    }
}
