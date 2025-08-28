using System.Collections.Generic;
using UnityEngine;

public class TowerAppearance : MonoBehaviour
{
    [Header("Default / Fallback")]
    [SerializeField] private Material defaultMaterial;

    [Header("Target to replace (e.g., your current BLACK material)")]
    [Tooltip("Only slots that reference THIS material will be replaced by the selected skin.")]
    [SerializeField] private Material targetBaseMaterial; // your original black

    [Tooltip("If targetBaseMaterial is not found on a renderer, replace slot 0 as a fallback.")]
    [SerializeField] private bool fallbackToIndex0IfNotFound = false;

    [Header("Which renderers to skin")]
    [SerializeField] private List<Renderer> renderers = new List<Renderer>();

    // Cache: for each renderer, which indices should be replaced
    private readonly Dictionary<Renderer, int[]> _replaceIndices = new();

    void Awake()
    {
        if (renderers.Count == 0)
            renderers.AddRange(GetComponentsInChildren<Renderer>(true));

        // Precompute which indices match the "black" (targetBaseMaterial)
        BuildReplaceIndexCache();

        // Subscribe + apply
        if (CosmeticManager.Instance != null)
            CosmeticManager.Instance.OnSkinChanged += ApplyMaterial;

        ApplyMaterial(CosmeticManager.Instance ? CosmeticManager.Instance.CurrentSkinOrNull : null);
    }

    void OnDestroy()
    {
        if (CosmeticManager.Instance != null)
            CosmeticManager.Instance.OnSkinChanged -= ApplyMaterial;
    }

    private void BuildReplaceIndexCache()
    {
        _replaceIndices.Clear();

        foreach (var r in renderers)
        {
            if (!r) continue;

            var mats = r.sharedMaterials;
            if (mats == null || mats.Length == 0) continue;

            List<int> toReplace = new();

            // Prefer exact reference match
            if (targetBaseMaterial)
            {
                for (int i = 0; i < mats.Length; i++)
                {
                    if (mats[i] == targetBaseMaterial)
                        toReplace.Add(i);
                }
            }

            // Optional fallback: slot 0 (the “first applied”)
            if (toReplace.Count == 0 && fallbackToIndex0IfNotFound)
                toReplace.Add(0);

            _replaceIndices[r] = toReplace.ToArray();
        }
    }

    public void RebuildCacheAndReapply()
    {
        BuildReplaceIndexCache();
        ApplyMaterial(CosmeticManager.Instance ? CosmeticManager.Instance.CurrentSkinOrNull : null);
    }

    private void ApplyMaterial(Material skinOrNull)
    {
        Material mat = skinOrNull ? skinOrNull : defaultMaterial;
        if (!mat) return;

        foreach (var r in renderers)
        {
            if (!r) continue;

            if (!_replaceIndices.TryGetValue(r, out var indices) || indices == null || indices.Length == 0)
                continue;

            var mats = r.sharedMaterials; // shared to avoid instancing per object
            if (mats == null || mats.Length == 0) continue;

            foreach (int i in indices)
            {
                if (i >= 0 && i < mats.Length)
                    mats[i] = mat;
            }

            r.sharedMaterials = mats;
        }
    }
}
