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
        if (Input.GetKeyDown(KeyCode.F1))
        {
            ExitToMainMenu();
        }
    }

    public void ExitToMainMenu()
    {
        SaveManager saveManager = FindObjectOfType<SaveManager>();

        if (saveManager != null)
        {
            saveManager.InitializeManagers();
            saveManager.SaveGame(); 
        }
        else
        {
            Debug.LogError("SaveManager не найден!");
        }

        SceneManager.LoadScene("MainMenu");
    }
}
