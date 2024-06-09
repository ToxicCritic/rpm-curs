using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void StartNewGame()
    {
        // Сбрасываем сохраненный прогресс (если есть)
        PlayerPrefs.DeleteAll();
        SceneManager.LoadScene("GameScene");
    }

    public void ContinueGame()
    {
        // Проверка наличия сохраненной игры
        if (PlayerPrefs.HasKey("SavedGame"))
        {
            SceneManager.LoadScene("GameScene");
        }
        else
        {
            Debug.Log("No saved game found!");
        }
    }

    public void ViewStatistics()
    {
        // Загрузите сцену или откройте окно со статистикой
        Debug.Log("View Statistics button pressed!");
    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Quit Game button pressed!");
    }
}
