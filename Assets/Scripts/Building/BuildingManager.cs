using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using static Building;

public class BuildingManager : MonoBehaviour
{
    public GameObject[] orcBuildings;
    public GameObject[] elfBuildings;
    public GameObject[] humanBuildings;
    public GameObject[] undeadBuildings;

    private GameObject[] currentBuildings;
    private GameObject selectedBuildingPrefab;

    public Transform buttonPanel;
    public Button buildingButtonPrefab;

    public LayerMask obstacleLayerMask;
    public int mapWidth = 30;
    public int mapHeight = 30;

    public GameObject tileHighlighterPrefab;
    private GameObject tileHighlighterInstance;

    public GameObject alwaysOnHighlighterPrefab;
    private GameObject alwaysOnHighlighterInstance;

    private GameObject selectedBuildingInstance;

    private bool isUnitPanelActive = false;

    public UnitPanelManager unitPanelManager;

    public int currentPlayerIndex { get; set; }

    public List<Building> playerBuildings = new List<Building>();

    public GameObject orcFortressPrefab;
    public GameObject elfFortressPrefab;
    public GameObject humanFortressPrefab;
    public GameObject undeadFortressPrefab;


    void Start()
    {
        tileHighlighterInstance = Instantiate(tileHighlighterPrefab);
        tileHighlighterInstance.SetActive(false);

        alwaysOnHighlighterInstance = Instantiate(alwaysOnHighlighterPrefab);
        alwaysOnHighlighterInstance.SetActive(true);

        SetPlayer(1);
    }

    public void SelectBuilding(int index)
    {
        if (index >= 0 && index < currentBuildings.Length)
        {
            selectedBuildingPrefab = currentBuildings[index];
            tileHighlighterInstance.SetActive(true);
            alwaysOnHighlighterInstance.SetActive(false);
        }
    }

    public void SetPlayer(int playerIndex)
    {
        currentPlayerIndex = playerIndex;

        switch (playerIndex)
        {
            case 1:
                currentBuildings = orcBuildings;
                break;
            case 2:
                currentBuildings = humanBuildings;
                break;
            case 3:
                currentBuildings = undeadBuildings;
                break;
            case 4:
                currentBuildings = elfBuildings;
                break;
            default:
                currentBuildings = null;
                break;
        }
        UpdateBuildPanel();
    }

    void UpdateBuildPanel()
    {
        foreach (Transform child in buttonPanel)
        {
            Destroy(child.gameObject);
        }

        if (currentBuildings == null) return;

        int numberOfBuildings = currentBuildings.Length;
        float panelWidth = buttonPanel.GetComponent<RectTransform>().rect.width;
        float buttonWidth = buildingButtonPrefab.GetComponent<RectTransform>().rect.width;
        float spacing = (panelWidth - (numberOfBuildings * buttonWidth)) / (numberOfBuildings + 1);

        for (int i = 0; i < numberOfBuildings; i++)
        {
            GameObject building = currentBuildings[i];
            Button button = Instantiate(buildingButtonPrefab, buttonPanel);
            int index = i;

            button.onClick.AddListener(() => SelectBuilding(index));
            Image buttonImage = button.GetComponent<Image>();

            SpriteRenderer buildingSpriteRenderer = building.GetComponent<SpriteRenderer>();
            if (buildingSpriteRenderer != null)
            {
                buttonImage.sprite = buildingSpriteRenderer.sprite;
            }

            float xPos = spacing * (i + 1) + buttonWidth * i;
            button.GetComponent<RectTransform>().anchoredPosition = new Vector2(xPos, 0);
        }
    }

    void Update()
    {
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0;
        Vector3Int gridPosition = Vector3Int.FloorToInt(mousePosition);
        gridPosition.z = 0;

        // Обновление позиции хайлайтера (подсветки)
        if (alwaysOnHighlighterInstance.activeSelf)
        {
            Vector3 alwaysOnHighlighterPosition = new Vector3(gridPosition.x + 0.5f, gridPosition.y + 0.5f, -0.2f);
            alwaysOnHighlighterInstance.transform.position = alwaysOnHighlighterPosition;
        }

        if (tileHighlighterInstance.activeSelf && !isUnitPanelActive)
        {
            Vector3 placePosition = new Vector3(gridPosition.x + 0.5f, gridPosition.y + 0.5f, -0.1f);
            tileHighlighterInstance.transform.position = placePosition;
        }

        // Проверка на возможность установки здания
        if (selectedBuildingPrefab != null && Input.GetMouseButtonDown(0))
        {
            if (CanPlaceBuilding(gridPosition) && IsWithinMapBounds(gridPosition) && IsInPlayerQuarter(gridPosition))
            {
                BuildingCost cost = selectedBuildingPrefab.GetComponent<BuildingCost>();
                if (cost != null)
                {
                    PlayerResourceManager currentPlayerResourceManager = TurnManager.Instance.GetCurrentPlayerResourceManager();
                    if (currentPlayerResourceManager.CanAfford(cost.wood, cost.stone, 0))
                    {
                        bool woodSpent = currentPlayerResourceManager.SpendResource("wood", cost.wood);
                        bool stoneSpent = currentPlayerResourceManager.SpendResource("stone", cost.stone);

                        if (woodSpent && stoneSpent)
                        {
                            Vector3 spawnPosition = new Vector3(gridPosition.x + 0.5f, gridPosition.y + 0.5f, -0.1f);

                            // Инстанцируем здание и сохраняем позицию
                            GameObject buildingObject = Instantiate(selectedBuildingPrefab, spawnPosition, Quaternion.identity);
                            Building building = buildingObject.GetComponent<Building>();

                            // Сохраняем позицию здания в его поля positionX и positionY
                            building.positionX = spawnPosition.x;
                            building.positionY = spawnPosition.y;

                            // Обнуляем выбор и скрываем хайлайтеры
                            selectedBuildingPrefab = null;
                            tileHighlighterInstance.SetActive(false);
                            alwaysOnHighlighterInstance.SetActive(true);
                        }
                    }
                    else
                    {
                        Debug.Log("Not enough resources to place building.");
                    }
                }
            }
            else
            {
                Debug.Log("Cannot place building, it is outside the map bounds, position is occupied, or not in the correct quarter.");
            }
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            CancelBuildingPlacement();
        }
    }

