using System;
using System.Collections.Generic;
using UnityEngine;

public class UnitManager : MonoBehaviour
{
    public List<Unit> units;  // Список юнитов фракции
    public GameObject tileHighlighterPrefab;
    private GameObject tileHighlighterInstance;
    public GameObject moveRangeIndicatorPrefab;
    public GameObject attackRangeIndicatorPrefab;

    public GameObject[] orcUnits;
    public GameObject[] elfUnits;
    public GameObject[] humanUnits;
    public GameObject[] undeadUnits;

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
                else if (SelectedUnit != null && Vector3.Distance(SelectedUnit.transform.position, hitUnit.transform.position) <= SelectedUnit.attackRange + 0.5f)
                {
                    SelectedUnit.SetTarget(hitUnit.transform);
                    SelectedUnit.Attack();
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
                else if (hitCollider.GetComponent<Building>() != null)
                {
                    Building hitBuilding = hitCollider.GetComponent<Building>();
                    if (SelectedUnit != null && Vector3.Distance(SelectedUnit.transform.position, hitBuilding.transform.position) <= SelectedUnit.attackRange + 1f)
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
            DeselectTower(); // Добавлено снятие выделения с башни
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

        tileHighlighterInstance.transform.position = new Vector3(unit.transform.position.x, unit.transform.position.y + 0.5f, unit.transform.position.z); // Подняли выделение на 0.5 по Y
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
    }

    public void SaveUnits()
    {
        int unitIndex = 0;
        foreach (var unit in units)
        {
            PlayerPrefs.SetInt($"Unit{unitIndex}_PlayerIndex", unit.playerIndex);
            PlayerPrefs.SetInt($"Unit{unitIndex}_UnitIndex", unit.unitIndex); // Индекс типа юнита
            PlayerPrefs.SetInt($"Unit{unitIndex}_Health", unit.health);
            PlayerPrefs.SetInt($"Unit{unitIndex}_AttackPower", unit.attackPower);
            PlayerPrefs.SetFloat($"Unit{unitIndex}_PositionX", unit.transform.position.x);
            PlayerPrefs.SetFloat($"Unit{unitIndex}_PositionY", unit.transform.position.y);
            unitIndex++;
        }
        PlayerPrefs.SetInt("UnitCount", unitIndex);
    }

    public void LoadUnits()
    {
        int unitCount = PlayerPrefs.GetInt("UnitCount", 0);
        for (int i = 0; i < unitCount; i++)
        {
            int playerIndex = PlayerPrefs.GetInt($"Unit{i}_PlayerIndex");
            int unitIndex = PlayerPrefs.GetInt($"Unit{i}_UnitIndex");
            int health = PlayerPrefs.GetInt($"Unit{i}_Health");
            int attackPower = PlayerPrefs.GetInt($"Unit{i}_AttackPower");
            float positionX = PlayerPrefs.GetFloat($"Unit{i}_PositionX");
            float positionY = PlayerPrefs.GetFloat($"Unit{i}_PositionY");

            GameObject unitPrefab = GetUnitPrefab(playerIndex, unitIndex);
            if (unitPrefab != null)
            {
                Vector3 position = new Vector3(positionX, positionY, -0.1f);
                GameObject unitObject = Instantiate(unitPrefab, position, Quaternion.identity);
                Unit unit = unitObject.GetComponent<Unit>();
                unit.playerIndex = playerIndex;
                unit.unitIndex = unitIndex;
                unit.health = health;
                unit.attackPower = attackPower;
                RegisterUnit(unit);
            }
        }
    }

    public GameObject GetUnitPrefab(int playerIndex, int unitIndex)
    {
        switch (playerIndex)
        {
            case 1:
                return orcUnits[unitIndex];
            case 2:
                return humanUnits[unitIndex];
            case 3:
                return undeadUnits[unitIndex];
            case 4:
                return elfUnits[unitIndex];
            default:
                return null;
        }
    }

}
