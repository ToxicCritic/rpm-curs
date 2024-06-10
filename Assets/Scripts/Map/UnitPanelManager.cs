using UnityEngine;
using UnityEngine.UI;

public class UnitPanelManager : MonoBehaviour
{
    public Transform unitButtonPanel;
    public Button unitButtonPrefab;

    public BuildingManager buildingManager;

    private Building selectedBuildingInstance;
    private UnitCreation unitCreation;

    public void ShowUnitCreationPanel(Building building)
    {
        SelectBuildingInstance(building);
        unitButtonPanel.gameObject.SetActive(true); // Показываем панель юнитов
    }

    private void SelectBuildingInstance(Building building)
    {
        selectedBuildingInstance = building;
        unitCreation = building.GetComponent<UnitCreation>();
        if (building.buildingType == Building.BuildingType.Barracks || building.buildingType == Building.BuildingType.ResourceGatherer)
        {
            UpdateUnitPanel();
            buildingManager.HideBuildPanel(); // Скрываем панель построек при выборе здания для создания юнитов
        }
    }

    private void UpdateUnitPanel()
    {
        foreach (Transform child in unitButtonPanel)
        {
            Destroy(child.gameObject);
        }

        GameObject[] availableUnits = unitCreation.GetAvailableUnits();
        float panelWidth = unitButtonPanel.GetComponent<RectTransform>().rect.width;
        float buttonWidth = unitButtonPrefab.GetComponent<RectTransform>().rect.width;
        float spacing = (panelWidth - (availableUnits.Length * buttonWidth)) / (availableUnits.Length + 1);

        for (int i = 0; i < availableUnits.Length; i++)
        {
            GameObject unit = availableUnits[i];
            Button button = Instantiate(unitButtonPrefab, unitButtonPanel);
            int index = i;

            button.onClick.AddListener(() => CreateUnit(index));
            Image buttonImage = button.GetComponent<Image>();

            SpriteRenderer unitSpriteRenderer = unit.GetComponent<SpriteRenderer>();
            if (unitSpriteRenderer != null)
            {
                buttonImage.sprite = unitSpriteRenderer.sprite;
            }

            UnitCost cost = unit.GetComponent<UnitCost>();
            PlayerResourceManager currentPlayerResourceManager = TurnManager.Instance.GetCurrentPlayerResourceManager();
            if (cost != null && !currentPlayerResourceManager.CanAfford(0, 0, cost.gold))
            {
                buttonImage.color = new Color(0.5f, 0.5f, 0.5f, 1f);
            }

            float xPos = spacing * (i + 1) + buttonWidth * i;
            button.GetComponent<RectTransform>().anchoredPosition = new Vector2(xPos, 0);
        }
    }

    private void CreateUnit(int index)
    {
        PlayerResourceManager currentPlayerResourceManager = TurnManager.Instance.GetCurrentPlayerResourceManager();
        GameObject unitPrefab = unitCreation.GetAvailableUnits()[index];
        UnitCost cost = unitPrefab.GetComponent<UnitCost>();

        if (cost != null && currentPlayerResourceManager.CanAfford(0, 0, cost.gold))
        {
            bool goldSpent = currentPlayerResourceManager.SpendResource("gold", cost.gold);

            if (goldSpent)
            {
                unitCreation.CreateUnit(index, selectedBuildingInstance.gameObject); // Передаем buildingInstance при создании юнита
                selectedBuildingInstance.ProduceUnit(); // Устанавливаем флаг производства юнита
                unitButtonPanel.gameObject.SetActive(false); // Скрываем панель юнитов после создания юнита
                buildingManager.ShowBuildPanel(); // Вернуться на панель построек после создания юнита
            }
        }
        else
        {
            Debug.Log("Not enough resources to create unit.");
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            unitButtonPanel.gameObject.SetActive(false); // Скрываем панель юнитов при нажатии Esc
            buildingManager.ShowBuildPanel(); // Вернуться на панель построек при нажатии Esc
        }
    }
}
