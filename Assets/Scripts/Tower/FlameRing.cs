using UnityEngine;

public class FlameRing : MonoBehaviour
{
    [SerializeField] private float expandSpeed = 5f;
    [SerializeField] private float maxRadius = 4f;
    private float startAlpha = 1f;  // fully visible
    private float endAlpha = 0f;    // fully transparent

    private SphereCollider col;
    private Vector3 startScale;
    private SpriteRenderer sr;
    private float currentRadius;

    void Awake()
    {
        maxRadius = GetComponentInParent<TowerUpgrade>().CurrentRange;
        col = GetComponent<SphereCollider>();
        col.isTrigger = true;
        col.radius = 0.1f;
        startScale = transform.localScale;

        sr = GetComponentInChildren<SpriteRenderer>();
        if (sr != null)
        {
            Color c = sr.color;
            c.a = startAlpha;
            sr.color = c;
        }
    }

    void Update()
    {
        if (col.radius < maxRadius)
        {
            col.radius += expandSpeed * Time.deltaTime;
            currentRadius = col.radius;

            float scale = col.radius * 2f; // diameter
            transform.localScale = startScale * scale;

            // Fade alpha based on expansion progress
            float t = currentRadius / maxRadius; // 0 → 1
            float alpha = Mathf.Lerp(startAlpha, endAlpha, t);

            if (sr != null)
            {
                Color c = sr.color;
                c.a = alpha;
                sr.color = c;
            }
        }
    }

    // void OnTriggerEnter(Collider other)
    // {
    //     // Apply damage if enemy
    //     EnemyHealth health = other.GetComponent<EnemyHealth>();
    //     EnemyPathAgent agent = other.GetComponent<EnemyPathAgent>();
    //     EnemySpawner manager = agent ? agent.GetComponentInParent<EnemySpawner>() : null;
    //     if (health != null)
    //     {
    //         health.TakeDamage(GetComponentInParent<TowerUpgrade>().CurrentDamage, manager, agent); // or pass damage from turret
    //     }
    // }
}
