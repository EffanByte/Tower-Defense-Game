using UnityEngine;

public class UltimateOpTower : MonoBehaviour
{
    [Header("Visual")]
    private Transform crown;

    [Header("Pulse Attack")]
    [SerializeField] private float pulseCooldown = 5f;
    [SerializeField] private string enemyLayerName = "Enemy";

    [Header("Aura Buff")]
    [SerializeField] private string towerLayerName = "Tower";
    [SerializeField] private float buffMultiplier = 1.5f;
    [SerializeField] private float buffDuration = 1.5f;

    [Header("Base Stats")]
    [SerializeField] private int baseDamage = 50;
    [SerializeField] private float baseRange = 12f;

    // ── Flame Ring FX ─────────────────────────────────────────────────────────
    [Header("Flame Ring FX")]
    [Tooltip("Prefab with SphereCollider (isTrigger) + child SpriteRenderer. (Can be the same prefab used by your flame tower.)")]
    [SerializeField] private GameObject blueflameRingPrefab;

    [SerializeField] private float ringExpandSpeed = 5f;
    [SerializeField] private float ringSpinSpeed = 180f;
    [SerializeField] private Color ringColor = new Color(0.35f, 0.65f, 1.0f, 1f); // blue-ish
    [SerializeField] private float ringYOffset = 0.05f; // hover slightly above ground

    private TowerUpgrade upgrade;
    private int enemyLayer;
    private int towerLayer;
    private float pulseTimer = 0f;

    void Awake()
    {
        if (transform.childCount > 0)
            crown = transform.GetChild(0);

        enemyLayer = LayerMask.NameToLayer(enemyLayerName);
        towerLayer = LayerMask.NameToLayer(towerLayerName);

        upgrade = GetComponent<TowerUpgrade>();
        if (upgrade)
        {
            // Init with stronger scaling than normal towers
            upgrade.InitOP(
                baseDamage,       // starting damage
                pulseCooldown,    // cooldown baseline
                baseRange,        // range baseline
                20, 5,            // bigDamageStep, smallDamageStep
                0.5f, 0.1f,       // bigSpeedStep, smallSpeedStep
                4f, 6             // rangeStep, maxRangeLvl
            );
        }

        if (enemyLayer < 0) Debug.LogError($"Layer '{enemyLayerName}' not found!");
        if (towerLayer < 0) Debug.LogWarning($"Layer '{towerLayerName}' not found!");
    }

    void Update()
    {
        pulseTimer -= Time.deltaTime;
        if (pulseTimer <= 0f)
        {
            DoPulse();
            pulseTimer = upgrade ? upgrade.CurrentCooldown : pulseCooldown;
        }

        ApplyAuraBuff();
    }

    void DoPulse()
    {
        // Spawn a blue, spinning ring that expands and damages enemies on contact
        SpawnFlameRing();

        if (crown)
            crown.localScale = Vector3.one * 1.2f; // quick visual kick (optional)
    }

    void SpawnFlameRing()
    {
        if (!blueflameRingPrefab)
        {
            Debug.LogWarning("[UltimateOpTower] FlameRing prefab not set.");
            return;
        }

        var maxRadius = upgrade ? upgrade.CurrentRange : baseRange;

        var go = Instantiate(
            blueflameRingPrefab,
            new Vector3(transform.position.x, transform.position.y + ringYOffset, transform.position.z),
            Quaternion.identity
        );

        // Ensure a FlameRing component exists (it can be on the prefab already)
        var ring = go.GetComponent<BlueFlameRing>();
        if (!ring) ring = go.AddComponent<BlueFlameRing>();

        // Initialize: damage, layers, expansion, spin, radius, color
        int enemyMask = 1 << enemyLayer;
        float dmg = upgrade ? upgrade.CurrentDamage : baseDamage;
        ring.Init(dmg, enemyMask, ringExpandSpeed, maxRadius, ringSpinSpeed, ringColor);
    }

    void ApplyAuraBuff()
    {
        if (towerLayer < 0) return;

        foreach (var t in Object.FindObjectsOfType<TowerUpgrade>())
            t.ClearAuraBuff();

        Collider[] towers = Physics.OverlapSphere(transform.position, (upgrade ? upgrade.CurrentRange : baseRange) / 2, 1 << towerLayer);
        foreach (var t in towers)
        {
            var tu = t.GetComponent<TowerUpgrade>();
            if (tu != null)
                tu.SetAuraBuff(buffMultiplier, buffMultiplier);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.2f, 0.2f, 0.4f);
        float r = upgrade ? upgrade.CurrentRange : baseRange;
        Gizmos.DrawWireSphere(transform.position, r);

        Gizmos.color = new Color(0.2f, 0.5f, 1f, 0.25f);
        Gizmos.DrawWireSphere(transform.position, r / 2f);
    }
}
