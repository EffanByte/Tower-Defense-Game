using UnityEngine;
using UnityEngine.Tilemaps;

public class TowerBuildController : MonoBehaviour
{
    [Header("Wire up in Inspector")]
    [SerializeField] private InputRouter inputRouter;
    [SerializeField] private Camera cam;
    [SerializeField] private Tilemap buildableTilemap; // fallback
    [SerializeField] private TDLevel level;
    [SerializeField] private GameObject[] towerPrefabs;

    [Header("Placement")]
    [SerializeField] private float placeY = 0.1f;
    [SerializeField] private Vector2Int footprint = new Vector2Int(1, 1);
    [SerializeField] private bool tileYInvertedForTDLevel = false;
    [SerializeField] private int minManhattanFromRoad = 0;

    public int selected = -1; // set from UI (1-4 etc.)


    void OnEnable()
    {
        if (inputRouter != null)
        {
            inputRouter.OnBuildableTileClicked += HandleBuildableTileClicked;
        }
    }

    void OnDisable()
    {
        if (inputRouter != null)
            inputRouter.OnBuildableTileClicked -= HandleBuildableTileClicked;
    }

    private void HandleBuildableTileClicked(RaycastHit hit, Tilemap tm, Vector3Int cell)
    {
        // 1) Make sure this tile exists
            if (!tm.HasTile(cell))
            {
                Debug.Log($"[TowerBuildController] No tile at {cell} → not placeable.");
                return;
            }

        // 2) Convert to logical coords for TDLevel checks
        Vector2Int logical = new Vector2Int(cell.x, tileYInvertedForTDLevel ? -cell.y : cell.y);
        bool canPlace = level ? level.CanPlace(logical, footprint, minManhattanFromRoad) : true;
        if (!canPlace) return;

        // 3) Compute spawn position
        Vector3 spawn = tm.GetCellCenterWorld(cell) + new Vector3(-0.5f, 0f, 0f);
        spawn.y = placeY;

        // 4) Occupancy check using Tower layer overlap
        int towerMask = LayerMask.GetMask("Tower");
        if (towerMask == 0) Debug.LogWarning("[TowerBuildController] 'Tower' layer mask is 0.");
        float checkRadius = Mathf.Max(tm.cellSize.x, tm.cellSize.y) * 0.35f;
        var overlaps = Physics.OverlapSphere(spawn, checkRadius, towerMask, QueryTriggerInteraction.Collide);
        if (overlaps.Length > 0)
        {
            selected = -1; // clear queued placement on occupied cell
            return;
        }

        // 5) Only now require a selection
        if (selected < 0)
        {
            Debug.Log("[TowerBuildController] Empty cell clicked but no tower selected → no placement.");
            return;
        }

        // 6) Place the tower
        var prefab = (selected >= 0 && selected < towerPrefabs.Length) ? towerPrefabs[selected] : null;
        if (!prefab)
            return;

        Instantiate(prefab, spawn, Quaternion.identity);

        selected = -1; // clear selection after place
        if (level)
        {
            level.SetOccupied(logical, footprint, true);
            Debug.Log($"[TowerBuildController] Marked logical {logical} as Occupied.");
        }
    }

    public void SelectTower(int index)
    {
        if (index < 0 || index >= towerPrefabs.Length)
        {
            Debug.LogWarning($"[TowerBuildController] Invalid tower index {index}");
            selected = -1;
            return;
        }

        selected = index;
    }
}
