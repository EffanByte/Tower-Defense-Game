using UnityEngine;

public class FlameTurret : MonoBehaviour
{
    [Header("Gun setup")]

    [Header("Targeting")]
    [SerializeField] private float range = 5f;
    public string enemyLayerName = "Enemy";

    [Header("Firing")]
    [Tooltip("Seconds between flame ticks (default 1s).")]
    [SerializeField] public float fireCooldown = 1f;

    private int enemyLayer;
    private float fireTimer = 0f;

    [Header("Damage")]
    [SerializeField] private int baseDamage = 5; // per tick
    void Awake()
    {

        enemyLayer = LayerMask.NameToLayer(enemyLayerName);
        if (enemyLayer < 0)
            Debug.LogError($"Layer '{enemyLayerName}' not found! Add it in Unity's Layer settings.");
    }

    void Update()
    {
        fireTimer -= Time.deltaTime;

        if (fireTimer <= 0f)
        {
            ShootAll();
            fireTimer = fireCooldown;
        }
    }

    void ShootAll()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, range, 1 << enemyLayer);
        foreach (var hit in hits)
        {
            var health = hit.GetComponent<EnemyHealth>();
            var agent = hit.GetComponent<EnemyPathAgent>();
            var manager = agent ? agent.GetComponentInParent<EnemySpawner>() : null;

            if (health != null)
            {
                health.TakeDamage(baseDamage, manager, agent);
                Debug.Log($"[FlameTurret] Burned {hit.name} for {baseDamage} dmg (HP left: {health.CurrentHealth})");
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.4f);
        Gizmos.DrawWireSphere(transform.position, range);
    }
}