    public bool CreateBuilding(int buildingIndex, Vector3Int gridPosition)
    {
        // Проверка на возможность установки здания
        if (CanPlaceBuilding(gridPosition) && IsWithinMapBounds(gridPosition) && IsInPlayerQuarter(gridPosition))
        {
            // Получаем префаб здания
            GameObject buildingPrefab = currentBuildings[buildingIndex];
            BuildingCost cost = buildingPrefab.GetComponent<BuildingCost>();
            PlayerResourceManager currentPlayerResourceManager = TurnManager.Instance.GetCurrentPlayerResourceManager();


            // Создаем здание
            Vector3 spawnPosition = new Vector3(gridPosition.x + 0.5f, gridPosition.y + 0.5f, -0.1f);
            GameObject buildingObject = Instantiate(buildingPrefab, spawnPosition, Quaternion.identity);
            Building building = buildingObject.GetComponent<Building>();
            building.positionX = spawnPosition.x;
            building.positionY = spawnPosition.y;

            // Регистрируем здание
            RegisterBuilding(building);

            return true; // Успешное создание здания

        }
        else
        {
            Debug.Log("Cannot place building.");
        }

        return false; // Ошибка создания здания
    }


    void CancelBuildingPlacement()
    {
        selectedBuildingPrefab = null;
        tileHighlighterInstance.SetActive(false);
        alwaysOnHighlighterInstance.SetActive(true);
        Debug.Log("Building placement cancelled.");
    }

    bool CanPlaceBuilding(Vector3Int position)
    {
        Vector2 checkPosition = new Vector2(position.x + 0.5f, position.y + 0.5f);
        Collider2D[] colliders = Physics2D.OverlapBoxAll(checkPosition, new Vector2(1, 1), 0, obstacleLayerMask);
        return colliders.Length == 0;
    }

    bool IsWithinMapBounds(Vector3Int position)
    {
        return position.x >= 0 && position.y >= 0 && position.x < mapWidth && position.y < mapHeight;
    }

    bool IsInPlayerQuarter(Vector3Int position)
    {
        int halfWidth = mapWidth / 2;
        int halfHeight = mapHeight / 2;

        switch (currentPlayerIndex)
        {
            case 1:
                return position.x <= halfWidth && position.y <= halfHeight;
            case 2:
                return position.x <= halfWidth && position.y >= halfHeight;
            case 3:
                return position.x >= halfWidth && position.y >= halfHeight;
            case 4:
                return position.x >= halfWidth && position.y <= halfHeight;
            default:
                return false;
        }
    }

    public void ShowUnitCreationPanel(Building building)
    {
        if (building.CanProduceUnit())
        {
            unitPanelManager.ShowUnitCreationPanel(building);
            HideBuildPanel();
        }
    }

    public void HideBuildPanel()
    {
        buttonPanel.gameObject.SetActive(false);
    }

    public void ShowBuildPanel()
    {
        buttonPanel.gameObject.SetActive(true);
    }

    public void RegisterBuilding(Building building)
    {
        if (!playerBuildings.Contains(building))
        {
            playerBuildings.Add(building);
        }
    }

    public void UnregisterBuilding(Building building)
    {
        if (playerBuildings.Contains(building))
        {
            playerBuildings.Remove(building);
        }

        if (building.buildingType == Building.BuildingType.Fortress)
        {
            TurnManager.Instance.DeactivatePlayer(building.playerIndex);
            DestroyPlayerBuildingsAndUnits(building.playerIndex); // Уничтожаем здания и юниты игрока
        }
    }

