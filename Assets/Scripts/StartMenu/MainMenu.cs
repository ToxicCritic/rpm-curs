using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.IO;
using System.Collections;

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
            // Загружаем игровую сцену
            SceneManager.LoadScene("GameScene");

            // После загрузки сцены загружаем сохраненные данные
            StartCoroutine(LoadGameAfterSceneLoaded());
        }
        else
        {
            Debug.Log("No saved game found!");
        }
    }

    private IEnumerator LoadGameAfterSceneLoaded()
    {
        // Ждем завершения загрузки сцены
        yield return new WaitUntil(() => SceneManager.GetActiveScene().name == "GameScene");

        // Находим SaveManager на сцене
        SaveManager saveManager = FindObjectOfType<SaveManager>();
        if (saveManager != null)
        {
            // Загружаем данные
            saveManager.LoadGame();
        }
        else
        {
            Debug.LogError("SaveManager не найден на сцене!");
        }
    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Quit Game button pressed!");
    }
}
