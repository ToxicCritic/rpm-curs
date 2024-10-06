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

    public static int mapWidth = 28;
    public static int mapHeight = 28;
    public float treeProbability = 0.05f;
    public float rockProbability = 0.05f;

    private GameObject[,] gridObjects = new GameObject[mapWidth, mapHeight];
    private BuildingManager buildingManager;

    private static readonly string saveDirectory = Path.Combine(Application.dataPath, "Saves");
    private static readonly string saveFile = Path.Combine(saveDirectory, "game_save.csv");


    void Start()
    {
        gridObjects = new GameObject[mapWidth, mapHeight];
        buildingManager = FindObjectOfType<BuildingManager>();
        if (!File.Exists(saveFile))
        {
            GenerateMap();
            SpawnFortresses();
            SpawnResources();
        }
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
                GameObject tile = gridObjects[gridPosition.x, gridPosition.y];

                if (tile != null)
                {
                    // ������������� �������� ��� �������� ������ �����
                    fortress.transform.parent = tile.transform;

                    Debug.Log($"Fortress placed on tile at {gridPosition.x},{gridPosition.y}.");
                }
                else
                {
                    Debug.LogWarning($"No tile found at {gridPosition.x},{gridPosition.y} to place the fortress!");
                }
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

                    // ��������� ��������� ����� ��������� ��� ������ ��������
                    if (!IsFortressOccupying(x, y))
                    {
                        if (Random.value < treeProbability)
                        {
                            GameObject tree = Instantiate(GetTreePrefab(x, y), position, Quaternion.identity);
                            tree.transform.parent = gridObjects[x, y].transform;
                        }
                        else if (Random.value < rockProbability)
                        {
                            GameObject rock = Instantiate(GetRockPrefab(x, y), position, Quaternion.identity);
                            rock.transform.parent = gridObjects[x, y].transform;
                        }
                    }
                }
            }
        }
    }


    bool IsFortressOccupying(int x, int y)
    {
        // �������� ����� ������ (����� ��������)
        if (IsOccupied(x, y)) return true;

        // �������� ������ ������
        if (IsValidGridPosition(new Vector3Int(x, y + 1, 0)) && IsOccupied(x, y + 1)) return true;

        // �������� ������ �����
        if (IsValidGridPosition(new Vector3Int(x - 1, y, 0)) && IsOccupied(x - 1, y)) return true;

        // �������� ������ ������ �����
        if (IsValidGridPosition(new Vector3Int(x - 1, y + 1, 0)) && IsOccupied(x - 1, y + 1)) return true;

        return false;
    }



    public void SaveResourcesToFile(StreamWriter writer)
    {
        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                // ���������, ��� �� ������ ���� ������
                if (gridObjects[x, y] != null && gridObjects[x, y].transform.childCount > 0)
                {
                    // �������� �������� ������ ������� (���� �� ����������)
                    Transform resourceTransform = gridObjects[x, y].transform.GetChild(0);

                    if (resourceTransform != null)
                    {
                        string resourceType = resourceTransform.gameObject.tag; // �������� ��� �������
                        string race = ""; // ���������� �������������� � ����

                        // ���������� �������������� ������� � ���� �� ������ ����
                        if (resourceType.Contains("orc"))
                            race = "Orc";
                        else if (resourceType.Contains("elf"))
                            race = "Elf";
                        else if (resourceType.Contains("human"))
                            race = "Human";
                        else if (resourceType.Contains("undead"))
                            race = "Undead";

                        // ��������� ���������� � ������� � ����
                        writer.WriteLine($"Resource,{resourceType},{race},{x},{y}");
                    }
                }
            }
        }
    }


    public void LoadResourcesFromFile(string[] data)
    {
        string resourceType = data[1]; // ��� ������� (Tree ��� Rock)
        string race = data[2];         // �������������� � ���� (Orc, Elf, Human, Undead)
        int x = int.Parse(data[3]);    // ���������� x
        int y = int.Parse(data[4]);    // ���������� y

        Vector3 position = new Vector3(x + 0.5f, y + 0.5f, -0.1f);
        GameObject prefab = null;

        // ���������� ������ � ����������� �� ���� � ���� �������
        if (race == "Orc")
        {
            prefab = (resourceType == "orcTree") ? orcTree : orcRock;
        }
        else if (race == "Elf")
        {
            prefab = (resourceType == "elfTree") ? elfTree : elfRock;
        }
        else if (race == "Human")
        {
            prefab = (resourceType == "humanTree") ? humanTree : humanRock;
        }
        else if (race == "Undead")
        {
            prefab = (resourceType == "undeadTree") ? undeadTree : undeadRock;
        }

        // ������� ������ ������� �� �����
        if (prefab != null)
        {
            GameObject resource = Instantiate(prefab, position, Quaternion.identity);
            resource.transform.parent = gridObjects[x, y].transform; // ����������� � ������
        }
    }

    public void SaveTilesToFile(StreamWriter writer)
    {
        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                // ���������, ��� �� ������ ���� ������
                if (gridObjects[x, y] != null)
                {
                    // ���������� ��� ����� � ��� ������ � �������
                    string tileType = GetTileTypeAndIndex(x, y, out int tileIndex);

                    // ��������� ���������� � �����, ������� ��� ������
                    writer.WriteLine($"Tile,{tileType},{tileIndex},{x},{y}");
                }
            }
        }
    }

    private string GetTileTypeAndIndex(int x, int y, out int tileIndex)
    {
        GameObject tile = gridObjects[x, y];

        // ���������, � ������ ���� ��������� ���� (�� ���� � ���� �����)
        if (IsTileFromSet(tile, orcTiles, out tileIndex)) return "OrcTile";
        if (IsTileFromSet(tile, elfTiles, out tileIndex)) return "ElfTile";
        if (IsTileFromSet(tile, humanTiles, out tileIndex)) return "HumanTile";
        if (IsTileFromSet(tile, undeadTiles, out tileIndex)) return "UndeadTile";

        tileIndex = -1;
        return "UnknownTile"; // �� ������, ���� ���� �� ����� ������ � ��������� �������
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

    public void LoadTilesFromFile(string[] data)
    {
        string tileType = data[1];  // ��� ����� (OrcTile, ElfTile, HumanTile, UndeadTile)
        int tileIndex = int.Parse(data[2]); // ������ ����� � �������
        int x = int.Parse(data[3]);         // ���������� x
        int y = int.Parse(data[4]);         // ���������� y

        Vector3 position = new Vector3(x + 0.5f, y + 0.5f, 0);
        GameObject tilePrefab = null;

        // ���������� ������ � ����������� �� ���� ����� � ��� �������
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
            tile.transform.parent = this.transform; // ����������� � MapGenerator
            gridObjects[x, y] = tile; // ���������� ���� � �����
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
