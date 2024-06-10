using System.Collections.Generic;
using UnityEngine;

public class Tower : Building
{
    public int attackRange;
    public int attackPower;
    public GameObject attackRangeIndicatorPrefab;

    private List<GameObject> attackRangeIndicators = new List<GameObject>();

    public override void EndTurn()
    {
        base.EndTurn();
        AttackNearbyEnemies();
    }

    void AttackNearbyEnemies()
    {
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, attackRange);
        foreach (var hitCollider in hitColliders)
        {
            Unit unit = hitCollider.GetComponent<Unit>();
            if (unit != null && unit.playerIndex != playerIndex)
            {
                unit.TakeDamage(attackPower);
                Debug.Log($"Tower attacked {unit.name} and dealt {attackPower} damage");
            }
        }
    }

    public void ShowAttackRange()
    {
        ClearAttackRangeIndicators();

        Vector3 towerPosition = transform.position;

        for (int x = Mathf.CeilToInt(-attackRange); x <= Mathf.FloorToInt(attackRange); x++)
        {
            for (int y = Mathf.CeilToInt(-attackRange); y <= Mathf.FloorToInt(attackRange); y++)
            {
                if (Mathf.Sqrt(x * x + y * y) <= attackRange + 0.5)
                {
                    Vector3 indicatorPosition = new Vector3(towerPosition.x + x, towerPosition.y + y, -0.2f);
                    GameObject indicator = Instantiate(attackRangeIndicatorPrefab, indicatorPosition, Quaternion.identity);
                    attackRangeIndicators.Add(indicator);
                }
            }
        }
    }

    public void ClearAttackRangeIndicators()
    {
        foreach (GameObject indicator in attackRangeIndicators)
        {
            Destroy(indicator);
        }
        attackRangeIndicators.Clear();
    }
}
