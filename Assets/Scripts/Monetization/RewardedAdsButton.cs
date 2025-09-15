using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Advertisements;
using System; // 🔥 NEW: Add this to use the 'Action' type.
 
public class RewardedAdsButton : MonoBehaviour, IUnityAdsLoadListener, IUnityAdsShowListener
{
  [SerializeField] Button _showAdButton; // This can now be optional
  [SerializeField] string _androidAdUnitId = "Rewarded_Android";
  string _adUnitId = null;

  // 🔥 NEW: A variable to hold the specific reward action to be performed.
  private Action _rewardCallback;
 
  void Awake()
  {   
    #if UNITY_ANDROID
    _adUnitId = _androidAdUnitId;
    #endif

    // We keep this to pre-load an ad when the scene starts.
    LoadAd();
  }
 
  // Call this public method when you want to get an ad ready to show.
  public void LoadAd()
  {
    Debug.Log("Loading Ad: " + _adUnitId);
    Advertisement.Load(_adUnitId, this);
  }
 
  // If the ad successfully loads, we no longer need to hook it to a specific button.
  // It's just ready to be shown by any script that needs it.
  public void OnUnityAdsAdLoaded(string adUnitId)
  {
    Debug.Log("Ad Loaded: " + adUnitId);
    if (_showAdButton) _showAdButton.interactable = true; // Still enable a generic button if one is assigned
  }
 
  /// <summary>
  /// 🔥 MODIFIED: This method now accepts a callback action to execute upon success.
  /// </summary>
  /// <param name="onReward">The function to call if the ad is completed.</param>
  public void ShowAd(Action onReward)
  {
    Debug.Log("Show Ad requested.");
    // Store the callback that was passed in.
    _rewardCallback = onReward;
    
    // Then show the ad.
    Advertisement.Show(_adUnitId, this);
  }
 
  // This is where the magic happens.
  public void OnUnityAdsShowComplete(string adUnitId, UnityAdsShowCompletionState showCompletionState)
  {
    if (adUnitId.Equals(_adUnitId) && showCompletionState.Equals(UnityAdsShowCompletionState.COMPLETED))
    {
      Debug.Log("Unity Ads Rewarded Ad Completed. Invoking reward callback.");
      // 🔥 CHANGED: Instead of granting a specific reward, we invoke the stored callback.
      _rewardCallback?.Invoke();
    }
    
    // We should load another ad for the next time.
    LoadAd();
  }
 
  // --- Error callbacks remain the same ---
  public void OnUnityAdsFailedToLoad(string adUnitId, UnityAdsLoadError error, string message)
  {
    Debug.Log($"Error loading Ad Unit {adUnitId}: {error.ToString()} - {message}");
  }
 
  public void OnUnityAdsShowFailure(string adUnitId, UnityAdsShowError error, string message)
  {
    Debug.Log($"Error showing Ad Unit {adUnitId}: {error.ToString()} - {message}");
  }
 
  public void OnUnityAdsShowStart(string adUnitId) { }
  public void OnUnityAdsShowClick(string adUnitId) { }
 
  void OnDestroy()
  {
    if (_showAdButton) _showAdButton.onClick.RemoveAllListeners();
  }
}