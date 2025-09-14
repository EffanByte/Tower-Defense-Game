using UnityEngine;

[RequireComponent(typeof(TowerUpgrade))]
public class GunTurret : MonoBehaviour
{
    [Header("Gun setup")]
    [SerializeField] private Transform gun; // assign if you like; else it uses child(1)
    private Vector3 gunDefaultLocalPos;

    [Header("Base Stats")]
    [SerializeField] private int baseDamage = 10;
    [SerializeField] private float baseCooldown = 1.5f;
    [SerializeField] private float baseRange = 8f;

    [Header("Recoil")]
    [SerializeField] private float recoilDistance = 0.5f;
    private float recoilReturnSpeed;

    [Header("Targeting")]
    [SerializeField] string enemyLayerName = "Enemy";

    // ── MUZZLE FLASH / SHOOT SPRITE ───────────────────────────────────────────
    [Header("Muzzle Flash")]
    [Tooltip("Optional transform right in front of the barrel. If null, a small forward offset from 'gun' is used.")]
    [SerializeField] private Transform muzzleAnchor;

    [Tooltip("Prefab with a SpriteRenderer (recommended). If null, 'muzzleSprite' will be used to create a temp object.")]
    [SerializeField] private GameObject muzzleFlashPrefab;

    [Tooltip("Fallback sprite if no prefab is assigned.")]
    [SerializeField] private Sprite muzzleSprite;

    [Tooltip("How far in front of the gun to spawn (used only when no muzzleAnchor).")]
    [SerializeField] private float muzzleForwardOffset = 0.6f;

    [Tooltip("Uniform scale for the flash instance.")]
    [SerializeField] private float muzzleScale = 1.0f;

    [Tooltip("Seconds before the spawned flash object is destroyed.")]
    [SerializeField] private float muzzleLifetime = 0.06f;

    [Tooltip("Face the camera (billboard) instead of aligning to gun forward.")]
    [SerializeField] private bool billboardToCamera = false;

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

        if (target != null && gun != null)
        {
            // Rotate gun toward target
            Vector3 dir = (target.position - gun.position).normalized;
            Quaternion lookRot = Quaternion.LookRotation(dir, Vector3.up);
            gun.rotation = Quaternion.Slerp(gun.rotation, lookRot, Time.deltaTime * 10f);

            // Shooting cooldown
            fireTimer -= Time.deltaTime;
            if (fireTimer <= 0f)
            {
                Shoot(target);
                fireTimer = upgrade.CurrentCooldown;
            }
        }

        // Recoil return
        if (gun)
        {
            recoilReturnSpeed = upgrade.CurrentCooldown;
            recoilAmount = Mathf.MoveTowards(recoilAmount, 0f, recoilReturnSpeed * Time.deltaTime);
            gun.localPosition = gunDefaultLocalPos + gun.localRotation * Vector3.back * recoilAmount;
        }
    }

Transform GetClosestEnemy()
{
    float radius = upgrade ? upgrade.CurrentRange : baseRange;
    Collider[] hits = Physics.OverlapSphere(transform.position, radius, 1 << enemyLayer);

    // If this object is a "Sniper", it has special targeting priorities.
    if (this.CompareTag("Sniper"))
    {
        float minStealthDist = Mathf.Infinity;
        Transform nearestStealth = null;

        // --- Priority Pass: Find the closest "stealth" enemy ---
        foreach (var hit in hits)
        {
            // We only care about stealth enemies in this pass
            if (hit.CompareTag("Stealth"))
            {
                float dist = Vector3.Distance(transform.position, hit.transform.position);
                if (dist < minStealthDist)
                {
                    minStealthDist = dist;
                    nearestStealth = hit.transform;
                }
            }
        }

        if (nearestStealth != null)
            return nearestStealth;

    }

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

        // Damage
        var health = target.GetComponent<EnemyHealth>();
        var agent = target.GetComponent<EnemyPathAgent>();
        var manager = agent ? agent.GetComponentInParent<EnemySpawner>() : null;

        if (health != null)
            health.TakeDamage(upgrade.CurrentDamage, manager, agent);

        // Spawn the shoot sprite / muzzle flash
        SpawnMuzzleFlash();
    }

    void SpawnMuzzleFlash()
    {
        if (gun == null) return;

        // Position
        Vector3 pos = muzzleAnchor
            ? muzzleAnchor.position
            : gun.position + gun.forward * muzzleForwardOffset;

        // Rotation
        Quaternion rot;
        if (billboardToCamera && Camera.main)
        {
            // Make the sprite face the camera
            Vector3 toCam = Camera.main.transform.position - pos;
            if (toCam.sqrMagnitude < 0.0001f) toCam = -gun.forward; // fallback
            rot = Quaternion.LookRotation(toCam.normalized, Vector3.up);
        }
        else
        {
            // Align with barrel direction
            rot = Quaternion.LookRotation(gun.forward, Vector3.up);
        }

        GameObject fx = null;

        if (muzzleFlashPrefab)
        {
            fx = Instantiate(muzzleFlashPrefab, pos, rot * Quaternion.Euler(90,90,0)); // adjust if your sprite is not facing up
            fx.transform.localScale *= muzzleScale;
            Destroy(fx, muzzleLifetime);
            return;
        }

        // Fallback: build a temp GO with SpriteRenderer
        if (muzzleSprite)
        {
            fx = new GameObject("MuzzleFlash_TEMP");
            fx.transform.SetPositionAndRotation(pos, rot);
            fx.transform.localScale = Vector3.one * muzzleScale;

            var sr = fx.AddComponent<SpriteRenderer>();
            sr.sprite = muzzleSprite;
            sr.sortingOrder = 100; // ensure on top; adjust or set a sorting layer if you use them
            // Optional: emissive-looking material, e.g. "Sprites/Default" is fine for a quick flash

            Destroy(fx, muzzleLifetime);
        }
        // If neither prefab nor sprite is set, nothing spawns.
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        float r = upgrade ? upgrade.CurrentRange : baseRange;
        Gizmos.DrawWireSphere(transform.position, r);

        // Visualize muzzle spawn
        if (gun)
        {
            Vector3 pos = muzzleAnchor ? muzzleAnchor.position : gun.position + gun.forward * muzzleForwardOffset;
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(pos, 0.05f);
        }
    }
}
