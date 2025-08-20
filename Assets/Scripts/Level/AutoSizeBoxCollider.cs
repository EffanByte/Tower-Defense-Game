// AutoSizeBoxCollider.cs
using UnityEngine;
using UnityEngine.Tilemaps;

[ExecuteAlways]
[RequireComponent(typeof(Tilemap))]
[RequireComponent(typeof(BoxCollider))]
public class AutoSizeBoxCollider : MonoBehaviour
{
    [Tooltip("Z thickness so 3D raycasts can hit the tilemap plane.")]
    public float thickness = 0.1f;

    [Tooltip("Compress tilemap bounds before sizing (removes empty margins).")]
    public bool compressOnSync = true;

    [Tooltip("Set the BoxCollider as a trigger (recommended for click picking).")]
    public bool isTrigger = true;

    void Reset() => Sync();
    void OnEnable() => Sync();
    void OnValidate() => Sync();

    public void Sync()
    {
        var tm = GetComponent<Tilemap>();
        var box = GetComponent<BoxCollider>();

        if (compressOnSync)
            tm.CompressBounds();

        // localBounds is in the Tilemap's local space
        Bounds lb = tm.localBounds;

        // If nothing is painted yet, bail out gracefully
        if (lb.size.sqrMagnitude < 1e-6f)
        {
            // Optionally set a tiny fallback to avoid zero-size collider
            box.size = new Vector3(0.01f, 0.01f, Mathf.Max(0.01f, thickness));
            box.center = Vector3.zero;
            box.isTrigger = isTrigger;
            return;
        }

        // BoxCollider expects local-space size/center
        box.size = new Vector3(lb.size.x, lb.size.y, Mathf.Max(0.01f, thickness));
        box.center = new Vector3(lb.center.x, lb.center.y, 0f);
        box.isTrigger = isTrigger;
    }
}
