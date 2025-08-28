using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class SkinItemUI : MonoBehaviour, IPointerClickHandler
{
    [Header("Optional UI (only name is required)")]
    public TextMeshProUGUI nameText;                 // label
    public GameObject lockGroup;          // e.g., "Watch ad to unlock" badge
    public GameObject selectedHighlight;  // outline/glow

    // runtime state
    private bool _isUnlocked;
    private int _cosmeticIndex;           // -1 = Default, >=0 = CosmeticManager.availableSkins index
    private System.Action<int> _onSelect; // will call CosmeticManager.SelectSkin(index)
    private System.Action _onUnlock;      // placeholder for ad unlock

    /// <summary>
    /// Prepare the card. Keep it dead simple.
    /// </summary>
    public void Setup(string displayName, int cosmeticIndex, bool isUnlocked, bool isSelected,
                      System.Action<int> onSelect, System.Action onUnlock)
    {
        if (nameText) nameText.text = displayName;

        _cosmeticIndex = cosmeticIndex;
        _isUnlocked = isUnlocked;
        _onSelect = onSelect;
        _onUnlock = onUnlock;

        if (lockGroup) lockGroup.SetActive(!_isUnlocked);
        if (selectedHighlight) selectedHighlight.SetActive(isSelected);
    }

    public void OnPointerClick(PointerEventData _)
    {
        if (_isUnlocked) _onSelect?.Invoke(_cosmeticIndex);
        else _onUnlock?.Invoke();
    }

    public void SetSelected(bool selected)
    {
        if (selectedHighlight) selectedHighlight.SetActive(selected);
    }

    public void SetUnlocked(bool unlocked)
    {
        _isUnlocked = unlocked;
        if (lockGroup) lockGroup.SetActive(!unlocked);
    }
}
