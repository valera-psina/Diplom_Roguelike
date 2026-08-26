using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuActions : MonoBehaviour
{
    public void PlayGame()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.StartNewGame();
        else
            Debug.LogError("GameManager.Instance не найден!");
    }

    public void GoToMenu()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.LoadMenu();
        else
            SceneManager.LoadScene(0);
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}