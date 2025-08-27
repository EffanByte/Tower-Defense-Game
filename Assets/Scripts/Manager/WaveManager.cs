using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    [Header("Lanes / Spawners")]
    [Tooltip("One EnemySpawner per lane. Index here should match each spawner's spawnerIndex.")]
    public EnemySpawner[] lanes;

    [Header("Enemy Prefabs + default speeds")]
    [SerializeField] private GameObject gruntPrefab; [SerializeField] private float gruntSpeed = 3.6f;
    [SerializeField] private GameObject heavyPrefab; [SerializeField] private float heavySpeed = 2.2f;
    [SerializeField] private GameObject stealthPrefab; [SerializeField] private float stealthSpeed = 3.3f;
    [SerializeField] private GameObject bossPrefab; [SerializeField] private float bossSpeed = 2.6f;

    [Header("Global placement")]
    [SerializeField] private float enemyY = 0.1f;

    [System.Serializable]
    public class WaveEntry
    {
        public string label = "Entry";
        public EnemyKind kind = EnemyKind.Grunt;
        public int laneIndex = 0;
        public int count = 10;
        public float interval = 0.8f;     // seconds between spawns
        public float speedOverride = -1f; // <0 = use default for kind
    }

    [System.Serializable]
    public class Wave
    {
        public string name = "Wave";
        public WaveEntry[] entries;
        public float delayAfterWave = 3f; // pause before next wave
    }

    [Header("Waves (manual)")]
    public List<Wave> waves = new List<Wave>();

    int _activeEnemies = 0;
    int _waveIndex = -1;
    bool _running;

    [Header("UI")]
    public TMPro.TextMeshProUGUI waveLabel;

    // ------------------ AUTO WAVES ------------------
    [Header("Auto waves (used after manual list ends)")]
    [Tooltip("If true, generate linear waves once authored waves are exhausted.")]
    public bool autoGenerateBeyondManual = true;

    [Tooltip("Boss wave cadence (first boss at this wave). 8 = boss at 8, 16, 24...")]
    public int bossEvery = 8;

    [Tooltip("Base counts & linear growth per wave (grunts always on; heavies unlock ~wave3; stealth unlock ~wave2).")]
    public int baseGrunts = 8;
    public int gruntsPerWave = 3;

    public int baseHeavies = 0;
    public int heaviesPerWave = 1;
    public int heavyUnlockWave = 3;

    public int baseStealth = 0;
    public int stealthPer2Waves = 1; // +1 stealth every 2 waves
    public int stealthUnlockWave = 2;

    [Tooltip("Spawn pacing over waves.")]
    public float autoBaseInterval = 0.8f;
    public float autoIntervalFalloff = 0.04f;  // faster spawns later
    public float autoMinInterval = 0.25f;

    [Tooltip("Delay between auto waves.")]
    public float autoDelayAfterWave = 3f;

    [Tooltip("Generate at most this many auto waves after manual; set large for endless.")]
    public int maxAutoWaves = 999;

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
        if (!_running) StartCoroutine(RunWaves());
    }

    IEnumerator RunWaves()
    {
        _running = true;

        // --- 1) Run authored waves (if any) ---
        for (_waveIndex = 0; _waveIndex < waves.Count; _waveIndex++)
        {
            yield return StartCoroutine(RunSingleWave(GetManualWave(_waveIndex), _waveIndex + 1));
        }

        // --- 2) Auto-generate more waves if enabled ---
        if (autoGenerateBeyondManual && lanes != null && lanes.Length > 0)
        {
            int autoStartNum = _waveIndex + 1; // next visible wave number
            for (int i = 0; i < maxAutoWaves; i++)
            {
                int waveNum = autoStartNum + i;
                var wave = BuildAutoWave(waveNum);
                yield return StartCoroutine(RunSingleWave(wave, waveNum));
            }
        }

        Debug.Log("[WaveManager] All waves complete.");
        _running = false;
    }

    IEnumerator RunSingleWave(Wave wave, int visibleWaveNumber)
    {
        CleanupDeadEnemies();

        if (waveLabel) waveLabel.text = $"Wave: {visibleWaveNumber}";
        Debug.Log($"[WaveManager] Starting Wave {visibleWaveNumber}: {wave.name}");

        // launch entries
        foreach (var spawn in wave.entries)
        {
            if (!IsValidLane(spawn.laneIndex))
            {
                Debug.LogWarning($"[WaveManager] Missing lane {spawn.laneIndex} for entry '{spawn.label}'. Skipping.");
                continue;
            }

            var (prefab, speedDefault) = ResolveArchetype(spawn.kind);
            if (!prefab)
            {
                Debug.LogWarning($"[WaveManager] No prefab for {spawn.kind}. Skipping entry '{spawn.label}'.");
                continue;
            }

            float useSpeed = (spawn.speedOverride > 0f) ? spawn.speedOverride : speedDefault;
            lanes[spawn.laneIndex].SpawnBatch(prefab, spawn.count, spawn.interval, useSpeed, enemyY, spawn.kind);
        }

        // optional: log expected cash for tuning
        LogExpectedCash(wave, visibleWaveNumber);

        // wait until wave cleared
        yield return new WaitUntil(() => _activeEnemies == 0);

        // small inter-wave delay
        float pause = wave.delayAfterWave > 0f ? wave.delayAfterWave : autoDelayAfterWave;
        if (pause > 0f) yield return new WaitForSeconds(pause);
    }

    Wave GetManualWave(int index) => waves[index];

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

    bool IsValidLane(int idx) => (idx >= 0 && lanes != null && idx < lanes.Length && lanes[idx] != null);

    // ------------------ AUTO WAVE BUILDER ------------------
    Wave BuildAutoWave(int waveNumber)
    {
        // linear growth
        int grunts = baseGrunts + gruntsPerWave * (waveNumber - 1);

        int heavies = 0;
        if (waveNumber >= heavyUnlockWave)
            heavies = baseHeavies + Mathf.Max(0, (waveNumber - heavyUnlockWave + 1)) * heaviesPerWave;

        int stealth = 0;
        if (waveNumber >= stealthUnlockWave)
            stealth = baseStealth + ((waveNumber - stealthUnlockWave + 2) / 2) * stealthPer2Waves; // +1 every 2 waves

        bool bossWave = (bossEvery > 0 && (waveNumber % bossEvery) == 0);
        int bosses = bossWave ? 1 : 0;

        // make boss waves spicier but not insane: reduce trash a bit on boss waves
        if (bossWave)
        {
            grunts = Mathf.RoundToInt(grunts * 0.7f);
            heavies = Mathf.RoundToInt(heavies * 0.8f);
        }

        float interval = Mathf.Max(autoMinInterval, autoBaseInterval - autoIntervalFalloff * (waveNumber - 1));

        var entries = new List<WaveEntry>();

        // Split each count across lanes fairly (alternating remainders)
        void AddSplit(EnemyKind kind, int totalCount, string labelPrefix)
        {
            if (totalCount <= 0 || lanes == null || lanes.Length == 0) return;
            int n = lanes.Length;
            int basePerLane = totalCount / n;
            int remainder = totalCount % n;

            for (int i = 0; i < n; i++)
            {
                int c = basePerLane + (i < remainder ? 1 : 0);
                if (c <= 0) continue;
                entries.Add(new WaveEntry
                {
                    label = $"{labelPrefix} L{i}",
                    kind = kind,
                    laneIndex = i,
                    count = c,
                    interval = interval,
                    speedOverride = -1f
                });
            }
        }

        AddSplit(EnemyKind.Grunt, grunts, "Grunts");
        AddSplit(EnemyKind.Heavy, heavies, "Heavies");
        AddSplit(EnemyKind.Stealth, stealth, "Stealth");

        if (bosses > 0)
        {
            // send boss down alternating lanes to keep pressure varied
            int bossLane = (waveNumber / bossEvery) % (lanes.Length > 0 ? lanes.Length : 1);
            entries.Add(new WaveEntry
            {
                label = "Boss",
                kind = EnemyKind.Boss,
                laneIndex = bossLane,
                count = bosses,
                interval = Mathf.Max(0.6f, interval), // pace boss reasonably
                speedOverride = -1f
            });
        }

        return new Wave
        {
            name = bossWave ? $"Auto {waveNumber} (BOSS)" : $"Auto {waveNumber}",
            entries = entries.ToArray(),
            delayAfterWave = autoDelayAfterWave
        };
    }

    // Rough reward projection to sanity-check economy pacing
    void LogExpectedCash(Wave wave, int waveNumber)
    {
        var ec = EconomyController.Instance;
        int rGrunt = ec ? ec.gruntReward : 5;
        int rStealth = ec ? ec.scoutReward : 10; // your design: Stealth == old Scout payout
        int rHeavy = ec ? ec.heavyReward : 20;
        int rBoss = ec ? ec.bossReward : 500;

        int total = 0, g = 0, s = 0, h = 0, b = 0;
        foreach (var e in wave.entries)
        {
            switch (e.kind)
            {
                case EnemyKind.Grunt: total += e.count * rGrunt; g += e.count; break;
                case EnemyKind.Stealth: total += e.count * rStealth; s += e.count; break;
                case EnemyKind.Heavy: total += e.count * rHeavy; h += e.count; break;
                case EnemyKind.Boss: total += e.count * rBoss; b += e.count; break;
            }
        }
        Debug.Log($"[WaveManager] Wave {waveNumber} expected cash ≈ ${total}  (G:{g} S:{s} H:{h} B:{b})");
    }

    // ---- callbacks from EnemySpawner ----
    void OnEnemySpawned(EnemyPathAgent agent) { _activeEnemies++; }
    void OnEnemyRemoved(EnemyPathAgent agent) { _activeEnemies = Mathf.Max(0, _activeEnemies - 1); }

    void CleanupDeadEnemies()
    {
        int deadLayer = LayerMask.NameToLayer("Dead");
        var corpses = GameObject.FindObjectsOfType<EnemyHealth>();
        foreach (var enemy in corpses)
            if (enemy && enemy.gameObject.layer == deadLayer)
                Destroy(enemy.gameObject);
    }
}
