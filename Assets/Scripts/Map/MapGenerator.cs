using System.IO;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    public GameObject[] orcTiles;
    public GameObject[] elfTiles;
    public GameObject[] humanTiles;
    public GameObject[] undeadTiles;

    public GameObject orcTree;
    public GameObject elfTree;
    public GameObject humanTree;
    public GameObject undeadTree;

    public GameObject orcRock;
    public GameObject elfRock;
    public GameObject humanRock;
    public GameObject undeadRock;

    public GameObject orcFortressPrefab;
    public GameObject elfFortressPrefab;
    public GameObject humanFortressPrefab;
    public GameObject undeadFortressPrefab;

    public int mapWidth = 30;
    public int mapHeight = 30;
    public float treeProbability = 0.05f;
    public float rockProbability = 0.05f;

    private GameObject[,] gridObjects;
    private BuildingManager buildingManager;

    void Start()
    {
        gridObjects = new GameObject[mapWidth, mapHeight];
        buildingManager = FindObjectOfType<BuildingManager>();

        GenerateMap();
        SpawnFortresses();
        SpawnResources();
    }

    public void GenerateMap()
    {
        int halfWidth = mapWidth / 2;
        int halfHeight = mapHeight / 2;

        FillQuadrant(0, halfWidth, 0, halfHeight, orcTiles);
        FillQuadrant(halfWidth, mapWidth, 0, halfHeight, elfTiles);
        FillQuadrant(0, halfWidth, halfHeight, mapHeight, humanTiles);
        FillQuadrant(halfWidth, mapWidth, halfHeight, mapHeight, undeadTiles);
    }

    void FillQuadrant(int startX, int endX, int startY, int endY, GameObject[] tilePrefabs)
    {
        for (int x = startX; x < endX; x++)
        {
            for (int y = startY; y < endY; y++)
            {
                Vector3 position = new Vector3(x + 0.5f, y + 0.5f, 0);
                GameObject tile = Instantiate(GetRandomTile(tilePrefabs), position, Quaternion.identity);
                tile.transform.parent = this.transform;
                gridObjects[x, y] = tile;
            }
        }
    }

    void SpawnFortresses()
    {
        Vector3[] positions = new Vector3[]
        {
        new Vector3(1.5f, 1.5f, -0.1f),
        new Vector3(1.5f, mapHeight - 2.5f, -0.1f),
        new Vector3(mapWidth - 2.5f, 1.5f, -0.1f),
        new Vector3(mapWidth - 2.5f, mapHeight - 2.5f, -0.1f)
        };

        for (int i = 0; i < 4; i++)
        {
            int race = PlayerPrefs.GetInt($"Player{i}Race", i);
            GameObject fortressPrefab = GetFortressPrefab(race);
            GameObject fortress = Instantiate(fortressPrefab, positions[i], Quaternion.identity);
            fortress.name = $"Player{i}Fortress";
            fortress.transform.parent = this.transform;

            Building building = fortress.GetComponent<Building>();
            if (building != null)
            {
                FindObjectOfType<BuildingManager>().RegisterBuilding(building);
            }

            MarkOccupied(fortress);
        }
    }


    GameObject GetFortressPrefab(int raceIndex)
    {
        switch (raceIndex)
        {
            case 0: return orcFortressPrefab;
            case 1: return humanFortressPrefab;
            case 2: return elfFortressPrefab;
            case 3: return undeadFortressPrefab;
            default: return null;
        }
    }

    void MarkOccupied(GameObject fortress)
    {
        foreach (Transform child in fortress.transform)
        {
            Vector3Int gridPosition = Vector3Int.FloorToInt(child.position - new Vector3(0.5f, 0.5f, 0));
            if (IsValidGridPosition(gridPosition))
            {
                gridObjects[gridPosition.x, gridPosition.y] = fortress;
            }
        }
    }

    void SpawnResources()
    {
        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                if (gridObjects[x, y] != null && gridObjects[x, y].transform.childCount == 0)
                {
                    Vector3 position = new Vector3(x + 0.5f, y + 0.5f, -0.1f);
                    if (Random.value < treeProbability && !IsOccupied(x, y))
                    {
                        GameObject tree = Instantiate(GetTreePrefab(x, y), position, Quaternion.identity);
                        tree.transform.parent = gridObjects[x, y].transform;
                    }
                    else if (Random.value < rockProbability && !IsOccupied(x, y))
                    {
                        GameObject rock = Instantiate(GetRockPrefab(x, y), position, Quaternion.identity);
                        rock.transform.parent = gridObjects[x, y].transform;
                    }
                }
            }
        }
    }

    void SaveMapState(string filePath)
    {
        using (StreamWriter writer = new StreamWriter(filePath))
        {
            // Сохраняем тайлы карты
            SaveTilesToFile(writer);

            // Сохраняем ресурсы карты
            SaveResourcesToFile(writer);
        }
    }

    void LoadMapState(string filePath)
    {
        using (StreamReader reader = new StreamReader(filePath))
        {
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                string[] data = line.Split(',');

                // Определяем, что именно загружаем (тайл или ресурс)
                if (data[0] == "Tile")
                {
                    LoadTilesFromFile(reader, data);
                }
                else if (data[0] == "Resource")
                {
                    LoadResourcesFromFile(reader, data);
                }
            }
        }
    }


    public void SaveResourcesToFile(StreamWriter writer)
    {
        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                // Проверяем, что на клетке есть объект
                if (gridObjects[x, y] != null && gridObjects[x, y].transform.childCount > 0)
                {
                    // Получаем дочерний объект ресурса (если он существует)
                    Transform resourceTransform = gridObjects[x, y].transform.GetChild(0);

                    if (resourceTransform != null)
                    {
                        string resourceType = resourceTransform.gameObject.tag; // Предполагаем, что ресурсы имеют теги "Tree" или "Rock"
                        string race = ""; // Определяем принадлежность к расе

                        // Определяем принадлежность ресурса к расе
                        if (resourceTransform.gameObject == orcTree || resourceTransform.gameObject == orcRock)
                            race = "Orc";
                        else if (resourceTransform.gameObject == elfTree || resourceTransform.gameObject == elfRock)
                            race = "Elf";
                        else if (resourceTransform.gameObject == humanTree || resourceTransform.gameObject == humanRock)
                            race = "Human";
                        else if (resourceTransform.gameObject == undeadTree || resourceTransform.gameObject == undeadRock)
                            race = "Undead";

                        // Сохраняем информацию о ресурсе в файл
                        writer.WriteLine($"Resource,{resourceType},{race},{x},{y}");
                    }
                }
            }
        }
    }

    public void LoadResourcesFromFile(StreamReader reader, string[] data)
    {
        string resourceType = data[1]; // Тип ресурса (Tree или Rock)
        string race = data[2];         // Принадлежность к расе (Orc, Elf, Human, Undead)
        int x = int.Parse(data[3]);    // Координаты x
        int y = int.Parse(data[4]);    // Координаты y

        Vector3 position = new Vector3(x + 0.5f, y + 0.5f, -0.1f);
        GameObject prefab = null;

        // Определяем префаб в зависимости от расы и типа ресурса
        if (race == "Orc")
        {
            prefab = (resourceType == "Tree") ? orcTree : orcRock;
        }
        else if (race == "Elf")
        {
            prefab = (resourceType == "Tree") ? elfTree : elfRock;
        }
        else if (race == "Human")
        {
            prefab = (resourceType == "Tree") ? humanTree : humanRock;
        }
        else if (race == "Undead")
        {
            prefab = (resourceType == "Tree") ? undeadTree : undeadRock;
        }

        // Создаем объект ресурса на карте
        if (prefab != null)
        {
            GameObject resource = Instantiate(prefab, position, Quaternion.identity);
            resource.transform.parent = gridObjects[x, y].transform; // Привязываем к клетке
        }
    }

    public void SaveTilesToFile(StreamWriter writer)
    {
        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                // Проверяем, что на клетке есть объект
                if (gridObjects[x, y] != null)
                {
                    // Определяем тип тайла и его индекс в массиве
                    string tileType = GetTileTypeAndIndex(x, y, out int tileIndex);

                    // Сохраняем информацию о тайле, включая его индекс
                    writer.WriteLine($"Tile,{tileType},{tileIndex},{x},{y}");
                }
            }
        }
    }

    private string GetTileTypeAndIndex(int x, int y, out int tileIndex)
    {
        GameObject tile = gridObjects[x, y];

        // Проверяем, к какому типу относится тайл (по расе и типу тайла)
        if (IsTileFromSet(tile, orcTiles, out tileIndex)) return "OrcTile";
        if (IsTileFromSet(tile, elfTiles, out tileIndex)) return "ElfTile";
        if (IsTileFromSet(tile, humanTiles, out tileIndex)) return "HumanTile";
        if (IsTileFromSet(tile, undeadTiles, out tileIndex)) return "UndeadTile";

        tileIndex = -1;
        return "UnknownTile"; // На случай, если тайл не будет найден в известных наборах
    }

    private bool IsTileFromSet(GameObject tile, GameObject[] tileSet, out int tileIndex)
    {
        for (int i = 0; i < tileSet.Length; i++)
        {
            if (tile.name.Contains(tileSet[i].name))
            {
                tileIndex = i;
                return true;
            }
        }
        tileIndex = -1;
        return false;
    }

    public void LoadTilesFromFile(StreamReader reader, string[] data)
    {
        string tileType = data[1];  // Тип тайла (OrcTile, ElfTile, HumanTile, UndeadTile)
        int tileIndex = int.Parse(data[2]); // Индекс тайла в массиве
        int x = int.Parse(data[3]);         // Координаты x
        int y = int.Parse(data[4]);         // Координаты y

        Vector3 position = new Vector3(x + 0.5f, y + 0.5f, 0);
        GameObject tilePrefab = null;

        // Определяем префаб в зависимости от типа тайла и его индекса
        switch (tileType)
        {
            case "OrcTile":
                tilePrefab = orcTiles[tileIndex];
                break;
            case "ElfTile":
                tilePrefab = elfTiles[tileIndex];
                break;
            case "HumanTile":
                tilePrefab = humanTiles[tileIndex];
                break;
            case "UndeadTile":
                tilePrefab = undeadTiles[tileIndex];
                break;
        }

        if (tilePrefab != null)
        {
            GameObject tile = Instantiate(tilePrefab, position, Quaternion.identity);
            tile.transform.parent = this.transform; // Привязываем к MapGenerator
            gridObjects[x, y] = tile; // Записываем тайл в сетку
        }
    }

    bool IsValidGridPosition(Vector3Int position)
    {
        return position.x >= 0 && position.x < mapWidth && position.y >= 0 && position.y < mapHeight;
    }

    bool IsOccupied(int x, int y)
    {
        return gridObjects[x, y] != null && gridObjects[x, y].transform.childCount > 0;
    }

    GameObject GetTreePrefab(int x, int y)
    {
        if (x < mapWidth / 2 && y < mapHeight / 2) return orcTree;
        if (x >= mapWidth / 2 && y < mapHeight / 2) return elfTree;
        if (x < mapWidth / 2 && y >= mapHeight / 2) return humanTree;
        return undeadTree;
    }

    GameObject GetRockPrefab(int x, int y)
    {
        if (x < mapWidth / 2 && y < mapHeight / 2) return orcRock;
        if (x >= mapWidth / 2 && y < mapHeight / 2) return elfRock;
        if (x < mapWidth / 2 && y >= mapHeight / 2) return humanRock;
        return undeadRock;
    }

    GameObject GetRandomTile(GameObject[] prefabs)
    {
        int index = Random.Range(0, prefabs.Length);
        return prefabs[index];
    }
}
