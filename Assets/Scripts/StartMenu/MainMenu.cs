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
        // Проверяем, существует ли файл сохранения
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

        // Загружаем игровую сцену
        SceneManager.LoadScene("GameScene");
    }

    public void ContinueGame()
    {
        if (File.Exists(saveFile))
        {
            // Подписываемся на событие загрузки сцены
            //SceneManager.sceneLoaded += OnGameSceneLoaded;

            // Загружаем игровую сцену
             SceneManager.LoadScene("GameScene");

        }
        else
        {
            Debug.Log("No saved game found!");
        }
    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Quit Game button pressed!");
    }
}
