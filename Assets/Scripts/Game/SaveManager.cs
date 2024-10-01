using System;
using System.IO;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    private static SaveManager instance;

    private MapGenerator mapGenerator;
    private GameResourceManager gameResourceManager;
    private BuildingManager buildingManager;
    private UnitManager unitManager;

    public static string saveDirectory;
    public static string saveFilePath;

    private void Start()
    {
        mapGenerator = FindObjectOfType<MapGenerator>();
        gameResourceManager = FindObjectOfType<GameResourceManager>();
        buildingManager = FindObjectOfType<BuildingManager>();
        unitManager = FindObjectOfType<UnitManager>();

        saveDirectory = Path.Combine(Application.dataPath, "Saves");
        saveFilePath = Path.Combine(saveDirectory, "game_save.csv");

        CreateSaveFileIfNotExists();
    }

    private void Awake()
    {
        // Проверка на наличие уже существующего экземпляра
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);  // Сохраняем объект между сценами
        }
        else
        {
            Destroy(gameObject);  // Уничтожаем дубликат, если уже существует экземпляр
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
            File.Create(saveFilePath);
            Debug.Log("Save file created: " + saveFilePath);
        }
    }

    public void SaveGame()
    {
        using (StreamWriter writer = new StreamWriter(saveFilePath))
        {
            // Сохранение карты (тайлы и ресурсы)
            mapGenerator.SaveTilesToFile(writer);
            mapGenerator.SaveResourcesToFile(writer);

            // Сохранение всех ресурсов
            gameResourceManager.SavePlayerResources(writer);

            // Сохранение зданий
            buildingManager.SaveBuildingsToFile(writer);

            // Сохранение юнитов
            unitManager.SaveUnitsToFile(writer);
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

        using (StreamReader reader = new StreamReader(saveFilePath))
        {
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                string[] data = line.Split(',');

                switch (data[0])
                {
                    case "Tile":
                        mapGenerator.LoadTilesFromFile(reader, data);
                        break;
                    case "Resource":
                        mapGenerator.LoadResourcesFromFile(reader, data);
                        break;
                    case "PlayerResources":
                        gameResourceManager.LoadPlayerResources(reader, data);
                        break;
                    case "BuildingData":
                        buildingManager.LoadBuildingsFromFile(reader, data);
                        break;
                    case "UnitData":
                        unitManager.LoadUnitsFromFile(reader, data);
                        break;
                }
            }
        }

        Debug.Log("Игра загружена из файла: " + saveFilePath);
    }
}
