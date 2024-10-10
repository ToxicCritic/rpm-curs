using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class UnitManager : MonoBehaviour
{
    public List<Unit> units;  
    public GameObject tileHighlighterPrefab;
    private GameObject tileHighlighterInstance;
    public GameObject moveRangeIndicatorPrefab;
    public GameObject attackRangeIndicatorPrefab;
    public GameObject[][] playerUnitPrefabs;
    public GameResourceManager gameResourceManager;

    private List<GameObject> moveRangeIndicators = new List<GameObject>();
    private List<GameObject> attackRangeIndicators = new List<GameObject>();

    private bool isTurnActive = false;
    public Unit SelectedUnit { get; private set; }
    private Tower selectedTower;

    void Start()
    {
        units = new List<Unit>();
        tileHighlighterInstance = Instantiate(tileHighlighterPrefab);
        tileHighlighterInstance.SetActive(false);


    }

    void Update()
    {
        if (isTurnActive)
        {
            HandleInput();
        }
    }

    public void RegisterUnit(Unit unit)
    {
        units.Add(unit);
        unit.resourceManager = gameResourceManager.GetResourceManagerForPlayer(unit.playerIndex);
    }

    public void UnregisterUnit(Unit unit)
    {
        if (units.Contains(unit))
        {
            units.Remove(unit);
        }
    }

    private void HandleInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3Int gridPosition = Vector3Int.FloorToInt(mousePosition);
            Vector3 clickPosition = new Vector3(gridPosition.x + 0.5f, gridPosition.y + 0.5f, 0);

            Collider2D hitCollider = Physics2D.OverlapPoint(clickPosition);

            if (hitCollider != null && hitCollider.GetComponent<Unit>() != null)
            {
                Unit hitUnit = hitCollider.GetComponent<Unit>();
                if (hitUnit.playerIndex == TurnManager.Instance.GetCurrentPlayerIndex())
                {
                    SelectUnit(hitUnit);
                }
                else if (SelectedUnit != null && Vector3.Distance(SelectedUnit.transform.position, hitUnit.transform.position) <= SelectedUnit.attackRange + 0.75f)
                {
                    SelectedUnit.SetTarget(hitUnit.transform);
                    SelectedUnit.Attack();
                    DeselectUnit();
                }
            }
            else if (hitCollider != null && SelectedUnit != null && SelectedUnit.unitIndex == 1)
            {
                string resourceTag = hitCollider.tag;
                if (resourceTag.Contains("Tree") || resourceTag.Contains("Rock"))
                {
                    SelectedUnit.SetTarget(hitCollider.transform);
                    SelectedUnit.CollectResource();
                    DeselectUnit();
                    return;
                }
            }
            else if (hitCollider != null)
            {
                if (hitCollider.GetComponent<Tower>() != null)
                {
                    Tower tower = hitCollider.GetComponent<Tower>();
                    SelectTower(tower);
                    return;
                }
                else if (hitCollider.GetComponent<Building>() != null && hitCollider.GetComponent<Building>().buildingType == Building.BuildingType.Fortress)
                {
                    Building hitBuilding = hitCollider.GetComponent<Building>();
                    if (SelectedUnit != null && Vector3.Distance(SelectedUnit.transform.position, hitBuilding.transform.position) <= SelectedUnit.attackRange + 1.8f)
                    {
                        SelectedUnit.SetTarget(hitBuilding.transform);
                        SelectedUnit.Attack();
                        DeselectUnit();
                    }
                }
                else if (hitCollider.GetComponent<Building>() != null)
                {
                    Building hitBuilding = hitCollider.GetComponent<Building>();
                    if (SelectedUnit != null && Vector3.Distance(SelectedUnit.transform.position, hitBuilding.transform.position) <= SelectedUnit.attackRange + 1.0f)
                    {
                        SelectedUnit.SetTarget(hitBuilding.transform);
                        SelectedUnit.Attack();
                        DeselectUnit();
                    }
                }
            }
            else if (SelectedUnit != null)
            {
                Vector3 destination = new Vector3(Mathf.Floor(clickPosition.x) + 0.5f, Mathf.Floor(clickPosition.y), -0.1f);
                if (SelectedUnit.CanMoveTo(destination))
                {
                    SelectedUnit.MoveTo(destination);
                    DeselectUnit();
                }
                else
                {
                    Debug.Log($"Cannot move to the selected position. Distance: {Vector3.Distance(SelectedUnit.transform.position, destination)}, Move Range: {SelectedUnit.moveRange}");
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            DeselectUnit();
            DeselectTower(); 
        }
    }
        


    private void SelectTower(Tower tower)
    {
        DeselectUnit();
        if (selectedTower != null)
        {
            selectedTower.ClearAttackRangeIndicators();
        }

        selectedTower = tower;
        selectedTower.ShowAttackRange();
    }

    private void DeselectTower()
    {
        if (selectedTower != null)
        {
            selectedTower.ClearAttackRangeIndicators();
            selectedTower = null;
        }
    }

    public void SelectUnit(Unit unit)
    {
        if (SelectedUnit != null)
        {
            SelectedUnit.Deselect();
        }

        SelectedUnit = unit;
        SelectedUnit.Select();

        tileHighlighterInstance.transform.position = new Vector3(unit.transform.position.x, unit.transform.position.y + 0.5f, unit.transform.position.z); 
        tileHighlighterInstance.SetActive(true);

        ShowMoveRangeIndicators(unit);
        ShowAttackRangeIndicators(unit);
    }

    public void DeselectUnit()
    {
        if (SelectedUnit != null)
        {
            SelectedUnit.Deselect();
            SelectedUnit = null;
        }

        tileHighlighterInstance.SetActive(false);
        ClearMoveRangeIndicators();
        ClearAttackRangeIndicators();
    }


    private void ShowMoveRangeIndicators(Unit unit)
    {
        ClearMoveRangeIndicators();

        Vector3 unitPosition = unit.transform.position;
        int moveRange = unit.moveRange;
        float moveZPosition = (unit.attackRange > moveRange) ? -0.2f : -0.1f;

        for (int x = -moveRange; x <= moveRange; x++)
        {
            for (int y = -moveRange; y <= moveRange; y++)
            {
                if (Mathf.Sqrt(x * x + y * y) <= moveRange + 1)
                {
                    Vector3 indicatorPosition = new Vector3(unitPosition.x + x, unitPosition.y + y + 0.5f, moveZPosition);
                    GameObject indicator = Instantiate(moveRangeIndicatorPrefab, indicatorPosition, Quaternion.identity);
                    moveRangeIndicators.Add(indicator);
                }
            }
        }
    }

    private void ShowAttackRangeIndicators(Unit unit)
    {
        ClearAttackRangeIndicators();

        Vector3 unitPosition = unit.transform.position;
        float attackRange = unit.attackRange;
        float attackZPosition = (attackRange > unit.moveRange) ? -0.1f : -0.2f;

        for (int x = Mathf.CeilToInt(-attackRange); x <= Mathf.FloorToInt(attackRange); x++)
        {
            for (int y = Mathf.CeilToInt(-attackRange); y <= Mathf.FloorToInt(attackRange); y++)
            {
                if (Mathf.Sqrt(x * x + y * y) <= attackRange + 0.5f)
                {
                    Vector3 indicatorPosition = new Vector3(unitPosition.x + x, unitPosition.y + y + 0.5f, attackZPosition);
                    GameObject indicator = Instantiate(attackRangeIndicatorPrefab, indicatorPosition, Quaternion.identity);
                    attackRangeIndicators.Add(indicator);
                }
            }
        }
    }

    private void ClearMoveRangeIndicators()
    {
        foreach (GameObject indicator in moveRangeIndicators)
        {
            Destroy(indicator);
        }
        moveRangeIndicators.Clear();
    }

    private void ClearAttackRangeIndicators()
    {
        foreach (GameObject indicator in attackRangeIndicators)
        {
            Destroy(indicator);
        }
        attackRangeIndicators.Clear();
    }

    public void StartTurn(int playerIndex)
    {
        isTurnActive = true;
        foreach (var unit in units)
        {
            if (unit.playerIndex == playerIndex)
            {
                unit.StartTurn();
            }
        }
    }

    public void EndTurn()
    {
        isTurnActive = false;
        foreach (var unit in units)
        {
            unit.EndTurn();
        }
        ClearMoveRangeIndicators();
        ClearAttackRangeIndicators();
        DeselectUnit();
        DeselectTower();
    }

    public void SaveUnitsToFile(StreamWriter writer)
    {
        foreach (var unit in units)
        {
            writer.WriteLine($"UnitData,{unit.playerIndex},{unit.unitIndex},{unit.health},{unit.maxHealth},{unit.attackPower},{unit.attackRange},{unit.moveRange},{unit.hasMoved},{unit.hasAttacked}, {unit.transform.position.x - 0.5f},{unit.transform.position.y}");
        }
    }

    public void LoadUnitsFromFile(string[] data)
    {
        try
        {
            playerUnitPrefabs = new GameObject[4][];

            playerUnitPrefabs[0] = LoadUnitPrefabsForFaction("Orc");
            playerUnitPrefabs[1] = LoadUnitPrefabsForFaction("Human");
            playerUnitPrefabs[2] = LoadUnitPrefabsForFaction("Undead");
            playerUnitPrefabs[3] = LoadUnitPrefabsForFaction("Elf");

            if (data[0] == "UnitData")
            {
                int playerIndex = int.Parse(data[1]);
                int unitIndex = int.Parse(data[2]);
                int health = int.Parse(data[3]);
                int maxHealth = int.Parse(data[4]);
                int attackPower = int.Parse(data[5]);
                int attackRange = int.Parse(data[6]);
                int moveRange = int.Parse(data[7]);
                bool hasMoved = bool.Parse(data[8]);
                bool hasAttacked = bool.Parse(data[9]);
                float positionX = float.Parse(data[10]);
                float positionY = float.Parse(data[11]);

                Vector3 unitPosition = new Vector3(positionX + 0.5f, positionY, -0.1f);

                GameObject unitPrefab = playerUnitPrefabs[playerIndex - 1][unitIndex];

                if (unitPrefab != null)
                {
                    GameObject newUnit = Instantiate(unitPrefab, unitPosition, Quaternion.identity);

                    Unit unitComponent = newUnit.GetComponent<Unit>();
                    if (unitComponent != null)
                    {
                        unitComponent.playerIndex = playerIndex;
                        unitComponent.unitIndex = unitIndex;
                        unitComponent.health = health;
                        unitComponent.maxHealth = maxHealth;
                        unitComponent.attackPower = attackPower;
                        unitComponent.attackRange = attackRange;
                        unitComponent.moveRange = moveRange;
                        unitComponent.hasMoved = hasMoved;
                        unitComponent.hasAttacked = hasAttacked;

                        Debug.Log($"Юнит загружен на позиции ({positionX}, {positionY}) для игрока {playerIndex}.");
                    }
                    else
                    {
                        Debug.LogError("Компонент Unit не найден у созданного объекта.");
                    }
                }
                else
                {
                    Debug.LogError($"Префаб для игрока {playerIndex} и юнита {unitIndex} не найден.");
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Ошибка загрузки юнитов: {ex.Message}");
        }
    }

    private GameObject[] LoadUnitPrefabsForFaction(string factionName)
    {
        GameObject[] unitPrefabs = new GameObject[4];

        for (int i = 0; i < 4; i++)
        {
            string path = $"Prefabs/Troops/{factionName}/{factionName}{i}";
            unitPrefabs[i] = Resources.Load<GameObject>(path);

            if (unitPrefabs[i] == null)
            {
                Debug.LogError($"Не удалось загрузить префаб по пути: {path}");
            }
        }

        return unitPrefabs;
    }
}
