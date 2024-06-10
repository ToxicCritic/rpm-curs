using UnityEngine;

public class Tower : Building
{
    public int attackRange;
    public int attackPower;

    void Start()
    {
        base.Start();
    }

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
}
