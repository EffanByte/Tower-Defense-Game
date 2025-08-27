using UnityEngine;

[RequireComponent(typeof(TowerUpgrade))]
public class GunTurret : MonoBehaviour
{
    [Header("Gun setup")]
    private Transform gun; // assign in inspector, or defaults to child 1
    private Vector3 gunDefaultLocalPos;

    [Header("Base Stats")]
    [SerializeField] private int baseDamage = 10;
    [SerializeField] private float baseCooldown = 1.5f;
    [SerializeField] private float baseRange = 8f;

    [Header("Recoil")]
    [SerializeField] private float recoilDistance = 0.5f;
    private float recoilReturnSpeed;

    [Header("Targeting")]
    public string enemyLayerName = "Enemy";

    private TowerUpgrade upgrade;
    private int enemyLayer;
    private float fireTimer = 0f;
    private float recoilAmount = 0f;

    void Awake()
    {
        if (!gun && transform.childCount > 1)
            gun = transform.GetChild(1);
        if (gun) gunDefaultLocalPos = gun.localPosition;

        upgrade = GetComponent<TowerUpgrade>();
        if (upgrade)
            upgrade.Init(baseDamage, baseCooldown, baseRange);

        enemyLayer = LayerMask.NameToLayer(enemyLayerName);
        if (enemyLayer < 0)
            Debug.LogError($"Layer '{enemyLayerName}' not found! Add it in Unity's Layer settings.");

        recoilReturnSpeed = baseCooldown;
    }

    void Update()
    {
        Transform target = GetClosestEnemy();

        if (target != null)
        {
            // Rotate gun toward target
            Vector3 dir = (target.position - gun.position).normalized;
            Quaternion lookRot = Quaternion.LookRotation(dir, Vector3.up);
            gun.rotation = Quaternion.Slerp(gun.rotation, lookRot, Time.deltaTime * 10f);

            // Handle shooting cooldown
            fireTimer -= Time.deltaTime;
            if (fireTimer <= 0f)
            {
                Shoot(target);
                fireTimer = upgrade.CurrentCooldown;
            }
        }

        // Smoothly return from recoil
        if (gun)
        {
            recoilReturnSpeed = upgrade.CurrentCooldown; // use current cooldown scaling
            recoilAmount = Mathf.MoveTowards(recoilAmount, 0f, recoilReturnSpeed * Time.deltaTime);
            gun.localPosition = gunDefaultLocalPos + gun.localRotation * Vector3.back * recoilAmount;
        }
    }

    Transform GetClosestEnemy()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, upgrade.CurrentRange, 1 << enemyLayer);

        float minDist = Mathf.Infinity;
        Transform nearest = null;

        foreach (var hit in hits)
        {
            float dist = Vector3.Distance(transform.position, hit.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = hit.transform;
            }
        }

        return nearest;
    }

    void Shoot(Transform target)
    {
        recoilAmount = recoilDistance;

        var health = target.GetComponent<EnemyHealth>();
        var agent = target.GetComponent<EnemyPathAgent>();
        var manager = agent ? agent.GetComponentInParent<EnemySpawner>() : null;

        if (health != null)
        {
            health.TakeDamage(upgrade.CurrentDamage, manager, agent);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        float r = upgrade ? upgrade.CurrentRange : baseRange;
        Gizmos.DrawWireSphere(transform.position, r);
    }
}
