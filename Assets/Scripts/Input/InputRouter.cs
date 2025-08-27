using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

public class InputRouter : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Camera cam;

    [Header("Masks")]
    [SerializeField] private LayerMask buildableMask;
    [SerializeField] private LayerMask towerMask;
    [SerializeField] private float maxRayDistance = 200f;

    [Header("Input (assign refs or leave blank)")]
    [Tooltip("Optional: Action Reference to your pointer position action")]
    [SerializeField] private InputActionReference pointerRef;
    [Tooltip("Optional: Action Reference to your click action")]
    [SerializeField] private InputActionReference clickRef;

    // If you don't use refs, you can keep these serialized or let the script create fallbacks
    public InputAction pointerAction; // "<Pointer>/position"
    public InputAction clickAction;   // "<Mouse>/leftButton"

    // Events that other scripts subscribe to
    public System.Action OnUIClicked;
    public System.Action<GameObject> OnTowerClicked;
    public System.Action<RaycastHit, Tilemap, Vector3Int> OnBuildableTileClicked;

    public System.Action OnNonTowerClicked;

    void Awake()
    {
        if (!cam) cam = Camera.main;

        // Wire actions from references if provided
        if (pointerRef != null && pointerRef.action != null) pointerAction = pointerRef.action;
        if (clickRef != null && clickRef.action != null) clickAction = clickRef.action;

        // Create sensible fallbacks if still missing
        if (pointerAction == null)
            pointerAction = new InputAction("PointerPos", InputActionType.Value, "<Pointer>/position");
        if (clickAction == null)
            clickAction = new InputAction("Click", InputActionType.Button, "<Mouse>/leftButton");
    }

    void OnEnable()
    {
        pointerAction.Enable();
        clickAction.Enable();
        clickAction.performed += HandleClick;
    }

    void OnDisable()
    {
        clickAction.performed -= HandleClick;
        pointerAction.Disable();
        clickAction.Disable();
    }

    void HandleClick(InputAction.CallbackContext ctx)
    {

        
        // 0) UI consumes clicks?
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            Debug.Log("[InputRouter] UI clicked.");
            OnUIClicked?.Invoke();
            return;
        }

        if (!cam)
        {
            Debug.LogWarning("[InputRouter] No camera found for click raycast.");
            return;
        }

        Vector2 screen = pointerAction.enabled
            ? pointerAction.ReadValue<Vector2>()
            : (Mouse.current != null ? Mouse.current.position.ReadValue() : Vector2.zero);

        Ray ray = cam.ScreenPointToRay(screen);
        Debug.DrawRay(ray.origin, ray.direction * maxRayDistance, Color.red, 1f);

        // Cast against everything
        var hits = Physics.RaycastAll(ray, maxRayDistance, ~0, QueryTriggerInteraction.Collide);
        if (hits.Length == 0)
        {
            Debug.Log("[InputRouter] Click hit nothing.");
            return;
        }

        // Sort by distance so closest is first
        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        // Look for towers first
        foreach (var h in hits)
        {
            if (((1 << h.collider.gameObject.layer) & towerMask) != 0)
            {
                OnTowerClicked?.Invoke(h.collider.gameObject);
                return;
            }
        }

        OnNonTowerClicked?.Invoke();
        
        // Then buildable tiles
        foreach (var h in hits)
        {
            if (((1 << h.collider.gameObject.layer) & buildableMask) != 0)
            {
                var tm = h.collider.GetComponentInParent<Tilemap>();
                if (tm != null)
                {
                    var nudged = h.point - h.normal * 0.001f;
                    Vector3Int cell = tm.WorldToCell(nudged);
                    OnBuildableTileClicked?.Invoke(h, tm, cell);
                    return;
                }
            }
        }

        Debug.Log("[InputRouter] Click did not hit tower or buildable.");
    }

}
