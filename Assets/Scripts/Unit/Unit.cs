using UnityEngine;

public class Unit : MonoBehaviour
{
    public int playerIndex;
    public int health;
    public int attackPower;
    public float attackRange;
    public int moveRange;  // Максимальное количество клеток, на которые может перемещаться за один ход

    private bool hasMoved;
    private bool hasAttacked;

    private Transform target;

    void Start()
    {
        hasMoved = false;
        hasAttacked = false;
    }

    public void StartTurn()
    {
        hasMoved = false;
        hasAttacked = false;
    }

    public void EndTurn()
    {
        hasMoved = true;
        hasAttacked = true;
    }

    public bool CanMove()
    {
        return !hasMoved;
    }

    public bool CanAttack()
    {
        return !hasAttacked;
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    public void MoveTo(Vector3 destination)
    {
        if (CanMove() && Vector3.Distance(transform.position, destination) <= moveRange)
        {
            transform.position = destination;
            hasMoved = true;
        }
    }

    public void Attack()
    {
        if (target != null && CanAttack() && Vector3.Distance(transform.position, target.position) <= attackRange)
        {
            // Атаковать цель
            Debug.Log("Атака цели!");
            target.GetComponent<Health>().TakeDamage(attackPower);
            hasAttacked = true;
        }
    }
}
