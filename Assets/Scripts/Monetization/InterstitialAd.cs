using UnityEngine;
using UnityEngine.Advertisements;
 
public class InterstitialAd : MonoBehaviour, IUnityAdsLoadListener, IUnityAdsShowListener
{
  [SerializeField] string _androidAdUnitId = "Interstitial_Android";
  string _adUnitId;
 
  void Awake()
  {
    // Get the Ad Unit ID for the current platform:
    _adUnitId = _androidAdUnitId;
  }
 
  // Load content to the Ad Unit:
  public void LoadAd()
  {
    //load content AFTER initialization
    Debug.Log("Loading Ad: " + _adUnitId);
    Advertisement.Load(_adUnitId, this);
  }
 
  // Show the loaded content in the Ad Unit:
  public void ShowAd()
  {
    // Note that if the ad content wasn't previously loaded, this method will fail
    Debug.Log("Showing Ad: " + _adUnitId);
    Advertisement.Show(_adUnitId, this);
  }
 
  // Implement Load Listener and Show Listener interface methods: 
  public void OnUnityAdsAdLoaded(string adUnitId)
  {
    // Optionally execute code if the Ad Unit successfully loads content.
  }
 
  public void OnUnityAdsFailedToLoad(string _adUnitId, UnityAdsLoadError error, string message)
  {
    Debug.Log($"Error loading Ad Unit: {_adUnitId} - {error.ToString()} - {message}");
  }
 
  public void OnUnityAdsShowFailure(string _adUnitId, UnityAdsShowError error, string message)
  {
    Debug.Log($"Error showing Ad Unit {_adUnitId}: {error.ToString()} - {message}");
  }
 
  public void OnUnityAdsShowStart(string _adUnitId) { }
  public void OnUnityAdsShowClick(string _adUnitId) { }
  public void OnUnityAdsShowComplete(string _adUnitId, UnityAdsShowCompletionState showCompletionState) { }
}
