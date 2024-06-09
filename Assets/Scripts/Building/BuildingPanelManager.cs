using UnityEngine;
using UnityEngine.UI;

public class BuildingPanelManager : MonoBehaviour
{
    public GameObject[] player1Buildings;
    public GameObject[] player2Buildings;
    public GameObject[] player3Buildings;
    public GameObject[] player4Buildings;

    private GameObject[] currentBuildings;
    private GameObject selectedBuildingPrefab;

    public Transform buttonPanel;          // Ссылка на панель для кнопок зданий
    public Button buildingButtonPrefab;    // Префаб кнопки для построек

    public GameObject tileHighlighterPrefab; // Префаб объекта выделения тайла
    private GameObject tileHighlighterInstance;

    public GameObject alwaysOnHighlighterPrefab; // Префаб второго объекта выделения
    private GameObject alwaysOnHighlighterInstance;

    public LayerMask obstacleLayerMask;

    private void Start()
    {
        InitializeHighlighters();
        SetPlayer(0); // Предполагаем, что игра начинается с первого игрока
    }

    private void InitializeHighlighters()
    {
        tileHighlighterInstance = Instantiate(tileHighlighterPrefab);
        tileHighlighterInstance.SetActive(false);

        alwaysOnHighlighterInstance = Instantiate(alwaysOnHighlighterPrefab);
        alwaysOnHighlighterInstance.SetActive(true);
    }

    public void SetPlayer(int playerIndex)
    {
        switch (playerIndex)
        {
            case 0:
                currentBuildings = player1Buildings;
                break;
            case 1:
                currentBuildings = player2Buildings;
                break;
            case 2:
                currentBuildings = player3Buildings;
                break;
            case 3:
                currentBuildings = player4Buildings;
                break;
            default:
                currentBuildings = null;
                break;
        }
        UpdateBuildPanel();
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

    private void UpdateBuildPanel()
    {
        foreach (Transform child in buttonPanel)
        {
            Destroy(child.gameObject);
        }

        if (currentBuildings == null) return;

        float panelWidth = buttonPanel.GetComponent<RectTransform>().rect.width;
        float buttonWidth = buildingButtonPrefab.GetComponent<RectTransform>().rect.width;
        float spacing = (panelWidth - (currentBuildings.Length * buttonWidth)) / (currentBuildings.Length + 1);

        for (int i = 0; i < currentBuildings.Length; i++)
        {
            CreateBuildingButton(currentBuildings[i], i, spacing, buttonWidth);
        }
    }

    private void CreateBuildingButton(GameObject building, int index, float spacing, float buttonWidth)
    {
        Button button = Instantiate(buildingButtonPrefab, buttonPanel);
        button.onClick.AddListener(() => SelectBuilding(index));
        Image buttonImage = button.GetComponent<Image>();

        SpriteRenderer buildingSpriteRenderer = building.GetComponent<SpriteRenderer>();
        if (buildingSpriteRenderer != null)
        {
            buttonImage.sprite = buildingSpriteRenderer.sprite;
        }

        float xPos = spacing * (index + 1) + buttonWidth * index;
        button.GetComponent<RectTransform>().anchoredPosition = new Vector2(xPos, 0);
    }

    private void Update()
    {
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3Int gridPosition = Vector3Int.FloorToInt(mousePosition);
        gridPosition.z = 0;

        UpdateHighlighterPosition(gridPosition);

        if (selectedBuildingPrefab != null && Input.GetMouseButtonDown(0))
        {
            HandleBuildingPlacement(gridPosition);
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            CancelBuildingPlacement();
        }
    }

    private void UpdateHighlighterPosition(Vector3Int gridPosition)
    {
        if (alwaysOnHighlighterInstance.activeSelf)
        {
            alwaysOnHighlighterInstance.transform.position = new Vector3(gridPosition.x + 0.5f, gridPosition.y + 0.5f, -0.2f);
        }

        if (tileHighlighterInstance.activeSelf)
        {
            tileHighlighterInstance.transform.position = new Vector3(gridPosition.x + 0.5f, gridPosition.y + 0.5f, -0.1f);
        }
    }

    private void HandleBuildingPlacement(Vector3Int gridPosition)
    {
        if (CanPlaceBuilding(gridPosition) && IsWithinMapBounds(gridPosition))
        {
            BuildingCost cost = selectedBuildingPrefab.GetComponent<BuildingCost>();
            if (cost != null && PlayerResourceManager.Instance.CanAfford(cost.wood, cost.stone, 0))
            {
                if (PlayerResourceManager.Instance.SpendResource("wood", cost.wood) && PlayerResourceManager.Instance.SpendResource("stone", cost.stone))
                {
                    Instantiate(selectedBuildingPrefab, new Vector3(gridPosition.x + 0.5f, gridPosition.y + 0.5f, -0.1f), Quaternion.identity);
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
        return position.x >= 0 && position.y >= 0 && position.x < 30 && position.y < 30;
    }
}
