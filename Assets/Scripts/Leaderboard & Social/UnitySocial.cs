
using UnityEngine;
using UnityEngine.SocialPlatforms;
using GooglePlayGames;

public class UnitySocial : MonoBehaviour {

    void Start () {
        DontDestroyOnLoad(this.gameObject);
        PlayGamesPlatform.Activate();   
        Social.localUser.Authenticate(ProcessAuthentication);
    }

    // This function gets called when Authenticate completes
    // if the operation is successful, Social.localUser will contain data from the server.
    void ProcessAuthentication (bool status) {
        if (status)
            Debug.Log ("Authenticated");
        else
            Debug.Log("Failed to authenticate"); 
    }
    public void PostWaveScore(int waves)
    {
        PlayGamesPlatform.Instance.ReportScore(waves, "WAVE LEADERBOARD ID HERE", (bool success) =>
        {
            Debug.Log("wave count posted successfully");
        }   );
    }
        public void PostKillScore(int waves)
    {
        PlayGamesPlatform.Instance.ReportScore(waves, "KILL COUNT LEADERBOARD ID HERE", (bool success) =>
        {
            Debug.Log("kill count posted successfully");
        }   );
    }
    public void DisplayLeaderBoard()
    {
        PlayGamesPlatform.Instance.ShowLeaderboardUI("LEADERBOARD ID HERE");
    }
}