using UnityEngine;
using UnityEngine.UI;

public class UnitPanelManager : MonoBehaviour
{
    public Transform unitButtonPanel; // ������ ��� ������ ������
    public Button unitButtonPrefab;   // ������ ������ �����

    private GameObject selectedBuildingInstance; // ������� ��������� ������� ��� ����� ��������� ��������
    private bool isUnitPanelActive = false;      // ���� ��� ������������ ������� �������� ������

    public BuildingPanelManager buildingPanelManager; // ������ �� �������� ������ ��������

    public void SelectBuildingInstance(GameObject buildingInstance)
    {
        selectedBuildingInstance = buildingInstance;
        UpdateUnitPanel();
    }

    private void UpdateUnitPanel()
    {
        // �������� ������ ������ ������
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

                // �������� ������ �� ���������� SpriteRenderer ������� �����
                SpriteRenderer unitSpriteRenderer = unit.GetComponent<SpriteRenderer>();
                if (unitSpriteRenderer != null)
                {
                    buttonImage.sprite = unitSpriteRenderer.sprite;
                }

                // ������������ ����������� ����� ������ ������
                UnitCost cost = unit.GetComponent<UnitCost>();
                if (cost != null && !ResourceManager.Instance.CanAffordUnit(cost))
                {
                    buttonImage.color = new Color(0.5f, 0.5f, 0.5f, 1f); // ������ ����
                }

                // ����������� ������ �� ������ ������ � ������������ ���������
                RectTransform buttonRect = button.GetComponent<RectTransform>();
                float xPos = spacing * (i + 1) + buttonWidth * i;
                buttonRect.anchoredPosition = new Vector2(xPos, 0);
                buttonRect.localScale = Vector3.one; // ��������, ��� ������ �������������� ���������

                Debug.Log($"Button for unit {i} created at position {xPos}"); // ���������� ���������
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
