using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class RaceSelector : MonoBehaviour
{
    private Dropdown raceDropdown;
    private Button startGameButton;

    void Start()
    {
        // ������� ������� �� �� ������ � ��������
        raceDropdown = GameObject.Find("RaceDropdown").GetComponent<Dropdown>();
        startGameButton = GameObject.Find("StartGameButton").GetComponent<Button>();

        // ����������� ����� OnStartGame � ������
        startGameButton.onClick.AddListener(OnStartGame);
    }

    void OnStartGame()
    {
        int selectedRace = raceDropdown.value;
        PlayerPrefs.SetInt("SelectedRace", selectedRace);
        SceneManager.LoadScene("MainScene");
    }
}
