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
        unitButtonPanel.gameObject.SetActive(true); 
    }

    private void SelectBuildingInstance(Building building)
    {
        selectedBuildingInstance = building;
        unitCreation = building.GetComponent<UnitCreation>();
        if (building.buildingType == Building.BuildingType.Barracks || building.buildingType == Building.BuildingType.ResourceGatherer)
        {
            UpdateUnitPanel();
            buildingManager.HideBuildPanel();
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

            GameObject costPanel = new GameObject("CostPanel");
            costPanel.transform.SetParent(button.transform, false); 

            GameObject goldIconObject = new GameObject("GoldIcon");
            Image goldIcon = goldIconObject.AddComponent<Image>();
            goldIcon.sprite = goldIconSprite;
            goldIconObject.transform.SetParent(costPanel.transform, false);

            GameObject goldCostTextObject = new GameObject("GoldCostText");
            TextMeshProUGUI goldCostText = goldCostTextObject.AddComponent<TextMeshProUGUI>();
            goldCostText.text = cost != null ? cost.gold.ToString() : "0";
            goldCostText.fontSize = 20; 
            goldCostText.font = fontAsset;
            goldCostText.alignment = TextAlignmentOptions.Center;
            goldCostTextObject.transform.SetParent(costPanel.transform, false);

            RectTransform iconRectTransform = goldIconObject.GetComponent<RectTransform>();
            iconRectTransform.sizeDelta = new Vector2(30, 30); 

            RectTransform textRectTransform = goldCostTextObject.GetComponent<RectTransform>();
            textRectTransform.sizeDelta = new Vector2(50, 30);
            textRectTransform.anchoredPosition = new Vector2(30, 0); 

            RectTransform costPanelRectTransform = costPanel.AddComponent<RectTransform>();
            costPanelRectTransform.sizeDelta = new Vector2(100, 30); 
            costPanelRectTransform.anchoredPosition = new Vector2(-80, 0); 

            PlayerResourceManager currentPlayerResourceManager = TurnManager.Instance.GetCurrentPlayerResourceManager();
            if (cost != null && !currentPlayerResourceManager.CanAfford(0, 0, cost.gold))
            {
                buttonImage.color = new Color(0.5f, 0.5f, 0.5f, 1f);
            }

            float xPos = 30 + spacing * (i + 1) + buttonWidth * i;
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
                unitCreation.CreateUnit(index, selectedBuildingInstance.gameObject); 
                selectedBuildingInstance.ProduceUnit();
                unitButtonPanel.gameObject.SetActive(false);
                buildingManager.ShowBuildPanel();
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
            unitButtonPanel.gameObject.SetActive(false); 
            buildingManager.ShowBuildPanel(); 
        }
    }
}
