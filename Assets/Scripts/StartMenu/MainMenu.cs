using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.IO;

public class MainMenu : MonoBehaviour
{
    public Button continueButton; 

    private static readonly string saveDirectory = Path.Combine(Application.dataPath, "Saves");
    private static readonly string saveFile = Path.Combine(saveDirectory, "game_save.csv");

    void Start()
    {
        // Проверяем, существует ли сохранение, и активируем кнопку "Продолжить", если оно есть
        if (File.Exists(saveFile))
        {
            continueButton.interactable = true;
            continueButton.onClick.AddListener(ContinueGame);
        }
        else
        {
            continueButton.interactable = false; // Отключаем кнопку "Продолжить", если сохранения нет
        }
    }

    public void StartNewGame()
    {
        // Удаляем сохраненный прогресс (если есть)
        if (File.Exists(saveFile))
        {
            File.Delete(saveFile);
            Debug.Log("Existing save file deleted. Starting new game.");
        }

        SceneManager.LoadScene("RaceSelector");
    }

    public void ContinueGame()
    {
        // Проверка наличия сохраненной игры
        if (File.Exists(saveFile))
        {

            TurnManager.Instance.LoadGameFromFile(saveFile);
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
        // Здесь вы можете добавить загрузку сцены или функциональность для отображения статистики
    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Quit Game button pressed!");
    }
}
