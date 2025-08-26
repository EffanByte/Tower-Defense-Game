using UnityEngine;

public class GunTurret : MonoBehaviour
{
    [Header("Gun setup")]
    private Transform gun; // assign in inspector, or defaults to child 1
    private Vector3 gunDefaultLocalPos;

    [Header("Targeting")]
    [SerializeField] private float range = 8f;
    public string enemyLayerName = "Enemy";

    [Header("Firing")]
    [Tooltip("Seconds between shots (default 1.5s).")]
    [SerializeField] public float fireCooldown = 1.5f;

    [Header("Recoil")]
    [SerializeField] private float recoilDistance = 0.2f;   // how far back the gun jumps
    private float recoilReturnSpeed; // how quickly it returns

    private int enemyLayer;
    public float fireTimer = 0f;
    private float recoilAmount = 0f; // how much recoil is currently applied

    void Awake()
    {
        if (!gun && transform.childCount > 1)
            gun = transform.GetChild(1);
        if (gun) gunDefaultLocalPos = gun.localPosition;

        enemyLayer = LayerMask.NameToLayer(enemyLayerName);
        if (enemyLayer < 0)
            Debug.LogError($"Layer '{enemyLayerName}' not found! Add it in Unity's Layer settings.");
        recoilReturnSpeed = fireCooldown;
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
                fireTimer = fireCooldown; // reset cooldown
            }
        }
        else
        {
            fireTimer = 0f;
        // Apply standard rotation later with more coherent code
        //    gun.transform.Rotate(0, 90 * Time.deltaTime, 0);
        }

        // Smoothly return from recoil
        if (gun)
        {
            recoilAmount = Mathf.MoveTowards(recoilAmount, 0f, recoilReturnSpeed * Time.deltaTime);
            gun.localPosition = gunDefaultLocalPos + gun.localRotation * Vector3.back * recoilAmount;
        }
    }

    Transform GetClosestEnemy()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, range, 1 << enemyLayer);

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
        var agent = target.GetComponent<EnemyPathAgent>();
        if (agent != null)
        {
            var manager = agent.GetComponentInParent<EnemySpawner>();
            if (manager != null)
            {
                manager.NotifyEnemyKilled(agent);

                // increment this tower's kills
                var details = GetComponent<TowerDetail>();
                if (details) details.AddKill();

                Debug.Log($"[Turret] Shot {target.name} → notified {manager.name}");
            }
            else
            {
                Debug.LogWarning($"[Turret] No EnemyManager found for {target.name}, destroying directly.");
                Destroy(agent.gameObject);
            }
        }
        else
        {
            Debug.LogWarning($"[Turret] Target {target.name} has no EnemyPathAgent, destroying directly.");
            Destroy(target.gameObject);
        }
    }


    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, range);
    }
}
