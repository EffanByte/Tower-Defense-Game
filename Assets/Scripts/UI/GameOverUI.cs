using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverUI : MonoBehaviour
{
    [SerializeField] private GameObject panel; // assign the UI root panel in Inspector

    void Awake()
    {
        panel.SetActive(false); // hidden at start
    }

    void OnEnable()
    {
        TrySubscribeOrQueue();
    }

    void OnDisable()
    {
        if (HealthManager.Instance != null)
            HealthManager.Instance.OnDeath -= Show;
    }

    void TrySubscribeOrQueue()
    {
        var hm = HealthManager.Instance;
        if (hm != null)
            hm.OnDeath += Show; // immediate draw
        else
            StartCoroutine(WaitForHMThenSubscribe());
    }

    System.Collections.IEnumerator WaitForHMThenSubscribe()
    {
        while (HealthManager.Instance == null) yield return null;
        TrySubscribeOrQueue();
    }
    void Show()
    {
        panel.SetActive(true);
        // Unlock cursor if you locked it during gameplay
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    // Button callback
    public void Retry()
    {
        Time.timeScale = 1f; // resume before reload
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // Button callback
    public void ReturnToMenu()
    {
        Time.timeScale = 1f; // resume before changing scene
        SceneManager.LoadScene("MainMenu"); // <-- name of your menu scene
    }
}
