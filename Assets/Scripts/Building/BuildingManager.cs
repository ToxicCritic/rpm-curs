using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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

    private int currentPlayerIndex;

    public List<Building> playerBuildings = new List<Building>();

    private bool isInitialized = false;

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
                            Instantiate(selectedBuildingPrefab, spawnPosition, Quaternion.identity);
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
                return position.x < halfWidth && position.y < halfHeight;
            case 2:
                return position.x < halfWidth && position.y >= halfHeight;
            case 3:
                return position.x >= halfWidth && position.y >= halfHeight;
            case 4:
                return position.x >= halfWidth && position.y < halfHeight;
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
            DestroyPlayerBuildingsAndUnits(building.playerIndex); // ”ничтожаем здани€ и юниты игрока
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

        UnitManager unitManager = TurnManager.Instance.GetUnitManagerForPlayer(playerIndex);
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

    public void StartTurn(int playerIndex)
    {
        foreach (var building in playerBuildings)
        {
            if (building.playerIndex == playerIndex)
            {
                building.StartTurn();
            }
        }
    }

    public void Initialize()
    {
        isInitialized = true;
    }
}
