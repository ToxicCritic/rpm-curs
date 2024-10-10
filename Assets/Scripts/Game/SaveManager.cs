using System;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    private MapGenerator mapGenerator;
    private GameResourceManager gameResourceManager;
    private BuildingManager buildingManager;
    public UnitManager[] unitManagers = new UnitManager[4];
    private TurnManager turnManager;

    public static readonly string saveDirectory = Path.Combine(Application.dataPath, "Saves");
    public static readonly string saveFilePath = Path.Combine(saveDirectory, "game_save.csv");

    private void Awake()
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
    }

    public void InitializeManagers()
    {
        mapGenerator = FindObjectOfType<MapGenerator>();
        gameResourceManager = FindObjectOfType<GameResourceManager>();
        buildingManager = FindObjectOfType<BuildingManager>();
        turnManager = FindObjectOfType<TurnManager>();

        for (int i = 0; i < 4; i++)
        {
            unitManagers[i] = GameObject.Find($"UnitManager_Player{i}")?.GetComponent<UnitManager>();
        }

        if (mapGenerator == null || gameResourceManager == null || buildingManager == null || unitManagers[0] == null)
        {
            Debug.LogWarning("Не удалось найти все менеджеры на сцене!");
        }
        else
        {
            Debug.Log("Все менеджеры успешно инициализированы.");
        }
    }

    private void CreateSaveFileIfNotExists()
    {
        if (!Directory.Exists(saveDirectory))
        {
            Directory.CreateDirectory(saveDirectory); 
        }

        if (!File.Exists(saveFilePath))
        {
            File.Create(saveFilePath).Close(); 
            Debug.Log("Файл сохранения создан: " + saveFilePath);
        }
    }

    public void SaveGame()
    {
        CreateSaveFileIfNotExists();

        using (StreamWriter writer = new StreamWriter(saveFilePath))
        {          
            if (mapGenerator != null)
            {
                mapGenerator.SaveTilesToFile(writer);
                mapGenerator.SaveResourcesToFile(writer);
            }

            if (gameResourceManager != null)
            {
                gameResourceManager.SavePlayerResources(writer);
            }

            if (buildingManager != null)
            {
                buildingManager.SaveBuildingsToFile(writer);
            }

            foreach (var unitManager in unitManagers)
            {
                if (unitManager != null)
                {
                    unitManager.SaveUnitsToFile(writer);
                }
            }

            if (turnManager != null)
            {
                turnManager.SaveTurnToFile(writer);
            }
        }

        Debug.Log("Игра сохранена в файл: " + saveFilePath);
    }

    public void LoadGame()
    {
        if (!File.Exists(saveFilePath))
        {
            Debug.LogError("Файл сохранения не найден: " + saveFilePath);
            return;
        }

        InitializeManagers();
        turnManager.activePlayers.Clear();

        using (StreamReader reader = new StreamReader(saveFilePath))
        {
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                string[] data = line.Split(',');

                switch (data[0])
                {
                    case "TurnData":
                        if (turnManager != null)
                        {
                            turnManager.LoadTurnFromFile(data);
                        }
                        break;
                    case "ActivePlayer":
                        if (turnManager != null)
                        {
                            turnManager.LoadTurnFromFile(data);
                        }
                        break;
                    case "Tile":
                        if (mapGenerator != null)
                        {
                            mapGenerator.LoadTilesFromFile(data);
                        }
                        break;
                    case "Resource":
                        if (mapGenerator != null)
                        {
                            mapGenerator.LoadResourcesFromFile(data);
                        }
                        break;
                    case "PlayerResources":
                        if (gameResourceManager != null)
                        {
                            gameResourceManager.LoadPlayerResources(data);
                        }
                        break;
                    case "BuildingData":
                        if (buildingManager != null)
                        {
                            buildingManager.LoadBuildingsFromFile(data);
                        }
                        break;
                    case "UnitData":
                        int playerIndex = int.Parse(data[2]);
                        if (playerIndex >= 0 && playerIndex < unitManagers.Length && unitManagers[playerIndex] != null)
                        {
                            unitManagers[playerIndex].LoadUnitsFromFile(data);
                        }
                        else
                        {
                            Debug.LogError($"Некорректный индекс игрока: {playerIndex} при загрузке юнита.");
                        }
                        break;
                }
            }
        }

        Debug.Log("Игра загружена из файла: " + saveFilePath);
    }
}
