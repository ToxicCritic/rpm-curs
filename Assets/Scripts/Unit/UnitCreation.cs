using UnityEngine;

public class UnitCreation : MonoBehaviour
{
    public GameObject[] availableUnits;

    public GameObject[] GetAvailableUnits()
    {
        return availableUnits;
    }

    public void CreateUnit(int index, GameObject building)
    {
        if (index >= 0 && index < availableUnits.Length)
        {
            Vector3 spawnPosition = new Vector3(building.transform.position.x, building.transform.position.y - 1.5f, building.transform.position.z - 0.1f);
            Instantiate(availableUnits[index], spawnPosition, Quaternion.identity);
        }
    }
}
