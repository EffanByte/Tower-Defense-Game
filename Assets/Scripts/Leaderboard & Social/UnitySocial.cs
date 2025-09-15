using UnityEngine;
using UnityEngine.SocialPlatforms;
using GooglePlayGames;

public class UnitySocial : MonoBehaviour {

    void Start () {
        PlayGamesPlatform.Activate();   
        Social.localUser.Authenticate(ProcessAuthentication);
        Social.ShowLeaderboardUI();
        DontDestroyOnLoad(this.gameObject);
    }

    // This function gets called when Authenticate completes
    // if the operation is successful, Social.localUser will contain data from the server.
    void ProcessAuthentication (bool status) {
        if (status)
            Debug.Log ("Authenticated");
        else
            Debug.Log("Failed to authenticate"); 
    }

}