    public void DestroyPlayerBuildingsAndUnits(int playerIndex)
    {
        List<Building> buildingsToDestroy = new List<Building>();
        foreach (var building in playerBuildings)
        {
            if (building.playerIndex == playerIndex)
            {
                buildingsToDestroy.Add(building);
            }
        }

        foreach (var building in buildingsToDestroy)
        {
            playerBuildings.Remove(building);
            Destroy(building.gameObject);
        }

        UnitManager unitManager = FindObjectOfType<UnitManager>();
        List<Unit> unitsToDestroy = new List<Unit>();
        foreach (var unit in unitManager.units)
        {
            if (unit.playerIndex == playerIndex)
            {
                unitsToDestroy.Add(unit);
            }
        }

        foreach (var unit in unitsToDestroy)
        {
            unitManager.UnregisterUnit(unit);
            Destroy(unit.gameObject);
        }
    }

    public void StartTurnForBuildings(int playerIndex)
    {
        foreach (var building in playerBuildings)
        {
            if (building.playerIndex == playerIndex)
            {
                building.StartTurn();
            }
        }
    }

    public void EndTurnForBuildings(int playerIndex)
    {
        foreach (var building in playerBuildings)
        {
            if (building.playerIndex == playerIndex)
            {
                building.EndTurn();
            }
        }
    }

    public void SaveBuildingsToFile(StreamWriter writer)
    {
        foreach (var building in playerBuildings)
        {
            writer.Write("BuildingData,");
            writer.WriteLine($"{(int)building.buildingType},{building.playerIndex},{building.health},{building.maxHealth},{building.hasProducedUnit},{building.positionX - 0.5f},{building.positionY - 0.5f}");
        }
    }


    public void LoadBuildingsFromFile(string[] data)
    {
        try
        {
            BuildingType buildingType = (BuildingType)Enum.Parse(typeof(BuildingType), data[1]);
            int playerIndex = int.Parse(data[2]);
            int health = int.Parse(data[3]);
            int maxHealth = int.Parse(data[4]);
            bool hasProducedUnit = bool.Parse(data[5]);
            float positionX = float.Parse(data[6]) + 0.5f;
            float positionY = float.Parse(data[7]) + 0.5f;

            // Переключаемся на нужного игрока
            SetPlayer(playerIndex);

            // Проверяем, является ли здание крепостью
            if (buildingType == BuildingType.Fortress)
            {
                // Если это крепость, вызываем метод для ее создания
                Vector3 fortressPosition = new Vector3(positionX, positionY, -0.2f);
                GameObject fortressPrefab = GetFortressPrefab(playerIndex);
                GameObject fortress = Instantiate(fortressPrefab, fortressPosition, Quaternion.identity);
                fortress.name = $"Player{playerIndex}Fortress";
                fortress.transform.parent = this.transform;

                Building fortressBuilding = fortress.GetComponent<Building>();
                if (fortressBuilding != null)
                {
                    fortressBuilding.playerIndex = playerIndex;
                    fortressBuilding.health = health;
                    fortressBuilding.maxHealth = maxHealth;
                    fortressBuilding.hasProducedUnit = hasProducedUnit;
                    fortressBuilding.buildingType = BuildingType.Fortress;
                    fortressBuilding.positionX = positionX;
                    fortressBuilding.positionY = positionY;
                    // Регистрация крепости в менеджере зданий
                    RegisterBuilding(fortressBuilding);
                }
            }
            else
            {
                // Обычные здания: используем метод CreateBuilding для создания
                Vector3Int gridPosition = new Vector3Int((int)positionX, (int)positionY, 0);
                bool success = CreateBuilding((int)buildingType, gridPosition);

                if (success)
                {
                    // Получаем созданное здание и устанавливаем его параметры
                    Building createdBuilding = playerBuildings[playerBuildings.Count - 1]; // Последнее созданное здание
                    createdBuilding.health = health;
                    createdBuilding.maxHealth = maxHealth;
                    createdBuilding.hasProducedUnit = hasProducedUnit;
                    createdBuilding.positionX = positionX;
                    createdBuilding.positionY = positionY;
                    RegisterBuilding(createdBuilding);
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Ошибка загрузки зданий: {ex.Message}");
        }
    }

    GameObject GetFortressPrefab(int playerIndex)
    {
        // В зависимости от индекса игрока, возвращаем нужный префаб крепости
        switch (playerIndex)
        {
            case 0: return orcFortressPrefab;  // Орки
            case 1: return humanFortressPrefab;  // Люди
            case 2: return elfFortressPrefab;  // Эльфы
            case 3: return undeadFortressPrefab;  // Нежить
            default:
                Debug.LogError($"Неизвестная раса для игрока с индексом {playerIndex}.");
                return null;
        }
    }


    private GameObject GetBuildingPrefab(Building.BuildingType buildingType, int playerIndex)
    {
        switch (playerIndex)
        {
            case 1:
                return orcBuildings[(int)buildingType];
            case 2:
                return humanBuildings[(int)buildingType];
            case 3:
                return undeadBuildings[(int)buildingType];
            case 4:
                return elfBuildings[(int)buildingType];
            default:
                return null;
        }
    }


}
