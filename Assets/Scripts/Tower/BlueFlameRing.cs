using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class BlueFlameRing : MonoBehaviour
{
    [Header("Settings")]
    // ADD THIS VARIABLE: Controls how much larger the visual is than the collider.
    // 2f means the visual diameter will be twice the collider's diameter.
    [SerializeField] private float visualToColliderRatio = 2f;

    [Header("Runtime (filled by Init)")]
    private float expandSpeed = 5f;
    private float maxRadius   = 4f; // This will now correctly be the FINAL RADIUS of the collider.
    private float spinSpeed   = 180f;
    private float damage      = 10f;
    private Color ringColor   = Color.cyan;
    private LayerMask enemyMask;

    [Header("Visual Fade")]
     private float startAlpha = 1f;
     private float endAlpha   = 0f;

    private SphereCollider col;
    private Vector3 baseScale;
    private SpriteRenderer sr;
    private float currentDiameter; // Renamed for clarity
    private bool initialized;

    public void Init(float dmg, LayerMask enemyMask, float expandSpeed, float maxRadius, float spinSpeed, Color color)
    {
        this.damage      = dmg;
        this.enemyMask   = enemyMask;
        this.expandSpeed = expandSpeed;
        this.maxRadius   = Mathf.Max(0.01f, maxRadius); // maxRadius is the collider's final radius
        this.spinSpeed   = spinSpeed;
        this.ringColor   = color;
        
        // This is the key calculation. We set the collider's LOCAL radius to be a fixed,
        // pre-corrected value. If the visual ratio is 2, the local radius becomes 0.5.
        // When the parent scales up, this smaller local radius will expand to the correct world size.
        col.radius = 1f / visualToColliderRatio;

        if (sr)
        {
            var c = ringColor; c.a = startAlpha;
            sr.color = c;
        }

        initialized = true;
    }

    void Awake()
    {
        col = GetComponent<SphereCollider>();
        col.isTrigger = true;
        
        // The starting radius should also be pre-corrected.
        col.radius = 1f / visualToColliderRatio;

        baseScale = transform.localScale;

        sr = GetComponentInChildren<SpriteRenderer>();
        if (sr != null)
        {
            var c = ringColor;
            c.a = startAlpha;
            sr.color = c;
            sr.sortingOrder = Mathf.Max(sr.sortingOrder, 50);
        }
    }

    void Update()
    {
        if (!initialized) return;

        // Spin the ring
        if (Mathf.Abs(spinSpeed) > 0.01f)
            transform.Rotate(0f, spinSpeed * Time.deltaTime, 0f, Space.World);

        // We will now expand a "diameter" value up to the maxRadius * 2
        float maxDiameter = maxRadius * 2f;

        // Expand outwards
        if (currentDiameter < maxDiameter)
        {
            currentDiameter = Mathf.Min(maxDiameter, currentDiameter + (expandSpeed * 2f) * Time.deltaTime);

            // 1. The parent's scale is now driven by the desired VISUAL size.
            // Assuming baseScale is (1,1,1), the final localScale will be (maxDiameter, maxDiameter, maxDiameter).
            transform.localScale = baseScale * currentDiameter * (visualToColliderRatio / 2f);
            
            // NOTE: The collider's radius is NOT changed here anymore. It was set once in Init/Awake.
            // As the parent object scales up, the fixed local radius of the collider expands with it,
            // reaching the perfect world-space radius at the end.

            // Fade alpha based on expansion
            float t = Mathf.InverseLerp(0f, maxDiameter, currentDiameter);
            if (sr != null)
            {
                Color c = ringColor;
                c.a = Mathf.Lerp(startAlpha, endAlpha, t);
                sr.color = c;
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // Apply damage if enemy
        EnemyHealth health = other.GetComponent<EnemyHealth>();
        EnemyPathAgent agent = other.GetComponent<EnemyPathAgent>();
        EnemySpawner manager = agent ? agent.GetComponentInParent<EnemySpawner>() : null;
        if (health != null)
        {
            health.TakeDamage(GetComponentInParent<TowerUpgrade>().CurrentDamage, manager, agent); // or pass damage from turret
        }
    }
}