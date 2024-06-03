using UnityEngine;

public class Building : MonoBehaviour
{
    private UnitPanelManager unitPanelManager;

    void Start()
    {
        unitPanelManager = FindObjectOfType<UnitPanelManager>();
        if (unitPanelManager == null)
        {
            Debug.LogError("UnitPanelManager not found in the scene.");
        }
    }

    void OnMouseDown()
    {
        if (unitPanelManager != null)
        {
            unitPanelManager.SelectBuildingInstance(this.gameObject);
        }
        else
        {
            Debug.LogError("UnitPanelManager is null.");
        }
    }
}
