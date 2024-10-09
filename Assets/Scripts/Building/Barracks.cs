using UnityEngine;

public class Barracks : MonoBehaviour
{
    public int playerIndex;

    // Метод для создания война
    public void CreateWarrior(GameObject warriorPrefab)
    {
        Vector3 spawnPosition = transform.position + new Vector3(1, 0, 0); 
        Instantiate(warriorPrefab, spawnPosition, Quaternion.identity);
    }
}
