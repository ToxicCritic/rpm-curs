using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void StartNewGame()
    {
        // ���������� ����������� �������� (���� ����)
        PlayerPrefs.DeleteAll();
        SceneManager.LoadScene("GameScene");
    }

    public void ContinueGame()
    {
        // �������� ������� ����������� ����
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
        // ��������� ����� ��� �������� ���� �� �����������
        Debug.Log("View Statistics button pressed!");
    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Quit Game button pressed!");
    }
}
