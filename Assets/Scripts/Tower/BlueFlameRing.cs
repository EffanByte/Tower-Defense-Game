using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class BlueFlameRing : MonoBehaviour
{
    [Header("Runtime (filled by Init)")]
    private float expandSpeed = 5f;
    private float maxRadius   = 4f;
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
    private float currentRadius;
    private bool initialized;

    public void Init(float dmg, LayerMask enemyMask, float expandSpeed, float maxRadius, float spinSpeed, Color color)
    {
        this.damage      = dmg;
        this.enemyMask   = enemyMask;
        this.expandSpeed = expandSpeed;
        this.maxRadius   = Mathf.Max(0.01f, maxRadius);
        this.spinSpeed   = spinSpeed;
        this.ringColor   = color;

        // apply color right away if SR exists already
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
        col.radius = 0.1f;

        baseScale = transform.localScale;

        sr = GetComponentInChildren<SpriteRenderer>();
        if (sr != null)
        {
            var c = ringColor; // default blue-ish
            c.a = startAlpha;
            sr.color = c;
            // make sure it renders above ground stuff if needed
            sr.sortingOrder = Mathf.Max(sr.sortingOrder, 50);
        }
    }

    void Update()
    {
        if (!initialized) return;

        // Spin the ring
        if (Mathf.Abs(spinSpeed) > 0.01f)
            transform.Rotate(0f, spinSpeed * Time.deltaTime, 0f, Space.World);

        // Expand outwards
        if (currentRadius < maxRadius)
        {
            currentRadius = Mathf.Min(maxRadius, currentRadius + expandSpeed * Time.deltaTime);
            col.radius = currentRadius;

            // Scale ring (diameter)
            float scale = currentRadius * 2f;
            transform.localScale = baseScale * scale;
            
            // Fade alpha based on expansion
            float t = Mathf.InverseLerp(0f, maxRadius, currentRadius);
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
