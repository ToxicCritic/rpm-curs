using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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

    private int currentTurnIndex = 1; // �������� � 1
    private string[] playerNames = { "����", "����", "������", "�����" };

    private HashSet<int> activePlayers;

    // ��������� ��� ���� ����� ���������� � ����� �������
    private static readonly string saveDirectory = Path.Combine(Application.dataPath, "Saves");
    private static readonly string saveFile = Path.Combine(saveDirectory, "game_save.csv");

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
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

        if (Input.GetKeyDown(KeyCode.F1))
        {
            if (File.Exists(saveFile))
            {
                File.Delete(saveFile);
            }
            SaveGameToFile(saveFile);
            // �������� ����� �������� ����
            UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
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
        activePlayers.Remove(playerIndex);
        Debug.Log($"Player {playerNames[playerIndex - 1]} has been deactivated.");

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

    // ����� ��� ���������� ���� � ����
    public void SaveGameToFile(string filePath)
    {
        try
        {
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                // ���������� �������� ������� ����
                writer.WriteLine($"CurrentTurnIndex,{currentTurnIndex}");

                // ���������� �������� �������
                writer.WriteLine($"ActivePlayers,{string.Join(",", activePlayers)}");

// ���������� ������ ������ ����� UnitManager
                foreach (var unitManager in unitManagers)
                {
                    writer.Write("UnitData,");
                    unitManager.SaveUnitsToFile(writer);
                }

                // ���������� ������ �������� ����� PlayerResourceManager

                foreach (var resourceManager in playerResourceManagers)
                {
                    writer.Write("ResourceData,");
                    resourceManager.SaveResourcesToFile(writer);
                }

                // ���������� ������ ������ ����� BuildingManager
                buildingManager.SaveBuildingsToFile(writer);
            }

            Debug.Log("Game saved to " + filePath);
        }
        catch (IOException ex)
        {
            Debug.LogError($"Error saving game to file: {ex.Message}");
        }
    }

    // ����� ��� �������� ���� �� �����
    public void LoadGameFromFile(string filePath)
    {
        if (!File.Exists(filePath))
        {
            Debug.LogError("Save file not found at " + filePath);
            return;
        }

        using (StreamReader reader = new StreamReader(filePath))
        {
            string line;
            bool readingUnits = false, readingResources = false, readingBuildings = false;

            while ((line = reader.ReadLine()) != null)
            {
                string[] data = line.Split(',');

                switch (data[0])
                {
                    case "CurrentTurnIndex":
                        currentTurnIndex = int.Parse(data[1]);
                        break;
                    case "ActivePlayers":
                        activePlayers = new HashSet<int>(data[1].Split(',').Select(int.Parse));
                        break;
                    case "UnitData":
                        readingUnits = true;
                        readingResources = false;
                        readingBuildings = false;
                        break;
                    case "ResourceData":
                        readingUnits = false;
                        readingResources = true;
                        readingBuildings = false;
                        break;
                    case "BuildingData":
                        readingUnits = false;
                        readingResources = false;
                        readingBuildings = true;
                        break;
                    default:
                        if (readingUnits)
                        {
                            // �������� ������ ������
                            int playerIndex = int.Parse(data[0]);
                            unitManagers[playerIndex - 1].LoadUnitsFromFile(filePath);
                        }
                        else if (readingResources)
                        {
                            // �������� ������ ��������
                            int playerIndex = int.Parse(data[0]);
                            playerResourceManagers[playerIndex - 1].LoadResourcesFromFile(filePath);
                        }
                        else if (readingBuildings)
                        {
                            // �������� ������ ������
                            buildingManager.LoadBuildingsFromFile(filePath);
                        }
                        break;
                }
            }
        }

        UpdateTurnText();
        Debug.Log("Game loaded from " + filePath);
    }
}
