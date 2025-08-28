using System;
using System.Collections.Generic;
using UnityEngine;

public class CosmeticManager : MonoBehaviour
{
    public static CosmeticManager Instance { get; private set; }

    [Header("Skins")]
    [Tooltip("Leave empty if you only want the default tower material. Otherwise add shared materials here.")]
    [SerializeField] private List<Material> availableSkins = new List<Material>();

    [Tooltip("Use -1 for 'no skin' so towers fall back to their own default (black & gold).")]
    [SerializeField] private int selectedIndex = -1;

    public event Action<Material> OnSkinChanged;

    private const string PrefKey = "SelectedSkinIndex";

    public Material CurrentSkinOrNull
        => (selectedIndex >= 0 && selectedIndex < availableSkins.Count) ? availableSkins[selectedIndex] : null;

    void Awake()
    {
        if (Instance && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Load saved selection if exists
        if (PlayerPrefs.HasKey(PrefKey))
            selectedIndex = PlayerPrefs.GetInt(PrefKey, -1);
    }

    /// <summary>
    /// Select a skin by index (-1 for none/fallback).
    /// </summary>
    public void SelectSkin(int index)
    {
        if (index < -1 || index >= availableSkins.Count)
        {
            Debug.LogWarning($"[CosmeticManager] Invalid skin index: {index}");
            return;
        }

        selectedIndex = index;
        PlayerPrefs.SetInt(PrefKey, selectedIndex);
        PlayerPrefs.Save();

        OnSkinChanged?.Invoke(CurrentSkinOrNull);
        Debug.Log($"[CosmeticManager] Skin selected: {selectedIndex} → {(CurrentSkinOrNull ? CurrentSkinOrNull.name : "default (black & gold)")}");
    }

    public int GetSelectedIndex() => selectedIndex;
    public int GetSkinCount() => availableSkins.Count;
    public Material GetSkinAt(int i) => (i >= 0 && i < availableSkins.Count) ? availableSkins[i] : null;
}
