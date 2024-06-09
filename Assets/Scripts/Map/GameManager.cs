using UnityEngine;

public class GameManager : MonoBehaviour
{
    public BuildingPanelManager buildingPanelManager;
    public UnitPanelManager unitPanelManager;

    void Start()
    {
        int selectedRace = PlayerPrefs.GetInt("SelectedRace", 0);
    }
}
