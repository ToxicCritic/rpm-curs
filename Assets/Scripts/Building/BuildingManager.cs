using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using TMPro;
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
    public Sprite woodIcon;
    public Sprite stoneIcon;
    public TMP_FontAsset customTMPFont;

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

    public void UpdateBuildPanel()
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

            float xPos = 30 + spacing * (i + 1) + buttonWidth * i;
            button.GetComponent<RectTransform>().anchoredPosition = new Vector2(xPos, 0);

            BuildingCost cost = building.GetComponent<BuildingCost>();
            if (cost != null)
            {
                GameObject woodCostTextObject = new GameObject("WoodCostText");
                woodCostTextObject.transform.SetParent(button.transform);

                TextMeshProUGUI woodCostText = woodCostTextObject.AddComponent<TextMeshProUGUI>();
                woodCostText.text = cost.wood.ToString();
                woodCostText.font = customTMPFont;  
                woodCostText.fontSize = 20;
                woodCostText.color = Color.green;  
                woodCostText.alignment = TextAlignmentOptions.Center;

                RectTransform woodTextRectTransform = woodCostTextObject.GetComponent<RectTransform>();
                woodTextRectTransform.sizeDelta = new Vector2(100, 30); 
                woodTextRectTransform.anchoredPosition = new Vector2(-45, 10); 

                // Создаем объект для иконки дерева
                GameObject woodIconObject = new GameObject("WoodIcon");
                woodIconObject.transform.SetParent(button.transform);

                Image woodIconImage = woodIconObject.AddComponent<Image>();
                woodIconImage.sprite = woodIcon; 

                RectTransform woodIconRectTransform = woodIconObject.GetComponent<RectTransform>();
                woodIconRectTransform.sizeDelta = new Vector2(24, 24);  
                woodIconRectTransform.anchoredPosition = new Vector2(-75, 10); 

                GameObject stoneCostTextObject = new GameObject("StoneCostText");
                stoneCostTextObject.transform.SetParent(button.transform);

                TextMeshProUGUI stoneCostText = stoneCostTextObject.AddComponent<TextMeshProUGUI>();
                stoneCostText.text = cost.stone.ToString(); 
                stoneCostText.font = customTMPFont; 
                stoneCostText.fontSize = 20;
                stoneCostText.color = Color.gray;  
                stoneCostText.alignment = TextAlignmentOptions.Center;

                RectTransform stoneTextRectTransform = stoneCostTextObject.GetComponent<RectTransform>();
                stoneTextRectTransform.sizeDelta = new Vector2(100, 30); 
                stoneTextRectTransform.anchoredPosition = new Vector2(-45, -15);  

                GameObject stoneIconObject = new GameObject("StoneIcon");
                stoneIconObject.transform.SetParent(button.transform);

                Image stoneIconImage = stoneIconObject.AddComponent<Image>();
                stoneIconImage.sprite = stoneIcon;  

                RectTransform stoneIconRectTransform = stoneIconObject.GetComponent<RectTransform>();
                stoneIconRectTransform.sizeDelta = new Vector2(24, 24); 
                stoneIconRectTransform.anchoredPosition = new Vector2(-75, -15);  
            }

            PlayerResourceManager currentPlayerResourceManager = TurnManager.Instance.GetCurrentPlayerResourceManager();
            if (cost != null && !currentPlayerResourceManager.CanAfford(cost.wood, cost.stone, 0))
            {
                buttonImage.color = new Color(0.5f, 0.5f, 0.5f, 1f); 
            }
        }
    }


    void Update()
    {
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0;
        Vector3Int gridPosition = Vector3Int.FloorToInt(mousePosition);
        gridPosition.z = 0;

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

                            GameObject buildingObject = Instantiate(selectedBuildingPrefab, spawnPosition, Quaternion.identity);
                            Building building = buildingObject.GetComponent<Building>();

                            building.positionX = spawnPosition.x;
                            building.positionY = spawnPosition.y;

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
        if (CanPlaceBuilding(gridPosition) && IsWithinMapBounds(gridPosition) && IsInPlayerQuarter(gridPosition))
        {
            GameObject buildingPrefab = currentBuildings[buildingIndex];
            BuildingCost cost = buildingPrefab.GetComponent<BuildingCost>();
            PlayerResourceManager currentPlayerResourceManager = TurnManager.Instance.GetCurrentPlayerResourceManager();


            Vector3 spawnPosition = new Vector3(gridPosition.x + 0.5f, gridPosition.y + 0.5f, -0.1f);
            GameObject buildingObject = Instantiate(buildingPrefab, spawnPosition, Quaternion.identity);
            Building building = buildingObject.GetComponent<Building>();
            building.positionX = spawnPosition.x;
            building.positionY = spawnPosition.y;

            RegisterBuilding(building);

            return true; 

        }
        else
        {
            Debug.Log("Cannot place building.");
        }

        return false; 
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
            DestroyPlayerBuildingsAndUnits(building.playerIndex);
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

            SetPlayer(playerIndex);

            if (buildingType == BuildingType.Fortress)
            {
                Vector3 fortressPosition = new Vector3(positionX, positionY, -0.2f);
                GameObject fortressPrefab = GetFortressPrefab(playerIndex - 1);
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
                    RegisterBuilding(fortressBuilding);
                }
            }
            else
            {
                Vector3Int gridPosition = new Vector3Int((int)positionX, (int)positionY, 0);
                bool success = CreateBuilding((int)buildingType, gridPosition);

                if (success)
                {
                    Building createdBuilding = playerBuildings[playerBuildings.Count - 1]; 
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
        switch (playerIndex)
        {
            case 0: return orcFortressPrefab; 
            case 1: return humanFortressPrefab; 
            case 3: return elfFortressPrefab;  
            case 2: return undeadFortressPrefab;  
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
