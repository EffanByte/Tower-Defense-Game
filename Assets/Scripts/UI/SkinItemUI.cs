using UnityEngine;
using UnityEngine.UI;

public class SkinItemUI : MonoBehaviour
{
    [Header("UI Refs")]
    public RawImage preview;       // assign in prefab
    public Text nameText;
    public GameObject lockGroup;   // shows "Watch ad to unlock"
    public Button unlockButton;
    public Button selectButton;
    public GameObject selectedHighlight;

    private System.Action _onSelect;
    private System.Action _onUnlock;

    public void Setup(
        Texture2D previewTex, string displayName,
        bool isUnlocked, bool isSelected,
        System.Action onSelect, System.Action onUnlock
    )
    {
        if (preview)
        {
            preview.texture = previewTex;
            preview.enabled = true;
            preview.color = Color.white;
        }

        if (nameText) nameText.text = displayName;

        _onSelect = onSelect;
        _onUnlock = onUnlock;

        if (lockGroup) lockGroup.SetActive(!isUnlocked);

        if (selectButton)
        {
            selectButton.interactable = isUnlocked;
            selectButton.onClick.RemoveAllListeners();
            selectButton.onClick.AddListener(() => _onSelect?.Invoke());
        }

        if (unlockButton)
        {
            unlockButton.onClick.RemoveAllListeners();
            unlockButton.onClick.AddListener(() => _onUnlock?.Invoke());
        }

        if (selectedHighlight) selectedHighlight.SetActive(isSelected);
    }

    public void SetSelected(bool selected)
    {
        if (selectedHighlight) selectedHighlight.SetActive(selected);
    }

    public void SetUnlocked(bool unlocked)
    {
        if (lockGroup) lockGroup.SetActive(!unlocked);
        if (selectButton) selectButton.interactable = unlocked;
    }
}
