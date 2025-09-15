using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI; // Required for the UI Text element
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

    [Header("UI")]
    [SerializeField] private Text airstrikeCountText; // Assign a UI Text element here

    // --- NEW VARIABLES FOR SAVING/LOADING ---
    private const string AirstrikeCountKey = "PlayerAirstrikeCount";
    private int _airstrikeCount;
    // ------------------------------------------

    private InputRouter router;
    private bool targeting = false;
    private Vector3 currentPos;
    private LineRenderer rangeCircle;

    void Awake()
    {
        if (!cam) cam = Camera.main;
        router = FindObjectOfType<InputRouter>();

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

    void Start()
    {
        // Load the saved airstrike count when the game starts
        _airstrikeCount = PlayerPrefs.GetInt(AirstrikeCountKey, 0);
        UpdateUI();
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
    
    // Called by UI Button
    public void StartTargeting()
    {
        ResumeGame();
        // Check if the player has any airstrikes left
        if (_airstrikeCount <= 0)
        {
            Debug.Log("[Airstrike] No airstrikes available!");
            return;
        }
        targeting = true;
    }

    void HandleWorldClick(RaycastHit hit, Tilemap tm, Vector3Int cell)
    {
        if (!targeting) return;
        
        // Use an airstrike and save the new count
        UseAirstrike(currentPos);
        
        CancelTargeting(); // This will disable the circle and resume the game
    }
    
    void CancelTargeting()
    {
        if (!targeting) return; // Prevent this from running multiple times
        
        targeting = false;
        if (rangeCircle) rangeCircle.enabled = false;
        ResumeGame(); // Resume the game
        Debug.Log("[Airstrike] Cancelled targeting.");
    }
    
    // This new method handles the logic of using an airstrike
    void UseAirstrike(Vector3 center)
    {
        // --- DECREMENT AND SAVE LOGIC ---
        _airstrikeCount--;
        PlayerPrefs.SetInt(AirstrikeCountKey, _airstrikeCount);
        PlayerPrefs.Save();
        UpdateUI();
        Time.timeScale = 1.0f;
        DoAirstrike(center);
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

        GameObject explosion = Instantiate(explosionPrefab, center, Quaternion.Euler(90,0,0));
        explosion.transform.localScale = Vector3.one * bombRadius * 2f;
        Destroy(explosion, 0.83f);
        Debug.Log($"[Airstrike] Bomb dropped at {center}, hits={hits.Length}. {_airstrikeCount} remaining.");
    }
    
    // --- UTILITY METHODS ---

    public void PauseWhileOrdering()
    {
        Time.timeScale = 0.0f; 
    }
    
    public void ResumeGame()
    {
        Time.timeScale = 1.0f;
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
    
    // New method to keep the UI up-to-date
    void UpdateUI()
    {
        if (airstrikeCountText != null)
        {
            airstrikeCountText.text = "x " + _airstrikeCount;
        }
    }
}       