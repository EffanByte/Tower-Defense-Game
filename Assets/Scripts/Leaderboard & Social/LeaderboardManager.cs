// using UnityEngine;
// #if UNITY_ANDROID
// using GooglePlayGames;
// using GooglePlayGames.BasicApi;
// #endif
// using UnityEngine.SocialPlatforms;

// public class LeaderboardManager : MonoBehaviour
// {
//     public static LeaderboardManager Instance { get; private set; }

//     [Header("Google Play Leaderboard IDs")]
//     public string wavesLeaderboardId = "PASTE_WAVES_ID_HERE";
//     public string killsLeaderboardId = "PASTE_KILLS_ID_HERE";

//     void Awake()
//     {
//         if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
//         else { Destroy(gameObject); }
//     }

//     void Start() { AuthenticatePlayer(); }

//     public void AuthenticatePlayer()
//     {
// #if UNITY_ANDROID
//         // Activate Google Play Games platform before authentication
//         PlayGamesPlatform.Activate();

//         // Authenticate using Google Play Games API
//         PlayGamesPlatform.Instance.Authenticate(SignInInteractivity.CanPromptOnce, (SignInStatus status) => {
//             if (status == SignInStatus.Success)
//             {
//                 Debug.Log("Player successfully authenticated with Google Play Games.");
//             }
//             else
//             {
//                 Debug.LogError("Player authentication failed with status: " + status);
//             }
//         });

// #else
//         // Fallback for non-Android platforms (e.g., iOS, Editor)
//         Social.localUser.Authenticate(success =>
//         {
//             if (success)
//             {
//                 Debug.Log("Player successfully authenticated.");
//             }
//             else
//             {
//                 Debug.LogError("Player authentication failed.");
//             }
//         });
// #endif
//     }

//     public void ReportWaveScore(int waveNumber)
//     {
//         if (Social.localUser.authenticated && !string.IsNullOrEmpty(wavesLeaderboardId))
//         {
//             Social.ReportScore(waveNumber, wavesLeaderboardId, success =>
//             {
//                 if (success) 
//                 {
//                     Debug.Log($"Successfully reported wave score: {waveNumber}");
//                 }
//                 else 
//                 {
//                     Debug.LogError("Failed to report wave score.");
//                 }
//             });
//         }
//     }

//     public void ReportKillScore(int totalKills)
//     {
//         if (Social.localUser.authenticated && !string.IsNullOrEmpty(killsLeaderboardId))
//         {
//             Social.ReportScore(totalKills, killsLeaderboardId, success =>
//             {
//                 if (success) 
//                 {
//                     Debug.Log($"Successfully reported kill score: {totalKills}");
//                 }
//                 else 
//                 {
//                     Debug.LogError("Failed to report kill score.");
//                 }
//             });
//         }
//     }

//     public void ShowLeaderboardsUI()
//     {
//         if (Social.localUser.authenticated) 
//         {
//             Social.ShowLeaderboardUI();
//         }
//     }
// }
