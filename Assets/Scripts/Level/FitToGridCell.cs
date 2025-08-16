using UnityEngine;
using UnityEngine.Tilemaps;

[ExecuteAlways]
public class FitToGridCell : MonoBehaviour
{
    public Grid grid;                     // assign the Grid (or auto-find)
    public bool keepAspect = true;        // false = stretch to fill
    public Vector2 padding = Vector2.zero; // world units trimmed from each side
    public Vector3 extraScale = Vector3.one; // post-multiply tweak

    void Reset() { TryFindGrid(); }
    void OnEnable() { Fit(); }
    void OnValidate() { Fit(); }

    void TryFindGrid()
    {
        if (grid) return;
        grid = GetComponentInParent<Grid>();
        if (!grid) Debug.LogWarning("FitToGridCell: No Grid found in parents.");
    }

    void Fit()
    {
        if (!grid) TryFindGrid();
        if (!grid) return;

        // get target cell size (XY only)
        var cell = grid.cellSize;
        Vector2 target = new Vector2(Mathf.Abs(cell.x) - padding.x * 2f,
                                     Mathf.Abs(cell.y) - padding.y * 2f);
        if (target.x <= 0 || target.y <= 0) return;

        // get current renderer bounds (in local space if possible)
        var sr = GetComponentInChildren<SpriteRenderer>();
        var mr = GetComponentInChildren<MeshRenderer>();

        Vector2 size;
        if (sr && sr.sprite)
        {
            // sprite world size at scale (1,1,1) = rect/PPU
            var rect = sr.sprite.rect;
            float ppu = sr.sprite.pixelsPerUnit;
            size = new Vector2(rect.width / ppu, rect.height / ppu);
        }
        else if (mr && mr.sharedMaterial)
        {
            // mesh size in local space (assuming mesh units ~= meters)
            var mf = mr.GetComponent<MeshFilter>();
            if (mf && mf.sharedMesh)
            {
                var b = mf.sharedMesh.bounds.size; // local space
                size = new Vector2(b.x, b.y);
            }
            else
            {
                size = Vector2.one; // fallback
            }
        }
        else
        {
            // last resort: renderer bounds in world space / current lossyScale
            var r = GetComponentInChildren<Renderer>();
            if (!r) return;
            var ws = r.bounds.size;
            var ls = transform.lossyScale;
            size = new Vector2(ws.x / Mathf.Max(ls.x, 1e-4f),
                               ws.y / Mathf.Max(ls.y, 1e-4f));
        }

        if (size.x <= 0 || size.y <= 0) return;

        // compute scale to hit target
        Vector3 scale = transform.localScale;
        float sx = target.x / size.x;
        float sy = target.y / size.y;

        if (keepAspect)
        {
            float s = Mathf.Min(sx, sy);
            scale = new Vector3(s, s, scale.z);
        }
        else
        {
            scale = new Vector3(sx, sy, scale.z);
        }

        scale = Vector3.Scale(scale, extraScale);
        transform.localScale = scale;

        // optional: center on cell
        if (grid && Application.isEditor)
        {
            var pos = grid.WorldToCell(transform.position);
            transform.position = grid.GetCellCenterWorld(pos);
        }
    }
}
