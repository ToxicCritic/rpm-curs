using UnityEngine;

public class UnitCreation : MonoBehaviour
{
    public GameObject[] unitPrefabs;

    public GameObject[] GetAvailableUnits()
    {
        Debug.Log("GetAvailableUnits called");
        Debug.Log($"Number of units available: {unitPrefabs.Length}");
        foreach (var unit in unitPrefabs)
        {
            Debug.Log($"Available unit: {unit.name}");
        }
        return unitPrefabs;
    }

    public void CreateUnit(int unitIndex)
    {
        if (unitIndex >= 0 && unitIndex < unitPrefabs.Length)
        {
            GameObject unitPrefab = unitPrefabs[unitIndex];
            UnitCost cost = unitPrefab.GetComponent<UnitCost>();

            if (cost != null && ResourceManager.Instance.CanAfford(0, 0, cost.gold))
            {
                bool goldSpent = ResourceManager.Instance.SpendResource("gold", cost.gold);

                if (goldSpent)
                {
                    Vector3 spawnPosition = new Vector3(Mathf.Floor(transform.position.x) + 0.5f, Mathf.Floor(transform.position.y) - 1f, -0.1f);
                    Instantiate(unitPrefabs[unitIndex], spawnPosition, Quaternion.identity);
                }
                else
                {
                    Debug.Log("Not enough gold to create unit.");
                }
            }
            else
            {
                Debug.Log("Not enough resources to create unit.");
            }
        }
    }
}
