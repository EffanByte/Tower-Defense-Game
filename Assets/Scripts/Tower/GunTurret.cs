using UnityEngine;

public class GunTurret : MonoBehaviour
{
    [Header("Gun setup")]
    private Transform gun; // assign in inspector, or defaults to child 1

    [Header("Targeting")]
    [SerializeField] private float range = 8f;
    public string enemyLayerName = "Enemy";

    [Header("Firing")]
    [Tooltip("Seconds between shots (default 1.25s.")]
    public float fireCooldown = 1.25f;

    private int enemyLayer;
    private float fireTimer = 0f;

    void Awake()
    {
        if (!gun && transform.childCount > 1)
            gun = transform.GetChild(1);

        enemyLayer = LayerMask.NameToLayer(enemyLayerName);
        if (enemyLayer < 0)
            Debug.LogError($"Layer '{enemyLayerName}' not found! Add it in Unity's Layer settings.");
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
            // reset timer when no enemies (optional)
            fireTimer = 0f;
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
        // For now: immediately destroy the enemy
        Debug.Log($"[Turret] Destroyed {target.name}");
        Destroy(target.gameObject);
    }

    // Draw turret radius in editor
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, range);
    }
}
