using UnityEngine;

public class FortressSpawner : MonoBehaviour
{
    public GameObject orcFortressPrefab;
    public GameObject elfFortressPrefab;
    public GameObject humanFortressPrefab;
    public GameObject undeadFortressPrefab;

    public GameObject[] treePrefabs;  // Префабы деревьев
    public GameObject[] rockPrefabs;  // Префабы камней

    public int mapWidth = 30;
    public int mapHeight = 30;
    public float treeProbability = 0.05f;
    public float rockProbability = 0.05f;

    private Transform[,] gridObjects; // Массив для хранения объектов на сетке

    void Start()
    {
        gridObjects = new Transform[mapWidth, mapHeight];
        SpawnFortresses();
        SpawnResources();
    }

    void SpawnFortresses()
    {
        // Координаты для спавна крепостей с отступом в одну клетку от края
        Vector3[] positions = new Vector3[]
        {
            new Vector3(1, 1, 0),                    // Нижний левый угол
            new Vector3(1, mapHeight - 3, 0),        // Верхний левый угол
            new Vector3(mapWidth - 3, 1, 0),         // Нижний правый угол
            new Vector3(mapWidth - 3, mapHeight - 3, 0) // Верхний правый угол
        };

        int selectedRace = PlayerPrefs.GetInt("SelectedRace", 0);

        GameObject playerFortress = Instantiate(GetFortressPrefab(selectedRace), positions[selectedRace], Quaternion.identity);
        playerFortress.name = "PlayerFortress";
        MarkOccupied(playerFortress);

        for (int i = 0; i < positions.Length; i++)
        {
            if (i != selectedRace)
            {
                GameObject fortress = Instantiate(GetFortressPrefab(i), positions[i], Quaternion.identity);
                fortress.name = $"EnemyFortress{i}";
                MarkOccupied(fortress);
            }
        }
    }

    GameObject GetFortressPrefab(int raceIndex)
    {
        switch (raceIndex)
        {
            case 0:
                return orcFortressPrefab;
            case 1:
                return humanFortressPrefab;  // Поменяли местами эльфов и людей
            case 2:
                return elfFortressPrefab;    // Поменяли местами эльфов и людей
            case 3:
                return undeadFortressPrefab;
            default:
                return null;
        }
    }

    void MarkOccupied(GameObject fortress)
    {
        foreach (Transform child in fortress.transform)
        {
            Vector3Int gridPosition = Vector3Int.RoundToInt(child.position);
            gridObjects[gridPosition.x, gridPosition.y] = child;
        }
    }

    void SpawnResources()
    {
        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                if (gridObjects[x, y] == null)
                {
                    Vector3 position = new Vector3(x, y, 0);

                    if (Random.value < treeProbability)
                    {
                        GameObject tree = Instantiate(GetRandomTile(treePrefabs), position, Quaternion.identity);
                        gridObjects[x, y] = tree.transform;
                    }
                    else if (Random.value < rockProbability)
                    {
                        GameObject rock = Instantiate(GetRandomTile(rockPrefabs), position, Quaternion.identity);
                        gridObjects[x, y] = rock.transform;
                    }
                }
            }
        }
    }

    GameObject GetRandomTile(GameObject[] prefabs)
    {
        int index = Random.Range(0, prefabs.Length);
        return prefabs[index];
    }
}
