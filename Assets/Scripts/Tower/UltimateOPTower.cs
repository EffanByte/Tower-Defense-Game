using UnityEngine;

public class UltimateOpTower : MonoBehaviour
{
    [Header("Tower Setup")]
    private Transform crown; // visual top piece, optional

    [Header("Pulse Attack")]
    [SerializeField] private float pulseRadius = 12f;
    [SerializeField] private float pulseCooldown = 5f;
    [SerializeField] private string enemyLayerName = "Enemy";

    [Header("Aura Buff")]
    [SerializeField] private float auraRadius = 6f;
    [SerializeField] private string towerLayerName = "Tower";
    [SerializeField] private float buffMultiplier = 1.5f; // +50% buff
    [SerializeField] private float buffDuration = 1.5f;  // refresh interval

    [Header("Damage")]
    [SerializeField] private int baseDamage = 50;
    private int enemyLayer;
    private int towerLayer;
    private float pulseTimer = 0f;

    void Awake()
    {
        if (transform.childCount > 0)
            crown = transform.GetChild(0);

        enemyLayer = LayerMask.NameToLayer(enemyLayerName);
        towerLayer = LayerMask.NameToLayer(towerLayerName);

        if (enemyLayer < 0)
            Debug.LogError($"Layer '{enemyLayerName}' not found!");
        if (towerLayer < 0)
            Debug.LogWarning($"Layer '{towerLayerName}' not found (for aura). Add a Tower layer if needed.");
    }

    void Update()
    {
        // Handle AOE pulse
        pulseTimer -= Time.deltaTime;
        if (pulseTimer <= 0f)
        {
            DoPulse();
            pulseTimer = pulseCooldown;
        }

        // Handle aura buff continuously
        ApplyAuraBuff();
    }

    void DoPulse()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, pulseRadius, 1 << enemyLayer);
        foreach (var hit in hits)
        {
            var health = hit.GetComponent<EnemyHealth>();
            var agent = hit.GetComponent<EnemyPathAgent>();
            var manager = agent ? agent.GetComponentInParent<EnemySpawner>() : null;

            if (health != null)
            {
                health.TakeDamage(baseDamage, manager, agent);
            }
        }

        // Optional: pulse visual
        if (crown)
            crown.localScale = Vector3.one * 1.2f; // tiny pop effect
    }

    void ApplyAuraBuff()
    {
        if (towerLayer < 0) return;

        Collider[] towers = Physics.OverlapSphere(transform.position, auraRadius, 1 << towerLayer);
        foreach (var t in towers)
        {
            var detail = t.GetComponent<TowerDetail>();
            if (detail != null)
            {
                // Here you can hook into your tower upgrade system
                // e.g., temporarily boost fireCooldown or damage
                Debug.Log($"[UltimateOpTower] Buffing tower {detail.towerName} (Lvl {detail.Level})");
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.2f, 0.2f, 0.4f);
        Gizmos.DrawWireSphere(transform.position, pulseRadius);

        Gizmos.color = new Color(0.2f, 0.5f, 1f, 0.25f);
        Gizmos.DrawWireSphere(transform.position, auraRadius);
    }
}
