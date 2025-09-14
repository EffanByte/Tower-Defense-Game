using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

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
    
    // 🔥 NEW: A constant prefix for our PlayerPrefs keys to keep them organized.
    private const string UnlockKeyPrefix = "SKIN_UNLOCKED_";

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

            if (i == 0)
            {
                var rt = go.GetComponent<RectTransform>();
                if (rt != null)
                {
                    rt.offsetMax -= new Vector2(0, 10);
                }
            }

            var card = go.GetComponent<SkinItemUI>();
            if (!card) continue;

            // 🔥 CHANGED: We now call our new, real functions.
            bool isUnlocked = IsUnlocked(meta);
            bool isSelected = IsSelected(meta.cosmeticIndex);

            card.Setup(
                displayName: meta.displayName,
                cosmeticIndex: meta.cosmeticIndex,
                isUnlocked: isUnlocked,
                isSelected: isSelected,
                onSelect: idx => cosmeticManager?.SelectSkin(idx),
                onUnlock: () => UnlockSkin(meta) // This is triggered by the lock icon.
            );
            
            var previewSprite = LoadSkinSprite(meta.id);
            if (previewSprite != null)
            {
                var img = go.GetComponentInChildren<Image>();
                if (img != null) img.sprite = previewSprite;
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

    // check for unlocks.
    private bool IsUnlocked(SkinMeta meta)
    {
        // A skin is always unlocked if it's marked as "unlocked by default".
        if (meta.unlockedByDefault)
        {
            return true;
        }

        // Otherwise, we check PlayerPrefs to see if a key exists for this skin's ID.
        // We use '0' as the default value (meaning locked). '1' means unlocked.
        string key = UnlockKeyPrefix + meta.id;
        return PlayerPrefs.GetInt(key, 0) == 1;
    }

    // perform an unlock.
    private void UnlockSkin(SkinMeta meta)
    {
        if (meta.unlockedByDefault) return;

        // Set the PlayerPrefs key for this skin to '1' (unlocked) and save it.
        string key = UnlockKeyPrefix + meta.id;
        PlayerPrefs.SetInt(key, 1);
        PlayerPrefs.Save();

        Debug.Log($"[SkinManagerUI] Unlocked '{meta.displayName}'! The UI will now refresh.");

        // After unlocking, we must rebuild the cards to reflect the change.
        RebuildCards();
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
    
    private Sprite LoadSkinSprite(string id)
    {
        string path = System.IO.Path.Combine(Application.dataPath, "Textures", id + ".png");
        if (!System.IO.File.Exists(path))
            return null;

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