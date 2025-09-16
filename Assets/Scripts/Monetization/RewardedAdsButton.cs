using System;
using UnityEngine;
using UnityEngine.UI;
using Unity.Services.LevelPlay;  // ensure correct namespace

public class RewardedAdsButton : MonoBehaviour
{
    [SerializeField] private Button _showAdButton;
    [SerializeField] private string _androidAdUnitId;
    [SerializeField] private string _iOSAdUnitId;

    private string _adUnitId;
    private LevelPlayRewardedAd _rewardedAd;
    private Action _rewardCallback;

    void Awake()
    {
    #if UNITY_ANDROID
        _adUnitId = _androidAdUnitId;
    #elif UNITY_IOS
        _adUnitId = _iOSAdUnitId;
    #else
        _adUnitId = _androidAdUnitId; // fallback
    #endif

        if (_showAdButton != null)
        {
            _showAdButton.interactable = false;
        }

        // LevelPlay is already initialized before creating ads.
        // wait for LevelPlay.OnInitSuccess before doing this.
        CreateRewardedAd();
        LoadAd();
    }

    void OnDestroy()
    {
        UnregisterCallbacks();
    }

    private void CreateRewardedAd()
    {
        _rewardedAd = new LevelPlayRewardedAd(_adUnitId);

        // Register event handlers
        _rewardedAd.OnAdLoaded += OnAdLoaded;
        _rewardedAd.OnAdLoadFailed += OnAdLoadFailed;
        _rewardedAd.OnAdDisplayed += OnAdDisplayed;
        _rewardedAd.OnAdDisplayFailed += OnAdDisplayFailed;
        _rewardedAd.OnAdRewarded += OnAdRewarded;
        _rewardedAd.OnAdClosed += OnAdClosed;
        _rewardedAd.OnAdClicked += OnAdClicked;
        _rewardedAd.OnAdInfoChanged += OnAdInfoChanged;
    }

    private void UnregisterCallbacks()
    {
        if (_rewardedAd != null)
        {
            _rewardedAd.OnAdLoaded -= OnAdLoaded;
            _rewardedAd.OnAdLoadFailed -= OnAdLoadFailed;
            _rewardedAd.OnAdDisplayed -= OnAdDisplayed;
            _rewardedAd.OnAdDisplayFailed -= OnAdDisplayFailed;
            _rewardedAd.OnAdRewarded -= OnAdRewarded;
            _rewardedAd.OnAdClosed -= OnAdClosed;
            _rewardedAd.OnAdClicked -= OnAdClicked;
            _rewardedAd.OnAdInfoChanged -= OnAdInfoChanged;
        }
    }

    public void LoadAd()
    {
        Debug.Log("[LevelPlay] Loading rewarded ad: " + _adUnitId);
        if (_rewardedAd != null)
        {
            _rewardedAd.LoadAd();
        }
    }

    public void ShowAd(Action onReward)
    {
        if (_rewardedAd == null)
        {
            Debug.LogWarning("[LevelPlay] ShowAd called but rewardedAd is null");
            return;
        }

        // Check if ad is ready and not placement‑capped
        if (_rewardedAd.IsAdReady() && !LevelPlayRewardedAd.IsPlacementCapped(_adUnitId))
        {
            Debug.Log("[LevelPlay] Showing rewarded ad");
            _rewardCallback = onReward;
            _rewardedAd.ShowAd();
        }
        else
        {
            Debug.Log("[LevelPlay] Ad not ready or placement capped");
            // maybe trigger fallback, or reload
            LoadAd();
        }
    }

    // --- Event handlers with correct signatures ---

    private void OnAdLoaded(LevelPlayAdInfo adInfo)
    {
        Debug.Log("[LevelPlay] Ad loaded: " + adInfo.PlacementName);
        if (_showAdButton != null)
        {
            _showAdButton.interactable = true;
        }
    }

    private void OnAdLoadFailed(LevelPlayAdError error)
    {
        Debug.LogError("[LevelPlay] Failed to load ad: " + error.ToString());
    }

    private void OnAdDisplayed(LevelPlayAdInfo adInfo)
    {
        Debug.Log("[LevelPlay] Ad displayed: " + adInfo.PlacementName);
    }

    private void OnAdDisplayFailed(LevelPlayAdDisplayInfoError displayError)
    {
        Debug.LogError("[LevelPlay] Failed to display ad: " + displayError.ToString());
        // Optionally load a new ad
        LoadAd();
    }

    private void OnAdRewarded(LevelPlayAdInfo adInfo, LevelPlayReward reward)
    {
        Debug.LogFormat("[LevelPlay] Ad rewarded: {0} - {1}", reward.Name, reward.Amount);
        if (_rewardCallback != null)
        {
            _rewardCallback.Invoke();
        }
    }

    private void OnAdClosed(LevelPlayAdInfo adInfo)
    {
        Debug.Log("[LevelPlay] Ad closed: " + adInfo.PlacementName);
        // After ad closes, load another for future
        LoadAd();
    }

    private void OnAdClicked(LevelPlayAdInfo adInfo)
    {
        Debug.Log("[LevelPlay] Ad clicked: " + adInfo.PlacementName);
    }

    private void OnAdInfoChanged(LevelPlayAdInfo adInfo)
    {
        Debug.Log("[LevelPlay] Ad info changed: " + adInfo.PlacementName);
    }
}
