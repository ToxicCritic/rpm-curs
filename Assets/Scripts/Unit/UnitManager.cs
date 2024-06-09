using System.Collections.Generic;
using UnityEngine;

public class UnitManager : MonoBehaviour
{
    public List<Unit> units;  // Список юнитов фракции
    public GameObject tileHighlighterPrefab;
    private GameObject tileHighlighterInstance;
    public GameObject moveRangeIndicatorPrefab;
    public GameObject attackRangeIndicatorPrefab;

    private List<GameObject> moveRangeIndicators = new List<GameObject>();
    private List<GameObject> attackRangeIndicators = new List<GameObject>();

    private bool isTurnActive = false;
    public Unit SelectedUnit { get; private set; }

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

    public void RegisterBuilding(Building building)
    {
        // Логика для регистрации здания (если необходимо)
    }

    public void UnregisterBuilding(Building building)
    {
        // Логика для отмены регистрации здания (если необходимо)
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
            }
            else if (hitCollider != null && hitCollider.GetComponent<Building>() != null)
            {
                Building hitBuilding = hitCollider.GetComponent<Building>();
                if (SelectedUnit != null && Vector3.Distance(SelectedUnit.transform.position, hitBuilding.transform.position) <= SelectedUnit.attackRange + 2)
                {
                    SelectedUnit.SetTarget(hitBuilding.transform);
                    SelectedUnit.Attack();
                    DeselectUnit();
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
                if (Mathf.Abs(x) + Mathf.Abs(y) <= moveRange + 2)
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
                if (Mathf.Abs(x) + Mathf.Abs(y) <= attackRange + 2)
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
            unit.StartTurn();
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
}
