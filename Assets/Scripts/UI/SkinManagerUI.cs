using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI; // for Image

public class SkinManagerUI : MonoBehaviour
{
    [System.Serializable]
    public class SkinMeta
    {
        public string id;                 
        public string displayName;        
        public int cosmeticIndex = -1;    
        public bool unlockedByDefault;    
    }

    [Header("Data")]
    public List<SkinMeta> skins = new List<SkinMeta>();

    [Header("UI")]
    public Transform gridParent;
    public GameObject skinCardPrefab;

    [Header("Managers")]
    public CosmeticManager cosmeticManager;

    private readonly List<SkinItemUI> _cards = new();

    private IEnumerator Start()
    {
        while (cosmeticManager == null)
        {
            cosmeticManager = CosmeticManager.Instance;
            if (cosmeticManager) break;
            yield return null;
        }

        if (!skinCardPrefab)
        {
            Debug.LogError("[SkinManagerUI] skinCardPrefab not assigned.");
            yield break;
        }
        if (!gridParent) gridParent = transform;

        if (cosmeticManager != null)
            cosmeticManager.OnSkinChanged += _ => RefreshSelection();

        AutoPopulateIfEmpty();

        RebuildCards();
        RefreshSelection();
    }

    private void OnDisable()
    {
        if (cosmeticManager != null)
            cosmeticManager.OnSkinChanged -= _ => RefreshSelection();
    }

    private void RebuildCards()
    {
        foreach (Transform child in gridParent)
            Destroy(child.gameObject);
        _cards.Clear();

        for (int i = 0; i < skins.Count; i++)
        {
            var meta = skins[i];

            var go = Instantiate(skinCardPrefab, gridParent);
            go.name = $"SkinCard_{meta.id}_{i}";

            //  add top padding for the first row
            if (i == 0)
            {
                var rt = go.GetComponent<RectTransform>();
                if (rt != null)
                {
                    rt.offsetMax -= new Vector2(0, 10); // shift down 10 from the top
                }
            }
            var card = go.GetComponent<SkinItemUI>();
            if (!card)
            {
                Debug.LogError($"[SkinManagerUI] SkinItemUI missing on prefab instance: {go.name}");
                continue;
            }

            bool isUnlocked = IsUnlockedForTesting(meta);
            bool isSelected = IsSelected(meta.cosmeticIndex);

            card.Setup(
                displayName: meta.displayName,
                cosmeticIndex: meta.cosmeticIndex,
                isUnlocked: isUnlocked,
                isSelected: isSelected,
                onSelect: idx => cosmeticManager?.SelectSkin(idx),
                onUnlock: () => UnlockForTesting(meta)
            );

            // 🔥 NEW: try load preview sprite from Assets/Textures
            var previewSprite = LoadSkinSprite(meta.id);
            if (previewSprite != null)
            {
                var img = go.GetComponentInChildren<Image>();
                if (img != null)
                {
                    img.sprite = previewSprite;
                }
            }

            _cards.Add(card);
        }
    }

    private void RefreshSelection()
    {
        int selected = cosmeticManager ? cosmeticManager.GetSelectedIndex() : -1;
        for (int i = 0; i < skins.Count && i < _cards.Count; i++)
            _cards[i].SetSelected(skins[i].cosmeticIndex == selected);
    }

    private bool IsUnlockedForTesting(SkinMeta meta)
    {
        if (meta.cosmeticIndex == -1) return true;
        int v = 1; 
        return v == 1;
    }

    private void UnlockForTesting(SkinMeta meta)
    {
        Debug.Log($"[SkinManagerUI] (Test) Unlock requested for '{meta.displayName}' — already unlocked in test.");
    }

    private void AutoPopulateIfEmpty()
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

    private bool IsSelected(int cosmeticIndex)
        => !cosmeticManager ? cosmeticIndex == -1 : cosmeticManager.GetSelectedIndex() == cosmeticIndex;

    private string SlugifyId(string raw)
    {
        if (string.IsNullOrEmpty(raw)) return "skin";
        raw = raw.ToLowerInvariant().Trim();
        var sb = new StringBuilder(raw.Length);
        foreach (char c in raw)
            sb.Append(((c >= 'a' && c <= 'z') || (c >= '0' && c <= '9') || c == '_' || c == '-') ? c : '_');
        var s = sb.ToString();
        while (s.Contains("__")) s = s.Replace("__", "_");
        return s.Trim('_');
    }

    // ─────────────────────────────────────────────
    // 🔥 NEW HELPER: load PNG as Sprite from Assets/Textures
    // ─────────────────────────────────────────────
    private Sprite LoadSkinSprite(string id)
    {
        string path = System.IO.Path.Combine(Application.dataPath, "Textures", id + ".png");
        if (!System.IO.File.Exists(path))
        {
            Debug.LogWarning($"[SkinManagerUI] Preview not found for {id} at {path}");
            return null;
        }

        byte[] pngData = System.IO.File.ReadAllBytes(path);
        Texture2D tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        if (tex.LoadImage(pngData))
        {
            return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
        }
        return null;
    }

    public void ShowUI() => transform.parent.gameObject.SetActive(true);
    public void HideUI() => transform.parent.gameObject.SetActive(false);
}
