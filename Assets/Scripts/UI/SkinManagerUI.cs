using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class SkinManagerUI : MonoBehaviour
{
    [System.Serializable]
    public class SkinMeta
    {
        public string id;
        public string displayName;
        public int cosmeticIndex = -1;   // -1 = Default (black & gold)
        public bool unlockedByDefault;
    }

    [Header("Data")]
    [Tooltip("Leave empty to auto-populate from CosmeticManager (Default + availableSkins).")]
    public List<SkinMeta> skins = new List<SkinMeta>();

    [Header("UI")]
    public Transform gridParent;         // panel with Grid/Vertical LayoutGroup
    public GameObject skinCardPrefab;    // root has SkinItemUI

    [Header("Materials")]
    public Material defaultMaterial;     // your black & gold

    [Header("Managers")]
    public CosmeticManager cosmeticManager; // auto-fills if left null

    private readonly Dictionary<string, bool> _unlocked = new();
    private readonly List<SkinItemUI> _cards = new();

    private const string PrefUnlockedKey = "skin_unlocked_";

    private IEnumerator Start()
    {
        // Wait for CosmeticManager (in case it spawns later)
        while (cosmeticManager == null)
        {
            cosmeticManager = CosmeticManager.Instance;
            if (cosmeticManager) break;
            yield return null;
        }

        if (cosmeticManager) cosmeticManager.OnSkinChanged += RefreshSelection;

        AutoPopulateSkinsFromManagerIfEmpty();

        if (!skinCardPrefab)
        {
            Debug.LogError("[SkinManagerUI] skinCardPrefab not assigned.");
            yield break;
        }
        if (!gridParent)
        {
            gridParent = transform;
        }
        if (skins == null || skins.Count == 0)
        {
            Debug.LogWarning("[SkinManagerUI] No skins configured.");
            yield break;
        }

        BuildOrRefresh();
    }

    private void OnDisable()
    {
        if (cosmeticManager) cosmeticManager.OnSkinChanged -= RefreshSelection;
    }

    public void BuildOrRefresh()
    {
        LoadUnlocks();

        if (_cards.Count == 0)
        {
            for (int i = 0; i < skins.Count; i++)
            {
                var meta = skins[i];
                var go = Instantiate(skinCardPrefab, gridParent);
                go.name = $"SkinCard_{meta.id}_{i}";

                var card = go.GetComponent<SkinItemUI>();
                if (!card)
                {
                    Debug.LogError($"[SkinManagerUI] SkinItemUI missing on prefab instance: {go.name}");
                    continue;
                }

                bool isUnlocked = IsUnlocked(meta);
                bool isSelected = IsSelected(meta.cosmeticIndex);
                Texture2D tex = MakePreviewTexture2D(ResolveMaterial(meta), 64);

                _cards.Add(card);
                card.Setup(
                    tex,
                    meta.displayName,
                    isUnlocked,
                    isSelected,
                    onSelect: () => SelectSkin(meta),
                    onUnlock: () => UnlockSkin(meta)
                );
            }
        }
        else
        {
            for (int i = 0; i < skins.Count && i < _cards.Count; i++)
            {
                var meta = skins[i];
                var card = _cards[i];
                if (!card) continue;

                card.SetUnlocked(IsUnlocked(meta));
                card.SetSelected(IsSelected(meta.cosmeticIndex));
                if (card.preview) card.preview.texture = MakePreviewTexture2D(ResolveMaterial(meta), 64);
            }
        }
    }

    // ─────────── Selection & Unlock ───────────
    private void SelectSkin(SkinMeta meta)
    {
        if (!IsUnlocked(meta)) return;
        cosmeticManager?.SelectSkin(meta.cosmeticIndex); // -1 = default
        RefreshSelection(cosmeticManager?.CurrentSkinOrNull);
    }

    private void UnlockSkin(SkinMeta meta)
    {
        // Placeholder: unlock immediately. Replace with ad logic later.
        SaveUnlock(meta.id, true);
        BuildOrRefresh();
    }

    private void RefreshSelection(Material _)
    {
        int sel = cosmeticManager ? cosmeticManager.GetSelectedIndex() : -1;
        for (int i = 0; i < skins.Count && i < _cards.Count; i++)
            _cards[i].SetSelected(skins[i].cosmeticIndex == sel);
    }

    // ─────────── Persistence ───────────
    private void LoadUnlocks()
    {
        _unlocked.Clear();
        foreach (var meta in skins)
        {
            if (meta.cosmeticIndex == -1)
            {
                _unlocked[meta.id] = true; // Default always unlocked
                continue;
            }

            int v = PlayerPrefs.GetInt(PrefUnlockedKey + meta.id, meta.unlockedByDefault ? 1 : 0);
            _unlocked[meta.id] = (v == 1);
        }
    }

    private void SaveUnlock(string id, bool unlocked)
    {
        PlayerPrefs.SetInt(PrefUnlockedKey + id, unlocked ? 1 : 0);
        PlayerPrefs.Save();
        _unlocked[id] = unlocked;
    }

    private bool IsUnlocked(SkinMeta meta)
        => meta.cosmeticIndex == -1 || (_unlocked.TryGetValue(meta.id, out var u) ? u : meta.unlockedByDefault);

    private bool IsSelected(int cosmeticIndex)
        => !cosmeticManager ? cosmeticIndex == -1 : cosmeticManager.GetSelectedIndex() == cosmeticIndex;

    // ─────────── Auto-populate from CosmeticManager ───────────
    private void AutoPopulateSkinsFromManagerIfEmpty()
    {
        if (skins != null && skins.Count > 0) return;

        skins = new List<SkinMeta>
        {
            new SkinMeta { id = "default", displayName = "Black & Gold", cosmeticIndex = -1, unlockedByDefault = true }
        };

        int count = cosmeticManager ? cosmeticManager.GetSkinCount() : 0;
        for (int i = 0; i < count; i++)
        {
            var mat = cosmeticManager.GetSkinAt(i);
            string name = mat ? mat.name : $"Skin_{i}";
            skins.Add(new SkinMeta
            {
                id = SlugifyId(name),
                displayName = name,
                cosmeticIndex = i,
                unlockedByDefault = false
            });
        }
    }

    private string SlugifyId(string raw)
    {
        if (string.IsNullOrEmpty(raw)) return "skin";
        raw = raw.ToLowerInvariant().Trim();
        var sb = new StringBuilder(raw.Length);
        foreach (char c in raw)
        {
            if ((c >= 'a' && c <= 'z') || (c >= '0' && c <= '9') || c == '_' || c == '-') sb.Append(c);
            else sb.Append('_');
        }
        var s = sb.ToString();
        while (s.Contains("__")) s = s.Replace("__", "_");
        return s.Trim('_');
    }

    // ─────────── MATERIAL → Texture2D (very basic) ───────────
    private Material ResolveMaterial(SkinMeta meta)
    {
        if (meta.cosmeticIndex == -1) return defaultMaterial;
        if (!cosmeticManager) return null;
        int idx = meta.cosmeticIndex;
        return (idx >= 0 && idx < cosmeticManager.GetSkinCount()) ? cosmeticManager.GetSkinAt(idx) : null;
    }

    private Texture2D MakePreviewTexture2D(Material mat, int size)
    {
        if (!mat) return MakeSolid(size, size, new Color(0.7f, 0.7f, 0.7f));

        // 1) Use Texture2D if the material has one
        Texture main = null;
        if (mat.HasProperty("_MainTex")) main = mat.GetTexture("_MainTex");
        if (!main && mat.HasProperty("_BaseMap")) main = mat.GetTexture("_BaseMap");

        var t2d = main as Texture2D;
        if (t2d != null) return t2d; // RawImage can show it directly

        // 2) Otherwise create a solid swatch from base color (or gray)
        Color color = new Color(0.7f, 0.7f, 0.7f);
        if (mat.HasProperty("_BaseColor")) color = mat.GetColor("_BaseColor");
        else if (mat.HasProperty("_Color")) color = mat.GetColor("_Color");

        return MakeSolid(size, size, color);
    }

    private Texture2D MakeSolid(int w, int h, Color c)
    {
        var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        var pixels = new Color32[w * h];
        var c32 = (Color32)c;
        for (int i = 0; i < pixels.Length; i++) pixels[i] = c32;
        tex.SetPixels32(pixels);
        tex.Apply(false, false); // keep readable (harmless)
        tex.name = $"Solid_{(int)(c.r * 255)}_{(int)(c.g * 255)}_{(int)(c.b * 255)}";
        return tex;
    }

    // optional panel toggles
    public void ShowUI() => transform.parent.gameObject.SetActive(true);
    public void HideUI() => transform.parent.gameObject.SetActive(false);
}
