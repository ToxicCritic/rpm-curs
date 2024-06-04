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

    public Transform buttonPanel;          // Ссылка на панель для кнопок зданий
    public Button buildingButtonPrefab;    // Префаб кнопки для построек

    public LayerMask obstacleLayerMask;    // Слой препятствий (ресурсы и здания)
    public int mapWidth = 30;
    public int mapHeight = 30;

    public GameObject tileHighlighterPrefab; // Префаб объекта выделения тайла
    private GameObject tileHighlighterInstance;

    public GameObject alwaysOnHighlighterPrefab; // Префаб второго объекта выделения
    private GameObject alwaysOnHighlighterInstance;

    private GameObject selectedBuildingInstance; // Текущая выбранная казарма или домик сборщиков ресурсов

    private bool isUnitPanelActive = false; // Флаг для отслеживания текущей активной панели

    void Start()
    {
        // Создаем объекты выделения и скрываем их
        tileHighlighterInstance = Instantiate(tileHighlighterPrefab);
        tileHighlighterInstance.SetActive(false);

        alwaysOnHighlighterInstance = Instantiate(alwaysOnHighlighterPrefab);
        alwaysOnHighlighterInstance.SetActive(true); // Второе выделение всегда активно

        // Инициализация текущих зданий в зависимости от выбранной расы
        SetRace(PlayerPrefs.GetInt("SelectedRace", 0)); // Предполагаем, что выбор расы сохраняется в PlayerPrefs
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

    void UpdateBuildPanel()
    {
        // Удаление старых кнопок
        foreach (Transform child in buttonPanel)
        {
            Destroy(child.gameObject);
        }

        // Создание кнопок для текущих зданий
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

            // Получаем спрайт из компонента SpriteRenderer префаба здания
            SpriteRenderer buildingSpriteRenderer = building.GetComponent<SpriteRenderer>();
            if (buildingSpriteRenderer != null)
            {
                buttonImage.sprite = buildingSpriteRenderer.sprite;
            }

            // Располагаем кнопки по центру панели с равномерными отступами
            float xPos = spacing * (i + 1) + buttonWidth * i;
            button.GetComponent<RectTransform>().anchoredPosition = new Vector2(xPos, 0);
        }
    }

    void Update()
    {
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0;  // Устанавливаем Z-координату в 0
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
                        selectedBuildingPrefab = null; // Сбрасываем выбранное здание после размещения
                        tileHighlighterInstance.SetActive(false);
                        alwaysOnHighlighterInstance.SetActive(true); // Включаем постоянное выделение после размещения здания
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

        // Обработка нажатия клавиши Escape
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            CancelBuildingPlacement();
        }
    }

    void CancelBuildingPlacement()
    {
        selectedBuildingPrefab = null;
        tileHighlighterInstance.SetActive(false);
        alwaysOnHighlighterInstance.SetActive(true); // Включаем постоянное выделение при отмене строительства
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
}
