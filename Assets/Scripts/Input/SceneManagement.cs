using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManagement : MonoBehaviour
{

    void Start()
    {
        DontDestroyOnLoad(gameObject);
    }
    public void StartGame()
    {
        SceneManager.LoadScene("GameScene");
    }

    public void GoToMenu()
    {
        SceneManager.LoadScene("TitleScene");
    }
    public void GoToCosmetics()
    {
        SceneManager.LoadScene("Cosmetics");
    }
}
