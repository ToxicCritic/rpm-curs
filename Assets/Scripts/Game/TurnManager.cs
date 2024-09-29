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
    public BuildingManager buildingManager; // Обновленный BuildingManager
    public Button endTurnButton;
    public TMP_Text turnText;

    public TMP_Text gameOverText;
    public Image gameOverOutline;

    private int currentTurnIndex = 1; // Начинаем с 1
    private string[] playerNames = { "Орки", "Люди", "Нежить", "Эльфы" };

    private HashSet<int> activePlayers;

    // Константа для пути файла сохранения в папке проекта
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
        activePlayers = new HashSet<int> { 1, 2, 3, 4 }; // Все игроки активны в начале игры

        // Создание папки для сохранений, если она не существует
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
            EndTurn(); // Пропускаем ход, если игрок не активен
            return;
        }

        Debug.Log($"Starting turn for player {currentTurnIndex}");
        unitManagers[currentTurnIndex - 1].StartTurn(currentTurnIndex); // Индексация с 1
        buildingManager.SetPlayer(currentTurnIndex); // Обновление доступных зданий для текущего игрока
        buildingManager.StartTurnForBuildings(currentTurnIndex);
        playerResourceManagers[currentTurnIndex - 1].StartTurn(); // Обновление ресурсов для текущего игрока
        UpdateTurnText();
    }

    public void EndTurn()
    {
        Debug.Log($"Ending turn for player {currentTurnIndex}");
        unitManagers[currentTurnIndex - 1].EndTurn();
        buildingManager.EndTurnForBuildings(currentTurnIndex);
        playerResourceManagers[currentTurnIndex - 1].EndTurn(); // Завершение хода для текущего игрока
        currentTurnIndex = (currentTurnIndex % unitManagers.Count) + 1; // Переключение между всеми игроками

        // Пропускаем ходы неактивных игроков
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
            // Загрузка сцены главного меню
            UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
        }
    }

    void UpdateTurnText()
    {
        if (currentTurnIndex > 0 && currentTurnIndex <= playerNames.Length)
        {
            turnText.text = $"Ход: {playerNames[currentTurnIndex - 1]}";
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
            gameOverText.text = $"Игра окончена! Победитель: {playerNames[currentTurnIndex - 1]}";
            gameOverOutline.gameObject.SetActive(true);
            turnText.gameObject.SetActive(false);
            endTurnButton.gameObject.SetActive(false);
            Debug.Log($"Game over! Player {currentTurnIndex} has won the game!");
            buildingManager.DestroyPlayerBuildingsAndUnits(currentTurnIndex);
        }
    }

    // Метод для сохранения игры в файл
    public void SaveGameToFile(string filePath)
    {
        try
        {
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                // Сохранение текущего индекса хода
                writer.WriteLine($"CurrentTurnIndex,{currentTurnIndex}");

                // Сохранение активных игроков
                writer.WriteLine($"ActivePlayers,{string.Join(",", activePlayers)}");

// Сохранение данных юнитов через UnitManager
                foreach (var unitManager in unitManagers)
                {
                    writer.Write("UnitData,");
                    unitManager.SaveUnitsToFile(writer);
                }

                // Сохранение данных ресурсов через PlayerResourceManager

                foreach (var resourceManager in playerResourceManagers)
                {
                    writer.Write("ResourceData,");
                    resourceManager.SaveResourcesToFile(writer);
                }

                // Сохранение данных зданий через BuildingManager
                buildingManager.SaveBuildingsToFile(writer);
            }

            Debug.Log("Game saved to " + filePath);
        }
        catch (IOException ex)
        {
            Debug.LogError($"Error saving game to file: {ex.Message}");
        }
    }

    // Метод для загрузки игры из файла
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
                            // Загрузка данных юнитов
                            int playerIndex = int.Parse(data[0]);
                            unitManagers[playerIndex - 1].LoadUnitsFromFile(filePath);
                        }
                        else if (readingResources)
                        {
                            // Загрузка данных ресурсов
                            int playerIndex = int.Parse(data[0]);
                            playerResourceManagers[playerIndex - 1].LoadResourcesFromFile(filePath);
                        }
                        else if (readingBuildings)
                        {
                            // Загрузка данных зданий
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
