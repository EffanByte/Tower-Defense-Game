using UnityEngine;

public class FlameTurret : MonoBehaviour
{
    [Header("Gun setup")]

    [Header("Targeting")]
    public string enemyLayerName = "Enemy";

    [Header("Base Stats")]
    [SerializeField] private int baseDamage = 10;
    [SerializeField] private float baseCooldown = 1.5f;
    [SerializeField] private float baseRange = 4f;

    private int enemyLayer;
    private float fireTimer = 0f;

    private TowerUpgrade upgrade;

    void Awake()
    {

        enemyLayer = LayerMask.NameToLayer(enemyLayerName);
        if (enemyLayer < 0)
            Debug.LogError($"Layer '{enemyLayerName}' not found! Add it in Unity's Layer settings.");
    }
    void Start()
    {
        upgrade = GetComponent<TowerUpgrade>();
        if (upgrade) upgrade.Init(baseDamage, baseCooldown, baseRange);
    }
    void Update()
    {
        fireTimer -= Time.deltaTime;

        if (fireTimer <= 0f)
        {
            ShootAll();
            fireTimer = upgrade.CurrentCooldown;
        }
    }

    void ShootAll()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, upgrade.CurrentRange, 1 << enemyLayer);
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
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.4f);
        Gizmos.DrawWireSphere(transform.position, upgrade.CurrentRange);
    }
}
