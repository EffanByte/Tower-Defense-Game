// TowerBuildController.cs (UI-selectable version)
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;

public class TowerBuildController : MonoBehaviour
{
    [Header("Refs")]
    public Camera cam;                         // if null, uses Camera.main
    public Tilemap buildableTilemap;           // used for WorldToCell + cell center
    public TDLevel level;                      // optional: for CanPlace/SetOccupied

    [Header("Towers (UI will call SelectByUI)")]
    public GameObject[] towerPrefabs = new GameObject[4];

    [Header("3D Raycast")]
    public LayerMask buildableMask;            // include your Buildable layer
    public float maxRayDistance = 500f;

    [Header("Placement")]
    public float placeY = 0.1f;
    public Vector2Int footprint = new Vector2Int(1, 1);
    public int minManhattanFromRoad = 0;

    [Header("TDLevel mapping")]
    public bool tileYInvertedForTDLevel = true; // true if CSV painted to (x, -y)

    // Input System (click + pointer only)
    private InputAction clickAction;           // <Mouse>/leftButton
    private InputAction pointerAction;         // <Pointer>/position

    private int selected = -1;                 // set by UI

    void OnEnable()
    {
        if (!cam) cam = Camera.main;

        clickAction = new InputAction("Click", InputActionType.Button, "<Mouse>/leftButton");
        pointerAction = new InputAction("Pointer", InputActionType.PassThrough, "<Pointer>/position");
        clickAction.performed += OnClick;

        clickAction.Enable();
        pointerAction.Enable();

        Debug.Log($"[TowerBuildController] Enabled. buildableMask={buildableMask.value}");
    }

    void OnDisable()
    {
        clickAction?.Disable();
        pointerAction?.Disable();
    }

    // -------- UI hooks --------
    // Call this from your UI Button's OnClick with argument 0..3
    public void SelectByUI(int idx)
    {
        if (idx >= 0 && idx < towerPrefabs.Length && towerPrefabs[idx] != null)
        {
            selected = idx;
            Debug.Log($"[TowerBuildController] UI selected {idx + 1}: {towerPrefabs[idx].name}");
        }
        else
        {
            selected = -1;
            Debug.Log($"[TowerBuildController] UI selection {idx + 1} invalid (no prefab).");
        }
    }

    // Optional: let a "Cancel" button call this
    public void ClearSelection()
    {
        selected = -1;
        Debug.Log("[TowerBuildController] Selection cleared.");
    }

    // -------- World click -> place --------
    private void OnClick(InputAction.CallbackContext _)
    {
        // Ignore if pointer is over UI
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            // We just clicked a UI element (e.g., one of your pictures)
            return;
        }

        if (selected < 0) { Debug.Log("[TowerBuildController] Click ignored: no tower selected."); return; }
        if (!cam) { Debug.Log("[TowerBuildController] Click ignored: no camera."); return; }

        Vector2 screen = pointerAction.ReadValue<Vector2>();
        Ray ray = cam.ScreenPointToRay(screen);

        if (!Physics.Raycast(ray, out var hit, maxRayDistance, buildableMask, QueryTriggerInteraction.Ignore))
        {
            Debug.Log("[TowerBuildController] Raycast MISS on Buildable mask.");
            return;
        }

        // Get tilemap (prefer from hit; else use assigned reference)
        Tilemap tm = hit.collider ? hit.collider.GetComponentInParent<Tilemap>() : null;
        if (!tm) tm = buildableTilemap;
        if (!tm)
        {
            Debug.Log($"[TowerBuildController] Hit '{hit.collider?.name ?? "<null>"}' but no Tilemap found. Assign buildableTilemap.");
            return;
        }

        // World → cell (nudge inside)
        Vector3 nudged = hit.point - hit.normal * 0.001f;
        Vector3Int cell = tm.WorldToCell(nudged);

        if (!tm.HasTile(cell))
        {
            Debug.Log($"[TowerBuildController] No tile at cell {cell} → not placeable.");
            return;
        }

        // Convert to TD grid coords if using TDLevel
        Vector2Int logical = new Vector2Int(cell.x, tileYInvertedForTDLevel ? -cell.y : cell.y);

        bool canPlace = true;
        if (level)
        {
            canPlace = level.CanPlace(logical, footprint, minManhattanFromRoad);
            Debug.Log($"[TowerBuildController] CanPlace({logical}, footprint={footprint}, minRoad={minManhattanFromRoad}) => {canPlace}");
        }

        if (!canPlace) return;

        // Place
        var prefab = towerPrefabs[selected];
        if (!prefab) { Debug.Log("[TowerBuildController] Selected prefab is null."); return; }

        Vector3 spawn = tm.GetCellCenterWorld(cell) + new Vector3(-0.5f,0,0); // Add vector so tower is centered
        spawn.y = placeY;

        Instantiate(prefab, spawn, Quaternion.identity);
        Debug.Log($"[TowerBuildController] Spawned '{prefab.name}' at {spawn} (cell {cell}, logical {logical})");

        if (level) level.SetOccupied(logical, footprint, true);
    }
}
