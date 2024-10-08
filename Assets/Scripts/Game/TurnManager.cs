using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine.SceneManagement;

public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance { get; private set; }

    public List<UnitManager> unitManagers;
    public List<PlayerResourceManager> playerResourceManagers;
    public BuildingManager buildingManager; // ����������� BuildingManager
    public Button endTurnButton;
    public TMP_Text turnText;

    public TMP_Text gameOverText;
    public Image gameOverOutline;

    public int currentTurnIndex = 1; // �������� � 1
    private string[] playerNames = { "����", "����", "������", "�����" };

    public HashSet<int> activePlayers;

    // ��������� ��� ���� ����� ���������� � ����� �������
    private static readonly string saveDirectory = Path.Combine(Application.dataPath, "Saves");
    private static readonly string saveFile = Path.Combine(saveDirectory, "game_save.csv");

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        activePlayers = new HashSet<int> { 1, 2, 3, 4 }; // ��� ������ ������� � ������ ����

        // �������� ����� ��� ����������, ���� ��� �� ����������
        if (!Directory.Exists(saveDirectory))
        {
            Directory.CreateDirectory(saveDirectory);
            Debug.Log($"Save directory created at {saveDirectory}");
        }
    }

    void Start()
    {
        if (unitManagers.Count != 4 || playerResourceManagers.Count != 4)
        {
            Debug.LogError("UnitManagers and PlayerResourceManagers count should be equal to 4.");
        }
        currentTurnIndex = PlayerPrefs.GetInt("SelectedRace", 1);
        StartTurn();
        endTurnButton.onClick.AddListener(EndTurn);
    }

    void StartTurn()
    {
        if (!activePlayers.Contains(currentTurnIndex))
        {
            EndTurn(); // ���������� ���, ���� ����� �� �������
            return;
        }

        Debug.Log($"Starting turn for player {currentTurnIndex}");
        unitManagers[currentTurnIndex - 1].StartTurn(currentTurnIndex); // ���������� � 1
        buildingManager.SetPlayer(currentTurnIndex); // ���������� ��������� ������ ��� �������� ������
        buildingManager.StartTurnForBuildings(currentTurnIndex);
        playerResourceManagers[currentTurnIndex - 1].StartTurn(); // ���������� �������� ��� �������� ������
        UpdateTurnText();
    }

    public void EndTurn()
    {
        Debug.Log($"Ending turn for player {currentTurnIndex}");
        unitManagers[currentTurnIndex - 1].EndTurn();
        buildingManager.EndTurnForBuildings(currentTurnIndex);
        playerResourceManagers[currentTurnIndex - 1].EndTurn(); // ���������� ���� ��� �������� ������
        currentTurnIndex = (currentTurnIndex % unitManagers.Count) + 1; // ������������ ����� ����� ��������

        // ���������� ���� ���������� �������
        while (!activePlayers.Contains(currentTurnIndex))
        {
            currentTurnIndex = (currentTurnIndex % unitManagers.Count) + 1;
        }

        Debug.Log($"Next turn index: {currentTurnIndex}");
        StartTurn();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            EndTurn();
        }
    }

    void UpdateTurnText()
    {
        if (currentTurnIndex > 0 && currentTurnIndex <= playerNames.Length)
        {
            turnText.text = $"���: {playerNames[currentTurnIndex - 1]}";
            Debug.Log($"Updated turn text: {turnText.text}");
        }
        else
        {
            Debug.LogError($"Invalid turn index: {currentTurnIndex}");
        }
    }

    public PlayerResourceManager GetCurrentPlayerResourceManager()
    {
        return playerResourceManagers[currentTurnIndex - 1];
    }

    public int GetCurrentPlayerIndex()
    {
        return currentTurnIndex;
    }

    public UnitManager GetUnitManagerForPlayer(int playerIndex)
    {
        return unitManagers[playerIndex - 1];
    }

    public void DeactivatePlayer(int playerIndex)
    {
        if (activePlayers == null)
        {
            Debug.LogError("ActivePlayers is null before attempting to deactivate a player.");
        }
        else
        {
            activePlayers.Remove(playerIndex);
            Debug.Log($"Player {playerNames[playerIndex - 1]} has been deactivated.");
        }

        if (activePlayers.Count == 1)
        {
            gameOverText.text = $"���� ��������! ����������: {playerNames[currentTurnIndex - 1]}";
            gameOverOutline.gameObject.SetActive(true);
            turnText.gameObject.SetActive(false);
            endTurnButton.gameObject.SetActive(false);
            Debug.Log($"Game over! Player {currentTurnIndex} has won the game!");
            buildingManager.DestroyPlayerBuildingsAndUnits(currentTurnIndex);
        }
    }

    public void SaveTurnToFile(StreamWriter writer)
    {
        writer.WriteLine($"TurnData,{currentTurnIndex}");
    }

    public void LoadTurnFromFile(string[] data)
    {
        if (data[0] == "TurnData")
        {
            PlayerPrefs.SetInt("SelectedRace", int.Parse(data[1]));
            Debug.Log($"��� ������ ��������: {currentTurnIndex}");
        }
    }

}
