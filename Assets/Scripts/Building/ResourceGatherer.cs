using UnityEngine;

public class ResourceGatherer : MonoBehaviour
{
    public int playerIndex;

    // Метод для создания сборщика ресурсов
    public void CreateGatherer(GameObject gathererPrefab)
    {
        Vector3 spawnPosition = transform.position + new Vector3(1, 0, 0); // Примерное место спауна рядом с домиком
        Instantiate(gathererPrefab, spawnPosition, Quaternion.identity);
    }
}
