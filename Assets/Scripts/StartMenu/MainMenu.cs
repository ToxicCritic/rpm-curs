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
        // ���������, ���������� �� ���� ����������
        if (File.Exists(saveFile))
        {
            continueButton.interactable = true;
            continueButton.onClick.AddListener(ContinueGame);
        }
        else
        {
            continueButton.interactable = false; // ��������� ������ "����������", ���� ���������� ���
        }
    }

    public void StartNewGame()
    {
        // ������� ����������� �������� (���� ����)
        if (File.Exists(saveFile))
        {
            File.Delete(saveFile);
            Debug.Log("Existing save file deleted. Starting new game.");
        }

        // ��������� ������� �����
        SceneManager.LoadScene("GameScene");
    }

    public void ContinueGame()
    {
        if (File.Exists(saveFile))
        {
            // ��������� ������� �����
            SceneManager.LoadScene("GameScene");

            // ����� �������� ����� ��������� ����������� ������
            StartCoroutine(LoadGameAfterSceneLoaded());
        }
        else
        {
            Debug.Log("No saved game found!");
        }
    }

    private IEnumerator LoadGameAfterSceneLoaded()
    {
        // ���� ���������� �������� �����
        yield return new WaitUntil(() => SceneManager.GetActiveScene().name == "GameScene");

        // ������� SaveManager �� �����
        SaveManager saveManager = FindObjectOfType<SaveManager>();
        if (saveManager != null)
        {
            // ��������� ������
            saveManager.LoadGame();
        }
        else
        {
            Debug.LogError("SaveManager �� ������ �� �����!");
        }
    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Quit Game button pressed!");
    }
}
