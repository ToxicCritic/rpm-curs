using UnityEngine;

public class UnitCreation : MonoBehaviour
{
    public GameObject[] unitPrefabs;

    public GameObject[] GetAvailableUnits()
    {
        return unitPrefabs;
    }

    public void CreateUnit(int unitIndex)
    {
        if (unitIndex >= 0 && unitIndex < unitPrefabs.Length)
        {
            Vector3 spawnPosition = transform.position + new Vector3(1, 0, -0.1f); // Позиция появления юнита рядом со зданием с отрицательной Z координатой
            Instantiate(unitPrefabs[unitIndex], spawnPosition, Quaternion.identity);
        }
    }
}
