using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UnitPanelManager : MonoBehaviour
{
    public Transform unitButtonPanel;
    public Button unitButtonPrefab;
    public Sprite goldIconSprite;
    public BuildingManager buildingManager;
    public TMP_FontAsset fontAsset;

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
        // Очищаем панель перед добавлением новых кнопок
        foreach (Transform child in unitButtonPanel)
        {
            Destroy(child.gameObject);
        }

        // Получаем доступные для создания юниты
        GameObject[] availableUnits = unitCreation.GetAvailableUnits();
        float panelWidth = unitButtonPanel.GetComponent<RectTransform>().rect.width;
        float buttonWidth = unitButtonPrefab.GetComponent<RectTransform>().rect.width;
        float spacing = (panelWidth - (availableUnits.Length * buttonWidth)) / (availableUnits.Length + 1);

        // Проходимся по каждому доступному юниту
        for (int i = 0; i < availableUnits.Length; i++)
        {
            GameObject unit = availableUnits[i];
            Button button = Instantiate(unitButtonPrefab, unitButtonPanel);
            int index = i;

            // Присоединяем событие к кнопке
            button.onClick.AddListener(() => CreateUnit(index));

            // Устанавливаем изображение юнита на кнопку
            Image buttonImage = button.GetComponent<Image>();
            SpriteRenderer unitSpriteRenderer = unit.GetComponent<SpriteRenderer>();
            if (unitSpriteRenderer != null)
            {
                buttonImage.sprite = unitSpriteRenderer.sprite;
            }

            // Получаем стоимость юнита
            UnitCost cost = unit.GetComponent<UnitCost>();

            // Создаем новый объект для отображения иконки и стоимости
            GameObject costPanel = new GameObject("CostPanel");
            costPanel.transform.SetParent(button.transform, false); // Устанавливаем как дочерний элемент кнопки

            // Создаем иконку золота
            GameObject goldIconObject = new GameObject("GoldIcon");
            Image goldIcon = goldIconObject.AddComponent<Image>();
            goldIcon.sprite = goldIconSprite;
            goldIconObject.transform.SetParent(costPanel.transform, false);

            // Создаем текст для стоимости
            GameObject goldCostTextObject = new GameObject("GoldCostText");
            TextMeshProUGUI goldCostText = goldCostTextObject.AddComponent<TextMeshProUGUI>();
            goldCostText.text = cost != null ? cost.gold.ToString() : "0";
            goldCostText.fontSize = 24; // Задаем размер шрифта
            goldCostText.font = fontAsset;
            goldCostText.alignment = TextAlignmentOptions.Center;
            goldCostTextObject.transform.SetParent(costPanel.transform, false);

            // Настраиваем размеры иконки и текста
            RectTransform iconRectTransform = goldIconObject.GetComponent<RectTransform>();
            iconRectTransform.sizeDelta = new Vector2(30, 30); // Размер иконки

            RectTransform textRectTransform = goldCostTextObject.GetComponent<RectTransform>();
            textRectTransform.sizeDelta = new Vector2(50, 30); // Размер текста
            textRectTransform.anchoredPosition = new Vector2(30, 0); // Расположение текста рядом с иконкой

            // Настраиваем положение панели стоимости относительно кнопки
            RectTransform costPanelRectTransform = costPanel.AddComponent<RectTransform>();
            costPanelRectTransform.sizeDelta = new Vector2(100, 30); // Размер панели стоимости
            costPanelRectTransform.anchoredPosition = new Vector2(-80, 0); // Слева от кнопки

            // Проверяем, может ли игрок позволить себе юнита
            PlayerResourceManager currentPlayerResourceManager = TurnManager.Instance.GetCurrentPlayerResourceManager();
            if (cost != null && !currentPlayerResourceManager.CanAfford(0, 0, cost.gold))
            {
                buttonImage.color = new Color(0.5f, 0.5f, 0.5f, 1f); // Серый цвет, если ресурсов недостаточно
            }

            // Позиционирование кнопок на панели
            float xPos = spacing * (i + 1) + buttonWidth * i;
            button.GetComponent<RectTransform>().anchoredPosition = new Vector2(xPos, 0);
        }
    }

    public void CreateUnit(int index) 
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
