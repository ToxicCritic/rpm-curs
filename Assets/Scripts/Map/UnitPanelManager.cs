using UnityEngine;
using UnityEngine.UI;

public class UnitPanelManager : MonoBehaviour
{
    public Transform unitButtonPanel; // Панель для кнопок юнитов
    public Button unitButtonPrefab;   // Префаб кнопки юнита

    private GameObject selectedBuildingInstance; // Текущая выбранная казарма или домик сборщиков ресурсов
    private bool isUnitPanelActive = false;      // Флаг для отслеживания текущей активной панели

    public BuildingPanelManager buildingPanelManager; // Ссылка на менеджер панели построек

    public void SelectBuildingInstance(GameObject buildingInstance)
    {
        selectedBuildingInstance = buildingInstance;
        UpdateUnitPanel();
    }

    private void UpdateUnitPanel()
    {
        // Удаление старых кнопок юнитов
        foreach (Transform child in unitButtonPanel)
        {
            Destroy(child.gameObject);
        }

        UnitCreation unitCreation = selectedBuildingInstance.GetComponent<UnitCreation>();

        if (unitCreation != null)
        {
            GameObject[] availableUnits = unitCreation.GetAvailableUnits();
            int numberOfUnits = availableUnits.Length;
            float panelWidth = unitButtonPanel.GetComponent<RectTransform>().rect.width;
            float buttonWidth = unitButtonPrefab.GetComponent<RectTransform>().rect.width;
            float spacing = (panelWidth - (numberOfUnits * buttonWidth)) / (numberOfUnits + 1);

            for (int i = 0; i < numberOfUnits; i++)
            {
                GameObject unit = availableUnits[i];
                Button button = Instantiate(unitButtonPrefab, unitButtonPanel);
                int index = i;

                button.onClick.AddListener(() => unitCreation.CreateUnit(index));
                Image buttonImage = button.GetComponent<Image>();

                // Получаем спрайт из компонента SpriteRenderer префаба юнита
                SpriteRenderer unitSpriteRenderer = unit.GetComponent<SpriteRenderer>();
                if (unitSpriteRenderer != null)
                {
                    buttonImage.sprite = unitSpriteRenderer.sprite;
                }

                // Подсвечиваем недоступные юниты темным цветом
                UnitCost cost = unit.GetComponent<UnitCost>();
                if (cost != null && !ResourceManager.Instance.CanAffordUnit(cost))
                {
                    buttonImage.color = new Color(0.5f, 0.5f, 0.5f, 1f); // Темный цвет
                }

                // Располагаем кнопки по центру панели с равномерными отступами
                RectTransform buttonRect = button.GetComponent<RectTransform>();
                float xPos = spacing * (i + 1) + buttonWidth * i;
                buttonRect.anchoredPosition = new Vector2(xPos, 0);
                buttonRect.localScale = Vector3.one; // Убедимся, что кнопка масштабируется правильно

                Debug.Log($"Button for unit {i} created at position {xPos}"); // Отладочное сообщение
            }
        }
        else
        {
            Debug.LogWarning("UnitCreation component not found on selected building instance.");
        }

        unitButtonPanel.gameObject.SetActive(true);
        buildingPanelManager.HideBuildPanel();
        isUnitPanelActive = true;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isUnitPanelActive)
            {
                ShowBuildPanel();
            }
        }
    }

    private void ShowBuildPanel()
    {
        unitButtonPanel.gameObject.SetActive(false);
        buildingPanelManager.ShowBuildPanel();
        isUnitPanelActive = false;
    }
}
