using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    public GameObject[] orcTiles;    // ������� ��� ����� �����
    public GameObject[] elfTiles;    // ������� ��� ����� ������
    public GameObject[] humanTiles;  // ������� ��� ����� �����
    public GameObject[] undeadTiles; // ������� ��� ����� ������

    public GameObject[] orcTrees;    // ������� �������� ��� ����� �����
    public GameObject[] elfTrees;    // ������� �������� ��� ����� ������
    public GameObject[] humanTrees;  // ������� �������� ��� ����� �����
    public GameObject[] undeadTrees; // ������� �������� ��� ����� ������

    public GameObject[] orcRocks;    // ������� ������ ��� ����� �����
    public GameObject[] elfRocks;    // ������� ������ ��� ����� ������
    public GameObject[] humanRocks;  // ������� ������ ��� ����� �����
    public GameObject[] undeadRocks; // ������� ������ ��� ����� ������

    public int mapWidth = 20;
    public int mapHeight = 20;
    public float treeProbability = 0.05f;  // ����������� ��������� ������ �� �����
    public float rockProbability = 0.05f;  // ����������� ��������� ����� �� �����

    private Transform[,] gridObjects; // ������ ��� �������� �������� �� �����

    void Start()
    {
        gridObjects = new Transform[mapWidth, mapHeight];
        GenerateMap();
    }

    void GenerateMap()
    {
        int halfWidth = mapWidth / 2;
        int halfHeight = mapHeight / 2;

        // ���������� ������� ���������
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

                // ���������� ������� � �����
                gridObjects[x, y] = tile.transform;

                // ���������� ������ � ��������� �� ����������� � ���������� ������ ��������
                if (Random.value < treeProbability && !HasObject(x, y))
                {
                    Vector3 treePosition = new Vector3(x, y, -0.1f); // �������� �� Z, ����� ������ ���� ��� ������
                    GameObject tree = Instantiate(GetRandomTile(treePrefabs), treePosition, Quaternion.identity);
                    tree.transform.parent = tile.transform; // ������ ������ �������� �������� �����
                    gridObjects[x, y] = tree.transform; // ��������� ������ � �����
                }

                // ���������� ����� � ��������� �� ����������� � ���������� ������ ��������
                if (Random.value < rockProbability && !HasObject(x, y))
                {
                    Vector3 rockPosition = new Vector3(x, y, -0.1f); // �������� �� Z, ����� ������ ��� ��� ������
                    GameObject rock = Instantiate(GetRandomTile(rockPrefabs), rockPosition, Quaternion.identity);
                    rock.transform.parent = tile.transform; // ������ ������ �������� �������� �����
                    gridObjects[x, y] = rock.transform; // ��������� ������ � �����
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