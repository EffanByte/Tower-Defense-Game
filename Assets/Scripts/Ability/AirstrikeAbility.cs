using UnityEngine;
using UnityEngine.Tilemaps;

public class AirstrikeAbility : MonoBehaviour
{
    [Header("Airstrike Settings")]
    [SerializeField] private Camera cam;
    [SerializeField] private float bombRadius = 3f;
    [SerializeField] private int bombDamage = 100;
    [SerializeField] private LayerMask enemyMask;

    [Header("Visuals")]
    [SerializeField] private int circleSegments = 64;
    [SerializeField] private float lineWidth = 0.05f;
    [SerializeField] private Color lineColor = Color.red;

    [SerializeField] private GameObject explosionPrefab;

    private InputRouter router;
    private bool targeting = false;
    private Vector3 currentPos;
    private LineRenderer rangeCircle;

    void Awake()
    {
        if (!cam) cam = Camera.main;
        router = FindObjectOfType<InputRouter>();

        // Create the LineRenderer entirely from code
        GameObject lrObj = new GameObject("AirstrikeRangeCircle");
        lrObj.transform.SetParent(transform, false);

        rangeCircle = lrObj.AddComponent<LineRenderer>();
        rangeCircle.loop = true;
        rangeCircle.useWorldSpace = true;
        rangeCircle.widthMultiplier = lineWidth;
        rangeCircle.material = new Material(Shader.Find("Sprites/Default"));
        rangeCircle.startColor = lineColor;
        rangeCircle.endColor = lineColor;
        rangeCircle.enabled = false;
    }

    void OnEnable()
    {
        if (router != null)
        {
            router.OnBuildableTileClicked += HandleWorldClick;
            router.OnUIClicked += CancelTargeting;
        }
    }

    void OnDisable()
    {
        if (router != null)
        {
            router.OnBuildableTileClicked -= HandleWorldClick;
            router.OnUIClicked -= CancelTargeting;
        }
    }

    void Update()
    {
        if (!targeting || !rangeCircle) return;

        Vector2 screen = router.pointerAction.ReadValue<Vector2>();
        Ray ray = cam.ScreenPointToRay(screen);

        if (Physics.Raycast(ray, out var hit, 500f, LayerMask.GetMask("Buildable", "Ground")))
        {
            currentPos = hit.point;
            DrawCircle(currentPos, bombRadius);
            rangeCircle.enabled = true;
        }
    }

    // Called by UI Button
    public void StartTargeting()
    {
        targeting = true;
        Debug.Log("[Airstrike] Enter targeting mode.");
    }

    void HandleWorldClick(RaycastHit hit, Tilemap tm, Vector3Int cell)
    {
        if (!targeting) return;
        DoAirstrike(currentPos);
        Debug.Log("[Airstrike] Airstrike at " + currentPos);
        CancelTargeting();
    }


    void CancelTargeting()
    {
        targeting = false;
        if (rangeCircle) rangeCircle.enabled = false;
        Debug.Log("[Airstrike] Cancelled targeting.");
    }

    void DoAirstrike(Vector3 center)
    {
        Collider[] hits = Physics.OverlapSphere(center, bombRadius, enemyMask);
        foreach (var h in hits)
        {
            var health = h.GetComponent<EnemyHealth>();
            var agent = h.GetComponent<EnemyPathAgent>();
            var manager = agent ? agent.GetComponentInParent<EnemySpawner>() : null;

            if (health)
                health.TakeDamage(bombDamage, manager, agent);
        }

        // 🔥 Instantiate explosion visual
        GameObject explosion = Instantiate(explosionPrefab, center, Quaternion.Euler(90,0,0));
        explosion.transform.localScale = Vector3.one * bombRadius * 2f; // scale to diameter
        Destroy(explosion, 0.83f); // cleanup after animation
        Debug.Log($"[Airstrike] Bomb dropped at {center}, hits={hits.Length}");
    }

    void DrawCircle(Vector3 center, float radius)
    {
        rangeCircle.positionCount = circleSegments;
        for (int i = 0; i < circleSegments; i++)
        {
            float angle = (i / (float)circleSegments) * Mathf.PI * 2f;
            Vector3 pos = center + new Vector3(Mathf.Cos(angle) * radius, 0.1f, Mathf.Sin(angle) * radius);
            rangeCircle.SetPosition(i, pos);
        }
    }
}
