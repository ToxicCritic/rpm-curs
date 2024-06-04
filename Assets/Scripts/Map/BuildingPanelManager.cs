using UnityEngine;
using UnityEngine.UI;

public class BuildingPanelManager : MonoBehaviour
{
    public GameObject[] orcBuildings;
    public GameObject[] elfBuildings;
    public GameObject[] humanBuildings;
    public GameObject[] undeadBuildings;

    private GameObject[] currentBuildings;
    private GameObject selectedBuildingPrefab;

    public Transform buttonPanel;
    public Button buildingButtonPrefab;

    public GameObject tileHighlighterPrefab;
    private GameObject tileHighlighterInstance;

    public GameObject alwaysOnHighlighterPrefab;
    private GameObject alwaysOnHighlighterInstance;

    public LayerMask obstacleLayerMask;
    public int mapWidth = 30;
    public int mapHeight = 30;

    private UnitPanelManager unitPanelManager;

    private void Start()
    {
        tileHighlighterInstance = Instantiate(tileHighlighterPrefab);
        tileHighlighterInstance.SetActive(false);

        alwaysOnHighlighterInstance = Instantiate(alwaysOnHighlighterPrefab);
        alwaysOnHighlighterInstance.SetActive(true);

        unitPanelManager = FindObjectOfType<UnitPanelManager>();
        if (unitPanelManager == null)
        {
            Debug.LogError("UnitPanelManager not found in the scene.");
        }
        else
        {
            unitPanelManager.buildingPanelManager = this;
        }
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

    public void SetRace(int raceIndex)
    {
        switch (raceIndex)
        {
            case 0:
                currentBuildings = orcBuildings;
                break;
            case 1:
                currentBuildings = elfBuildings;
                break;
            case 2:
                currentBuildings = humanBuildings;
                break;
            case 3:
                currentBuildings = undeadBuildings;
                break;
            default:
                currentBuildings = null;
                break;
        }
        UpdateBuildPanel();
    }

    private void UpdateBuildPanel()
    {
        foreach (Transform child in buttonPanel)
        {
            Destroy(child.gameObject);
        }

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
            RectTransform buttonRect = button.GetComponent<RectTransform>();
            buttonRect.anchoredPosition = new Vector2(xPos, 0);
            buttonRect.localScale = Vector3.one; // Убедимся, что кнопка масштабируется правильно
        }
    }

    private void Update()
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

        if (tileHighlighterInstance.activeSelf)
        {
            Vector3 placePosition = new Vector3(gridPosition.x + 0.5f, gridPosition.y + 0.5f, -0.1f);
            tileHighlighterInstance.transform.position = placePosition;
        }

        if (selectedBuildingPrefab != null && Input.GetMouseButtonDown(0))
        {
            if (CanPlaceBuilding(gridPosition) && IsWithinMapBounds(gridPosition))
            {
                BuildingCost cost = selectedBuildingPrefab.GetComponent<BuildingCost>();
                if (cost != null && ResourceManager.Instance.CanAfford(cost.wood, cost.stone, 0))
                {
                    bool woodSpent = ResourceManager.Instance.SpendResource("wood", cost.wood);
                    bool stoneSpent = ResourceManager.Instance.SpendResource("stone", cost.stone);

                    if (woodSpent && stoneSpent)
                    {
                        Vector3 spawnPosition = new Vector3(gridPosition.x + 0.5f, gridPosition.y + 0.5f, -0.1f); // Отрицательная координата Z
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
            else
            {
                Debug.Log("Cannot place building, it is outside the map bounds or position is occupied.");
            }
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            CancelBuildingPlacement();
        }
    }

    private void CancelBuildingPlacement()
    {
        selectedBuildingPrefab = null;
        tileHighlighterInstance.SetActive(false);
        alwaysOnHighlighterInstance.SetActive(true);
        Debug.Log("Building placement cancelled.");
    }

    private bool CanPlaceBuilding(Vector3Int position)
    {
        Vector2 checkPosition = new Vector2(position.x + 0.5f, position.y + 0.5f);
        Collider2D[] colliders = Physics2D.OverlapBoxAll(checkPosition, new Vector2(1, 1), 0, obstacleLayerMask);
        return colliders.Length == 0;
    }

    private bool IsWithinMapBounds(Vector3Int position)
    {
        return position.x >= 0 && position.y >= 0 && position.x < mapWidth && position.y < mapHeight;
    }

    private void MoveHighlighterOutOfView(GameObject highlighter)
    {
        highlighter.transform.position = new Vector3(-9999, -9999, highlighter.transform.position.z);
    }

    public void ShowUnitPanel(GameObject buildingInstance)
    {
        unitPanelManager.SelectBuildingInstance(buildingInstance);
        tileHighlighterInstance.SetActive(false);
        alwaysOnHighlighterInstance.SetActive(false);
        buttonPanel.gameObject.SetActive(false); // Скрываем панель построек
    }

    public void HideBuildPanel()
    {
        buttonPanel.gameObject.SetActive(false);
    }

    public void ShowBuildPanel()
    {
        buttonPanel.gameObject.SetActive(true);
    }
}
