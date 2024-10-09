using UnityEngine;

public class ResourceGatherer : MonoBehaviour
{
    public int playerIndex;

    public void CreateGatherer(GameObject gathererPrefab)
    {
        Vector3 spawnPosition = transform.position + new Vector3(1, 0, 0);
        Instantiate(gathererPrefab, spawnPosition, Quaternion.identity);
    }
}
