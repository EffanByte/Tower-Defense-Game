// TowerBuildController.cs  (clean version)
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

public class TowerBuildController : MonoBehaviour
{
    [Header("Refs")]
    public Camera cam;                         // if null, uses Camera.main
    public Tilemap buildableTilemap;           // used to convert world->cell & read tile name
    public TDLevel level;                      // optional: for CanPlace + SetOccupied

    [Header("Towers (press 1–4)")]
    public GameObject[] towerPrefabs = new GameObject[4];

    [Header("3D Raycast")]
    public LayerMask buildableMask;            // set to include your Buildable layer (with 3D collider)
    public float maxRayDistance = 500f;

    [Header("Placement")]
    public float placeY = 0.1f;                // requested fixed Y
    public Vector2Int footprint = new Vector2Int(1, 1);
    public int minManhattanFromRoad = 0;

    [Header("TDLevel mapping")]
    // TRUE if your TDLevel painted CSV with y inverted (CSV y -> Tilemap -y). Leave true if using my painter.
    public bool tileYInvertedForTDLevel = true;

    // --- Input System actions ---
    private InputAction clickAction;           // <Mouse>/leftButton
    private InputAction pointerAction;         // <Pointer>/position
    private InputAction sel1, sel2, sel3, sel4;

    private int selected = -1;

    void OnEnable()
    {
        if (!cam) cam = Camera.main;

        clickAction = new InputAction("Click", InputActionType.Button, "<Mouse>/leftButton");
        pointerAction = new InputAction("Pointer", InputActionType.PassThrough, "<Pointer>/position");
        clickAction.performed += OnClick;

        sel1 = MakeSelect("<Keyboard>/1", "<Keyboard>/numpad1", 0);
        sel2 = MakeSelect("<Keyboard>/2", "<Keyboard>/numpad2", 1);
        sel3 = MakeSelect("<Keyboard>/3", "<Keyboard>/numpad3", 2);
        sel4 = MakeSelect("<Keyboard>/4", "<Keyboard>/numpad4", 3);

        clickAction.Enable();
        pointerAction.Enable();
        sel1.Enable(); sel2.Enable(); sel3.Enable(); sel4.Enable();

        Debug.Log($"[TowerBuildController] Enabled. buildableMask={buildableMask.value}");
    }

    void OnDisable()
    {
        clickAction?.Disable();
        pointerAction?.Disable();
        sel1?.Disable(); sel2?.Disable(); sel3?.Disable(); sel4?.Disable();
    }

    private InputAction MakeSelect(string a, string b, int index)
    {
        var act = new InputAction($"Select{index + 1}", InputActionType.Button);
        act.AddBinding(a); act.AddBinding(b);
        act.performed += _ => Select(index);
        return act;
    }

    private void Select(int idx)
    {
        var ok = idx >= 0 && idx < towerPrefabs.Length && towerPrefabs[idx] != null;
        selected = ok ? idx : -1;
        Debug.Log(ok
            ? $"[TowerBuildController] Selected {idx + 1}: {towerPrefabs[idx].name}"
            : $"[TowerBuildController] Selection {idx + 1} invalid (no prefab).");
    }

    private void OnClick(InputAction.CallbackContext _)
    {
        if (selected < 0) { Debug.Log("[TowerBuildController] Click ignored: no tower selected."); return; }
        if (!cam) { Debug.Log("[TowerBuildController] Click ignored: no camera."); return; }

        // 3D raycast to the Buildable layer
        Vector2 screen = pointerAction.ReadValue<Vector2>();
        Ray ray = cam.ScreenPointToRay(screen);

        if (!Physics.Raycast(ray, out var hit, maxRayDistance, buildableMask, QueryTriggerInteraction.Ignore))
        {
            Debug.Log("[TowerBuildController] Raycast MISS on Buildable mask.");
            return;
        }

        // Prefer a Tilemap on/above the collider, else fall back to the assigned one
        Tilemap tm = hit.collider ? hit.collider.GetComponentInParent<Tilemap>() : null;
        if (!tm) tm = buildableTilemap;
        if (!tm)
        {
            Debug.Log($"[TowerBuildController] Hit '{hit.collider?.name ?? "<null>"}' but no Tilemap found. Assign buildableTilemap.");
            return;
        }

        // World → cell (nudge inside to avoid border ambiguity)
        Vector3 nudged = hit.point - hit.normal * 0.001f;
        Vector3Int cell = tm.WorldToCell(nudged);
        TileBase tile = tm.GetTile(cell);

        Debug.Log($"[TowerBuildController] Hit Tilemap='{tm.name}' at world {hit.point:F3} → nudged {nudged:F3} → cell {cell}, tile={(tile ? tile.name : "<none>")}");

        if (!tm.HasTile(cell))
        {
            Debug.Log("[TowerBuildController] No tile at this cell → not placeable.");
            return;
        }

        // Map Tilemap cell → TDLevel logical cell (CSV coords)
        Vector2Int logical = new Vector2Int(cell.x, tileYInvertedForTDLevel ? -cell.y : cell.y);

        bool canPlace = true;
        if (level)
        {
            if (!level.InBounds(logical))
            {
                Debug.Log($"[TowerBuildController] NOT placeable ❌ (out of TD grid bounds) logical={logical}");
                canPlace = false;
            }
            else
            {
                canPlace = level.CanPlace(logical, footprint, minManhattanFromRoad);
                Debug.Log($"[TowerBuildController] CanPlace({logical}, footprint={footprint}, minRoad={minManhattanFromRoad}) => {canPlace}");
            }
        }
        else
        {
            Debug.Log("[TowerBuildController] No TDLevel assigned; skipping validation and placing anyway.");
        }

        if (!canPlace) return;

        // Place at cell center, force Y
        var prefab = towerPrefabs[selected];
        if (!prefab) { Debug.Log("[TowerBuildController] Selected prefab is null."); return; }

        Vector3 spawn = tm.GetCellCenterWorld(cell);
        spawn.y = placeY;

        Instantiate(prefab, spawn, Quaternion.identity);
        Debug.Log($"[TowerBuildController] Spawned '{prefab.name}' at {spawn} (cell {cell}, logical {logical})");

        if (level) level.SetOccupied(logical, footprint, true);
    }
}
