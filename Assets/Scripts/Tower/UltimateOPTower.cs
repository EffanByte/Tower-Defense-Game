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
        Collider[] hits = Physics.OverlapSphere(transform.position, upgrade.CurrentRange, 1 << enemyLayer);
        foreach (var hit in hits)
        {
            var health = hit.GetComponent<EnemyHealth>();
            var agent = hit.GetComponent<EnemyPathAgent>();
            var manager = agent ? agent.GetComponentInParent<EnemySpawner>() : null;

            if (health != null)
            {
                health.TakeDamage(upgrade.CurrentDamage, manager, agent);
            }
        }

        if (crown)
            crown.localScale = Vector3.one * 1.2f;
    }

    void ApplyAuraBuff()
{
    if (towerLayer < 0) return;

    foreach (var t in Object.FindObjectsOfType<TowerUpgrade>())
        t.ClearAuraBuff();

    Collider[] towers = Physics.OverlapSphere(transform.position, upgrade.CurrentRange / 2, 1 << towerLayer);
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
        Gizmos.DrawWireSphere(transform.position, upgrade ? upgrade.CurrentRange : baseRange);

        Gizmos.color = new Color(0.2f, 0.5f, 1f, 0.25f);
        Gizmos.DrawWireSphere(transform.position, (upgrade ? upgrade.CurrentRange : baseRange) / 2);
    }
}
