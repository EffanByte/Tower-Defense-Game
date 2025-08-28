using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class SkinManagerUI : MonoBehaviour
{
    [System.Serializable]
    public class SkinMeta
    {
        public string id;                 // unique key (e.g., "default", "green", "blue")
        public string displayName;        // label on the card
        public int cosmeticIndex = -1;    // -1 = Default, >=0 = CosmeticManager.availableSkins index
        public bool unlockedByDefault;    // ignored in testing (we force unlocked)
    }

    [Header("Data")]
    [Tooltip("Leave empty to auto-populate from CosmeticManager (Default + availableSkins).")]
    public List<SkinMeta> skins = new List<SkinMeta>();

    [Header("UI")]
    public Transform gridParent;      // panel with Grid/Vertical LayoutGroup
    public GameObject skinCardPrefab; // prefab with SkinItemUI on the root

    [Header("Managers")]
    public CosmeticManager cosmeticManager; // auto-fills if null

    private readonly List<SkinItemUI> _cards = new();

    private IEnumerator Start()
    {
        // wait for CosmeticManager (if it spawns later)
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

        // keep UI highlight in sync with changes
        if (cosmeticManager != null)
            cosmeticManager.OnSkinChanged += _ => RefreshSelection();

        // if you didn’t hand-enter skins in Inspector, auto-list them
        AutoPopulateIfEmpty();

        // build cards fresh
        RebuildCards();
        RefreshSelection();
    }

    private void OnDisable()
    {
        if (cosmeticManager != null)
            cosmeticManager.OnSkinChanged -= _ => RefreshSelection();
    }

    // Build from scratch (simple & reliable)
    private void RebuildCards()
    {
        // clear old
        foreach (Transform child in gridParent)
            Destroy(child.gameObject);
        _cards.Clear();

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

            bool isUnlocked = IsUnlockedForTesting(meta);  // ← your “v = 1” testing rule
            bool isSelected = IsSelected(meta.cosmeticIndex);

            card.Setup(
                displayName: meta.displayName,
                cosmeticIndex: meta.cosmeticIndex,
                isUnlocked: isUnlocked,
                isSelected: isSelected,
                onSelect: idx => cosmeticManager?.SelectSkin(idx), // fires OnSkinChanged
                onUnlock: () => UnlockForTesting(meta)             // placeholder
            );

            _cards.Add(card);
        }
    }

    private void RefreshSelection()
    {
        int selected = cosmeticManager ? cosmeticManager.GetSelectedIndex() : -1;
        for (int i = 0; i < skins.Count && i < _cards.Count; i++)
            _cards[i].SetSelected(skins[i].cosmeticIndex == selected);
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Testing unlock logic (keeps your “v = 1” behavior)
    // ──────────────────────────────────────────────────────────────────────────
    private bool IsUnlockedForTesting(SkinMeta meta)
    {
        if (meta.cosmeticIndex == -1) return true; // Default always unlocked
        int v = 1; // <- DO NOT CHANGE: your testing flag
        return v == 1;
    }

    private void UnlockForTesting(SkinMeta meta)
    {
        // For now everything is considered unlocked (v=1).
        // If you later hook ads, handle success here and call RebuildCards()/RefreshSelection().
        Debug.Log($"[SkinManagerUI] (Test) Unlock requested for '{meta.displayName}' — already unlocked in test.");
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Auto-populate from CosmeticManager (Default + availableSkins)
    // ──────────────────────────────────────────────────────────────────────────
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

    public void ShowUI() => transform.parent.gameObject.SetActive(true);
    public void HideUI() => transform.parent.gameObject.SetActive(false);
    
}
