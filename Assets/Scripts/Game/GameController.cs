using UnityEngine;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    private void Start()
    {
        SaveManager.Instance.LoadGame();
    }
    void Update()
    {
        // Проверяем, нажата ли клавиша F1
        if (Input.GetKeyDown(KeyCode.F1))
        {
            ExitToMainMenu();
        }
    }

    // Метод для выхода в главное меню
    public void ExitToMainMenu()
    {
        SaveManager saveManager = FindObjectOfType<SaveManager>();

        if (saveManager != null)
        {
            saveManager.InitializeManagers();
            saveManager.SaveGame(); // Сохранение игры перед выходом
        }
        else
        {
            Debug.LogError("SaveManager не найден!");
        }

        // Загрузка главного меню
        SceneManager.LoadScene("MainMenu");
    }
}
