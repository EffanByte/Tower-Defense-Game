using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TDLevel : MonoBehaviour
{
    [Header("CSV Assets (assign the files I gave you)")]
    public TextAsset roadCsv;        // road.csv    (H rows x W columns; 1=road)
    public TextAsset buildableCsv;   // buildable.csv (1=buildable, 0=blocked)

    [Header("Tilemaps & Tiles")]
    public Grid grid;
    public Tilemap roadTilemap;
    public Tilemap buildableTilemap;
    public TileBase roadTile;
    public TileBase buildableTile;
    public TileBase blockedTile;     // optional; painted where buildable=0 and road=0

    [Header("Map Size (CSV must match)")]
    public int width = 32;
    public int height = 18;

    [Header("Spawners & Exit (defaults match the two-lane map)")]
    public Vector2Int[] spawners = { new Vector2Int(0, 6), new Vector2Int(0, 11) };
    public Vector2Int exitCell = new Vector2Int(31, 10);

    [Header("Options")]
    public bool paintOnStart = true;
    public bool paintBlocked = true;   // paint non-buildable background with blockedTile
    public int roadNeighborMode = 4;   // 4-neighbors for grid roads

    // Runtime grids
    private bool[,] Road;       // [y,x]
    private bool[,] Buildable;  // [y,x]
    private bool[,] Occupied;   // [y,x] runtime tower occupancy

    // Cache baked waypoint paths per spawner index
    private readonly Dictionary<int, List<Vector2Int>> _bakedPaths = new();

    void Awake()
    {
        if (roadCsv == null || buildableCsv == null)
        {
            Debug.LogError("Assign road.csv and buildable.csv TextAssets.");
            return;
        }

        Road = ParseCsvBool(roadCsv, width, height, "road");
        Buildable = ParseCsvBool(buildableCsv, width, height, "buildable");
        Occupied = new bool[height, width];

        // Make sure road cells are not buildable (defensive)
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
                if (Road[y, x]) Buildable[y, x] = false;

        if (paintOnStart) PaintTilemaps();

        // Bake paths once (roads are static)
        for (int i = 0; i < spawners.Length; i++)
        {
            var path = BakeRoadPath(spawners[i], exitCell);
            if (path == null || path.Count == 0)
                Debug.LogError($"No road path from spawner {i} at {spawners[i]} to exit {exitCell}.");
            else
                _bakedPaths[i] = path; // sets baked path
        }
        // Paths are being added here, but maybe this script runs after baked paths are set
    }

    #region CSV / Painting

    private static bool[,] ParseCsvBool(TextAsset csv, int expectedW, int expectedH, string label)
    {
        var lines = csv.text.Replace("\r", "").Trim().Split('\n');
        if (lines.Length != expectedH)
            Debug.LogWarning($"{label}.csv rows={lines.Length} but expected {expectedH}.");
        int H = Mathf.Min(expectedH, lines.Length);

        var grid = new bool[expectedH, expectedW];
        for (int y = 0; y < H; y++)
        {
            var parts = lines[y].Split(',');
            if (parts.Length != expectedW)
                Debug.LogWarning($"{label}.csv row {y} cols={parts.Length} but expected {expectedW}.");
            int W = Mathf.Min(expectedW, parts.Length);
            for (int x = 0; x < W; x++)
            {
                // Accept 0/1 or truthy strings
                var s = parts[x].Trim();
                int iv;
                bool bv = int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out iv) ? iv != 0 : s == "true" || s == "True";
                grid[y, x] = bv;
            }
        }
        return grid;
    }

    private void PaintTilemaps()
    {
        if (grid == null) grid = GetComponent<Grid>();
        if (roadTilemap == null || buildableTilemap == null)
        {
            Debug.LogError("Assign roadTilemap and buildableTilemap.");
            return;
        }

        roadTilemap.ClearAllTiles();
        buildableTilemap.ClearAllTiles();

        // Coordinate convention:
        // CSV (0,0) is top-left; Unity Tilemap y increases upward.
        // We map CSV (x, y) -> Tilemap (x, -y) so row 0 is at y=0 and grows downward.
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                var cell = new Vector3Int(x, -y, 0);

                if (Road[y, x] && roadTile != null)
                    roadTilemap.SetTile(cell, roadTile);

                if (Buildable[y, x] && buildableTile != null)
                    buildableTilemap.SetTile(cell, buildableTile);
                else if (paintBlocked && !Road[y, x] && blockedTile != null)
                    buildableTilemap.SetTile(cell, blockedTile);
            }
        }
    }

    #endregion

    #region Placement API

    /// <summary>
    /// Checks build bounds, Buildable mask, not Occupied, and optional road spacing.
    /// Since roads are fixed and towers cannot be placed on roads, path safety is guaranteed by design.
    /// </summary>
    public bool CanPlace(Vector2Int origin, Vector2Int footprint, int minManhattanFromRoad = 0)
    {
        foreach (var c in Cells(origin, footprint))
        {
            if (!InBounds(c)) return false;
            if (!Buildable[c.y, c.x]) return false;
            if (Occupied[c.y, c.x]) return false;
            if (minManhattanFromRoad > 0 && IsNearRoad(c, minManhattanFromRoad)) return false;
        }
        return true;
    }

    /// <summary>
    /// Marks or unmarks the Occupied grid for a placed tower footprint.
    /// </summary>
    public void SetOccupied(Vector2Int origin, Vector2Int footprint, bool value)
    {
        foreach (var c in Cells(origin, footprint))
            if (InBounds(c)) Occupied[c.y, c.x] = value;
    }

    #endregion

    #region Path / Waypoints

    /// <summary>
    /// Returns baked grid path (cells) for spawnerIndex -> exit, in order.
    /// </summary>
    public List<Vector2Int> GetPath(int spawnerIndex)
    {
        return _bakedPaths.TryGetValue(spawnerIndex, out var list) ? list : null;
    }

    /// <summary>
    /// Returns world-space center points for the baked path.
    /// </summary>
    public List<Vector3> GetWaypoints(int spawnerIndex)
    {
        var cells = GetPath(spawnerIndex);
        if (cells == null) return null;
        var list = new List<Vector3>(cells.Count);
        foreach (var c in cells) list.Add(CellCenterWorld(c));
        return list;
    }

    private List<Vector2Int> BakeRoadPath(Vector2Int start, Vector2Int goal)
    {
        if (!InBounds(start) || !InBounds(goal) || !Road[start.y, start.x] || !Road[goal.y, goal.x])
            return null;

        var q = new Queue<Vector2Int>();
        q.Enqueue(start);

        var came = new Dictionary<Vector2Int, Vector2Int>();
        var seen = new HashSet<Vector2Int> { start };

        while (q.Count > 0)
        {
            var cur = q.Dequeue();
            if (cur == goal) break;

            foreach (var n in Neighbors(cur))
            {
                if (!seen.Contains(n) && InBounds(n) && Road[n.y, n.x])
                {
                    seen.Add(n);
                    came[n] = cur;
                    q.Enqueue(n);
                }
            }
        }

        if (!came.ContainsKey(goal) && start != goal) return null;

        // Reconstruct
        var path = new List<Vector2Int>();
        var p = goal;
        path.Add(p);
        while (p != start)
        {
            if (!came.TryGetValue(p, out var prev))
                return null; // disconnected
            p = prev;
            path.Add(p);
        }
        path.Reverse();
        // Optional: thin waypoints (keep only corners)
        return ThinCorners(path);
    }

    private IEnumerable<Vector2Int> Neighbors(Vector2Int c)
    {
        if (roadNeighborMode == 8)
        {
            for (int dy = -1; dy <= 1; dy++)
                for (int dx = -1; dx <= 1; dx++)
                {
                    if (dx == 0 && dy == 0) continue;
                    yield return new Vector2Int(c.x + dx, c.y + dy);
                }
        }
        else
        {
            yield return new Vector2Int(c.x + 1, c.y);
            yield return new Vector2Int(c.x - 1, c.y);
            yield return new Vector2Int(c.x, c.y + 1);
            yield return new Vector2Int(c.x, c.y - 1);
        }
    }

    private static List<Vector2Int> ThinCorners(List<Vector2Int> path)
    {
        if (path.Count <= 2) return path;
        var thinned = new List<Vector2Int> { path[0] };
        for (int i = 1; i < path.Count - 1; i++)
        {
            var a = path[i - 1];
            var b = path[i];
            var c = path[i + 1];
            var ab = new Vector2Int(b.x - a.x, b.y - a.y);
            var bc = new Vector2Int(c.x - b.x, c.y - b.y);
            if (ab != bc) thinned.Add(b); // direction change => keep corner
        }
        thinned.Add(path[^1]);
        return thinned;
    }

    #endregion

    #region Helpers

    public bool InBounds(Vector2Int c) => c.x >= 0 && c.x < width && c.y >= 0 && c.y < height;

    public IEnumerable<Vector2Int> Cells(Vector2Int origin, Vector2Int footprint)
    {
        for (int dy = 0; dy < footprint.y; dy++)
            for (int dx = 0; dx < footprint.x; dx++)
                yield return new Vector2Int(origin.x + dx, origin.y + dy);
    }

    public bool IsNearRoad(Vector2Int c, int minManhattan)
    {
        for (int y = Math.Max(0, c.y - minManhattan); y <= Math.Min(height - 1, c.y + minManhattan); y++)
            for (int x = Math.Max(0, c.x - minManhattan); x <= Math.Min(width - 1, c.x + minManhattan); x++)
                if (Mathf.Abs(x - c.x) + Mathf.Abs(y - c.y) <= minManhattan && Road[y, x])
                    return true;
        return false;
    }

    /// <summary>
    /// CSV (x,y) → Tilemap cell (x, -y), then center world.
    /// </summary>
    public Vector3 CellCenterWorld(Vector2Int cell)
    {
        var tmCell = new Vector3Int(cell.x, -cell.y, 0);
        return (roadTilemap != null ? roadTilemap : buildableTilemap).GetCellCenterWorld(tmCell);
    }

    #endregion

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        // Draw spawners (green) and exit (red)
        Gizmos.matrix = Matrix4x4.identity;
        if (roadTilemap != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(CellCenterWorld(exitCell), 0.25f);
            Gizmos.color = Color.green;
            foreach (var s in spawners)
                Gizmos.DrawWireSphere(CellCenterWorld(s), 0.25f);
        }
    }
#endif
}
