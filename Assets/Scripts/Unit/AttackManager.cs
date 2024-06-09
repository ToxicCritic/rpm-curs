using UnityEngine;

public class AttackManager : MonoBehaviour
{
    public void AttackUnit(GameObject attacker, GameObject target)
    {
        Unit attackerUnit = attacker.GetComponent<Unit>();
        IDamageable targetDamageable = target.GetComponent<IDamageable>();

        if (attackerUnit != null && targetDamageable != null)
        {
            if (Vector3.Distance(attacker.transform.position, target.transform.position) <= attackerUnit.attackRange)
            {
                Debug.Log("Атака цели!");
                targetDamageable.TakeDamage(attackerUnit.attackPower);
                attackerUnit.hasAttacked = true;
            }
        }
    }

    public void AttackBuilding(GameObject attacker, GameObject building)
    {
        Unit attackerUnit = attacker.GetComponent<Unit>();
        IDamageable buildingDamageable = building.GetComponent<IDamageable>();

        if (attackerUnit != null && buildingDamageable != null)
        {
            if (Vector3.Distance(attacker.transform.position, building.transform.position) <= attackerUnit.attackRange)
            {
                Debug.Log("Атака здания!");
                buildingDamageable.TakeDamage(attackerUnit.attackPower);
                attackerUnit.hasAttacked = true;
            }
        }
    }
}
