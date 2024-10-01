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
        SceneManager.LoadScene("GameScene");
    }

    public void ContinueGame()
    {
        if (File.Exists(saveFile))
        {
            // Загружаем игровую сцену
            SceneManager.LoadScene("GameScene");

            // После загрузки сцены загружаем данные
            StartCoroutine(LoadGameAfterSceneLoaded());
        }
        else
        {
            Debug.Log("No saved game found!");
        }
    }

    private IEnumerator LoadGameAfterSceneLoaded()
    {
        // Ждем, пока сцена полностью загрузится
        yield return new WaitForSeconds(1f);

        // Загружаем сохраненную игру
        SaveManager saveManager = FindObjectOfType<SaveManager>();
        if (saveManager != null)
        {
            saveManager.LoadGame();
        }
    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Quit Game button pressed!");
    }
}
