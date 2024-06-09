using UnityEngine;

public class Tower : MonoBehaviour
{
    public int attackPower = 5;
    public float attackRange = 5f;
    private Transform target;

    private void Update()
    {
        FindTarget();
        AttackTarget();
    }

    private void FindTarget()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, attackRange);
        foreach (Collider2D hit in hits)
        {
            Unit unit = hit.GetComponent<Unit>();
            if (unit != null)
            {
                target = unit.transform;
                break;
            }
        }
    }

    private void AttackTarget()
    {
        if (target != null)
        {
            Unit unit = target.GetComponent<Unit>();
            if (unit != null)
            {
                unit.TakeDamage(attackPower);
            }
        }
    }
}
