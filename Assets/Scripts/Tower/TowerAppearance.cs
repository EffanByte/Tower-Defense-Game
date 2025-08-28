using System.Collections.Generic;
using UnityEngine;

public class TowerAppearance : MonoBehaviour
{
    [Header("Default / Fallback")]
    [Tooltip("Your default black & gold material.")]
    [SerializeField] private Material defaultMaterial;

    [Header("Which renderers to skin")]
    [Tooltip("Leave empty to auto-collect all child MeshRenderers & SkinnedMeshRenderers.")]
    [SerializeField] private List<Renderer> renderers = new List<Renderer>();

    void Awake()
    {
        if (renderers.Count == 0)
        {
            renderers.AddRange(GetComponentsInChildren<Renderer>(true));
        }

        // Subscribe to manager if exists
        if (CosmeticManager.Instance != null)
            CosmeticManager.Instance.OnSkinChanged += ApplyMaterial;

        // Apply current skin (or fallback) immediately
        ApplyMaterial(CosmeticManager.Instance ? CosmeticManager.Instance.CurrentSkinOrNull : null);
    }

    void OnDestroy()
    {
        if (CosmeticManager.Instance != null)
            CosmeticManager.Instance.OnSkinChanged -= ApplyMaterial;
    }

    private void ApplyMaterial(Material skinOrNull)
    {
        Material mat = skinOrNull != null ? skinOrNull : defaultMaterial;
        if (mat == null) return;

        // Use sharedMaterial to avoid per-instance material copies
        foreach (var r in renderers)
        {
            if (!r) continue;

            // If the renderer has multiple material slots, fill them all with the same skin
            var mats = r.sharedMaterials;
            for (int i = 0; i < mats.Length; i++)
                mats[i] = mat;
            r.sharedMaterials = mats;
        }
    }
}
