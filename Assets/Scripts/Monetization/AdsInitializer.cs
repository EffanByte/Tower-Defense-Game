using com.unity3d.mediation;
using UnityEngine;

public class AdsInitializer : MonoBehaviour
{
    [SerializeField]
    private string _androidAppKey;

    [SerializeField]
    private string _iosAppKey;

    private string appKey;

    [SerializeField]
    private string _userId; // optional, can be null or empty if you don’t have one

    void Awake()
    {
        // Pick correct app key depending on platform
    #if UNITY_ANDROID
        appKey = _androidAppKey;
    #elif UNITY_IOS
        appKey = _iosAppKey;
    #else
        appKey = _androidAppKey; // fallback
    #endif

        // Register for initialization callbacks *before* calling Init
        LevelPlay.OnInitSuccess += OnLevelPlayInitSuccess;
        LevelPlay.OnInitFailed += OnLevelPlayInitFailed;

        // Optionally: specify legacy ad formats if you still need them
        // For example, if Rewarded is still using legacy behavior
        LevelPlayAdFormat[] legacyFormats = null;
        // To use legacy formats:
        // legacyFormats = new[] { LevelPlayAdFormat.REWARDED };

        // Initialize LevelPlay
        if (!string.IsNullOrEmpty(_userId))
        {
            LevelPlay.Init(appKey, _userId, adFormats: legacyFormats);
        }
        else
        {
            LevelPlay.Init(appKey, userId: null, adFormats: legacyFormats);
        }
    }

    private void OnDestroy()
    {
        // Unregister to avoid memory leaks
        LevelPlay.OnInitSuccess -= OnLevelPlayInitSuccess;
        LevelPlay.OnInitFailed -= OnLevelPlayInitFailed;
    }

    private void OnLevelPlayInitSuccess(LevelPlayConfiguration config)
    {
        Debug.Log("LevelPlay SDK Initialization Complete. Configuration: " + config.ToString());

        // you can load ads i.e interstitial, rewarded

        var rewardedAd = new LevelPlayRewardedAd("AD UNIT ID HERE");
        rewardedAd.LoadAd();
    }

    private void OnLevelPlayInitFailed(LevelPlayInitError error)
    {
        Debug.LogError($"LevelPlay SDK Initialization Failed: {error.ToString()}");
        // Handle error (retry, fallback, show message, etc.)
    }
}
