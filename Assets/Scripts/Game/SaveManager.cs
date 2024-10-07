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
    private UnitManager unitManager;

    public static readonly string saveDirectory = Path.Combine(Application.dataPath, "Saves");
    public static readonly string saveFilePath = Path.Combine(saveDirectory, "game_save.csv");


    private void Awake()
    {
        // Проверка на наличие уже существующего экземпляра
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);  // Сохраняем объект между сценами
        }
        else
        {
            Destroy(gameObject);  // Уничтожаем дубликат, если уже существует экземпляр
        }
    }

    // Метод для обновления ссылок на менеджеры
    public void InitializeManagers()
    {
        mapGenerator = FindObjectOfType<MapGenerator>();
        gameResourceManager = FindObjectOfType<GameResourceManager>();
        buildingManager = FindObjectOfType<BuildingManager>();
        unitManager = FindObjectOfType<UnitManager>();

        if (mapGenerator == null || gameResourceManager == null || buildingManager == null || unitManager == null)
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
        // Проверяем, существует ли директория
        if (!Directory.Exists(saveDirectory))
        {
            Directory.CreateDirectory(saveDirectory); // Создаем директорию, если ее нет
        }

        // Проверяем, существует ли файл сохранения
        if (!File.Exists(saveFilePath))
        {
            // Создаем файл, если его нет
            File.Create(saveFilePath).Close(); // Закрываем поток после создания файла
            Debug.Log("Файл сохранения создан: " + saveFilePath);
        }
    }

    public void SaveGame()
    {
        CreateSaveFileIfNotExists();

        using (StreamWriter writer = new StreamWriter(saveFilePath))
        {
            // Сохранение карты (тайлы и ресурсы)
            if (mapGenerator != null)
            {
                mapGenerator.SaveTilesToFile(writer);
                mapGenerator.SaveResourcesToFile(writer);
            }

            // Сохранение всех ресурсов
            if (gameResourceManager != null)
            {
                gameResourceManager.SavePlayerResources(writer);
            }

            // Сохранение зданий
            if (buildingManager != null)
            {
                buildingManager.SaveBuildingsToFile(writer);
            }

            // Сохранение юнитов
            if (unitManager != null)
            {
                unitManager.SaveUnitsToFile(writer);
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
        using (StreamReader reader = new StreamReader(saveFilePath))
        {
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                string[] data = line.Split(',');

                switch (data[0])
                {
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
                        if (unitManager != null)
                        {
                            unitManager.LoadUnitsFromFile(data);
                        }
                        break;
                }
            }
        }

        Debug.Log("Игра загружена из файла: " + saveFilePath);
    }
}
