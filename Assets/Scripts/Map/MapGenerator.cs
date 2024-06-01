using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    public GameObject[] orcTiles;    // Префабы для биома орков
    public GameObject[] elfTiles;    // Префабы для биома эльфов
    public GameObject[] humanTiles;  // Префабы для биома людей
    public GameObject[] undeadTiles; // Префабы для биома нежити

    public GameObject[] orcTrees;    // Префабы деревьев для биома орков
    public GameObject[] elfTrees;    // Префабы деревьев для биома эльфов
    public GameObject[] humanTrees;  // Префабы деревьев для биома людей
    public GameObject[] undeadTrees; // Префабы деревьев для биома нежити

    public GameObject[] orcRocks;    // Префабы камней для биома орков
    public GameObject[] elfRocks;    // Префабы камней для биома эльфов
    public GameObject[] humanRocks;  // Префабы камней для биома людей
    public GameObject[] undeadRocks; // Префабы камней для биома нежити

    public int mapWidth = 20;
    public int mapHeight = 20;
    public float treeProbability = 0.05f;  // Вероятность появления дерева на тайле
    public float rockProbability = 0.05f;  // Вероятность появления камня на тайле

    private Transform[,] gridObjects; // Массив для хранения объектов на сетке

    void Start()
    {
        gridObjects = new Transform[mapWidth, mapHeight];
        GenerateMap();
    }

    void GenerateMap()
    {
        int halfWidth = mapWidth / 2;
        int halfHeight = mapHeight / 2;

        // Заполнение каждого квадранта
        FillQuadrant(0, halfWidth, 0, halfHeight, orcTiles, orcTrees, orcRocks);
        FillQuadrant(halfWidth, mapWidth, 0, halfHeight, elfTiles, elfTrees, elfRocks);
        FillQuadrant(0, halfWidth, halfHeight, mapHeight, humanTiles, humanTrees, humanRocks);
        FillQuadrant(halfWidth, mapWidth, halfHeight, mapHeight, undeadTiles, undeadTrees, undeadRocks);
    }

    void FillQuadrant(int startX, int endX, int startY, int endY, GameObject[] tilePrefabs, GameObject[] treePrefabs, GameObject[] rockPrefabs)
    {
        for (int x = startX; x < endX; x++)
        {
            for (int y = startY; y < endY; y++)
            {
                Vector3 position = new Vector3(x, y, 0);
                GameObject tile = Instantiate(GetRandomTile(tilePrefabs), position, Quaternion.identity);
                tile.transform.parent = this.transform;

                // Сохранение объекта в сетке
                gridObjects[x, y] = tile.transform;

                // Добавление дерева с проверкой на вероятность и отсутствие других объектов
                if (Random.value < treeProbability && !HasObject(x, y))
                {
                    Vector3 treePosition = new Vector3(x, y, -0.1f); // Смещение по Z, чтобы дерево было над тайлом
                    GameObject tree = Instantiate(GetRandomTile(treePrefabs), treePosition, Quaternion.identity);
                    tree.transform.parent = tile.transform; // Делаем дерево дочерним объектом тайла
                    gridObjects[x, y] = tree.transform; // Обновляем объект в сетке
                }

                // Добавление камня с проверкой на вероятность и отсутствие других объектов
                if (Random.value < rockProbability && !HasObject(x, y))
                {
                    Vector3 rockPosition = new Vector3(x, y, -0.1f); // Смещение по Z, чтобы камень был над тайлом
                    GameObject rock = Instantiate(GetRandomTile(rockPrefabs), rockPosition, Quaternion.identity);
                    rock.transform.parent = tile.transform; // Делаем камень дочерним объектом тайла
                    gridObjects[x, y] = rock.transform; // Обновляем объект в сетке
                }
            }
        }
    }

    bool HasObject(int x, int y)
    {
        return gridObjects[x, y].childCount > 0;
    }

    GameObject GetRandomTile(GameObject[] prefabs)
    {
        int index = Random.Range(0, prefabs.Length);
        return prefabs[index];
    }
}