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
    [SerializeField] private float maxRayDistance = 200f;

    [Header("Input (assign refs or leave blank)")]
    [Tooltip("Optional: Action Reference to your pointer position action")]
    [SerializeField] private InputActionReference pointerRef;
    [Tooltip("Optional: Action Reference to your click action")]
    [SerializeField] private InputActionReference clickRef;

    // if you don't use refs, you can keep these serialized or let the script create fallbacks
    public InputAction pointerAction; // "<Pointer>/position"
    public InputAction clickAction;   // "<Mouse>/leftButton"

    public System.Action<RaycastHit, Tilemap, Vector3Int> OnBuildableTileClicked;

    void Awake()
    {
        if (!cam) cam = Camera.main;
        Debug.Log($"[InputRouter] Awake. cam={(cam ? cam.name : "<null>")} buildableMask={buildableMask.value}");

        // Wire actions from references if provided
        if (pointerRef != null && pointerRef.action != null) pointerAction = pointerRef.action;
        if (clickRef != null && clickRef.action != null) clickAction = clickRef.action;

        // Create sensible fallbacks if still missing
        if (pointerAction == null)
        {
            pointerAction = new InputAction("PointerPos", InputActionType.Value, "<Pointer>/position");
            Debug.Log("[InputRouter] Created fallback pointerAction '<Pointer>/position'.");
        }
        if (clickAction == null)
        {
            clickAction = new InputAction("Click", InputActionType.Button, "<Mouse>/leftButton");
            Debug.Log("[InputRouter] Created fallback clickAction '<Mouse>/leftButton'.");
        }
    }

    void OnEnable()
    {
        Debug.Log("[InputRouter] OnEnable → enabling actions and subscribing.");
        try
        {
            pointerAction.Enable();
            clickAction.Enable();
            clickAction.performed += HandleClick;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[InputRouter] Failed to enable actions: {e}");
        }
    }

    void OnDisable()
    {
        Debug.Log("[InputRouter] OnDisable → unsubscribing and disabling actions.");
        clickAction.performed -= HandleClick;
        pointerAction.Disable();
        clickAction.Disable();
    }

    void HandleClick(InputAction.CallbackContext ctx)
    {
        Debug.Log($"[InputRouter] HandleClick(performed). time={Time.time:F3}");

        // 0) UI consumes clicks?
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            Debug.Log("[InputRouter] UI raycast hit → routing to UI only, ignoring world.");
            return;
        }

        if (!cam)
        {
            Debug.LogError("[InputRouter] No camera set; cannot raycast.");
            return;
        }

        Vector2 screen = Vector2.zero;
        if (pointerAction.enabled)
            screen = pointerAction.ReadValue<Vector2>();
        else if (Mouse.current != null)
            screen = Mouse.current.position.ReadValue();

        Debug.Log($"[InputRouter] Screen pos {screen}");

        var ray = cam.ScreenPointToRay(screen);
        Debug.Log($"[InputRouter] Ray origin={ray.origin:F3} dir={ray.direction:F3} mask={buildableMask.value}");

        // Raycast against Buildable
        if (!Physics.Raycast(ray, out var hit, maxRayDistance, buildableMask, QueryTriggerInteraction.Ignore))
        {
            Debug.Log("[InputRouter] No Buildable hit.");
            return;
        }

        var tm = hit.collider ? hit.collider.GetComponentInParent<Tilemap>() : null;
        if (!tm)
        {
            Debug.LogWarning($"[InputRouter] Hit '{hit.collider?.name ?? "<null>"}' but no Tilemap on parent.");
            return;
        }

        // nudge inward to avoid edge ambiguity
        var nudged = hit.point - hit.normal * 0.001f;
        Vector3Int cell = tm.WorldToCell(nudged);
        Debug.Log($"[InputRouter] Buildable HIT '{tm.name}' at world={hit.point:F3}, cell={cell}, tile={(tm.HasTile(cell) ? (tm.GetTile(cell)?.name ?? "<tile>") : "<none>")}");

        OnBuildableTileClicked?.Invoke(hit, tm, cell);
    }

    // Safety net: if your actions never fire, this will still log a click so you see *something*
    void Update()
    {
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            Debug.Log("[InputRouter] (Fallback) Mouse leftButton pressed this frame.");
        }
    }
}
