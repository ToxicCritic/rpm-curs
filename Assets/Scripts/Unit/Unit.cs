using UnityEngine;

public class Unit : MonoBehaviour
{
    public int playerIndex;
    public int health;
    public int maxHealth;
    public int attackPower;
    public int attackRange;
    public int moveRange;

    public bool hasMoved;
    public bool hasAttacked;
    private Transform target;

    private SpriteRenderer spriteRenderer;
    private Color originalColor;

    private UnitManager unitManager;

    public GameObject fullHealthBarPrefab;
    public GameObject threeQuarterHealthBarPrefab;
    public GameObject halfHealthBarPrefab;
    public GameObject quarterHealthBarPrefab;
    public GameObject oneEighthHealthBarPrefab; // Префаб для 1/8 здоровья

    private GameObject healthBarInstance;

    void Start()
    {
        hasMoved = false;
        hasAttacked = false;
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalColor = spriteRenderer.color;
        unitManager = FindObjectOfType<UnitManager>();
        unitManager.RegisterUnit(this);

        // Создаем полоску здоровья
        UpdateHealthBar();
    }

    void UpdateHealthBar()
    {
        if (healthBarInstance != null)
        {
            Destroy(healthBarInstance);
        }

        float healthPercentage = (float)health / maxHealth;

        if (healthPercentage > 0.875f)
        {
            healthBarInstance = Instantiate(fullHealthBarPrefab, transform);
        }
        else if (healthPercentage > 0.625f)
        {
            healthBarInstance = Instantiate(threeQuarterHealthBarPrefab, transform);
        }
        else if (healthPercentage > 0.375f)
        {
            healthBarInstance = Instantiate(halfHealthBarPrefab, transform);
        }
        else if (healthPercentage > 0.125f)
        {
            healthBarInstance = Instantiate(quarterHealthBarPrefab, transform);
        }
        else
        {
            healthBarInstance = Instantiate(oneEighthHealthBarPrefab, transform);
        }

        if (healthBarInstance != null)
        {
            healthBarInstance.transform.localPosition = new Vector3(0, 1.0f, -0.2f);
        }
        else
        {
            Debug.LogError("Failed to create health bar instance.");
        }
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
        if (CanMove() && CanMoveTo(destination))
        {
            transform.position = destination;
            hasMoved = true;
        }
    }

    public bool CanMoveTo(Vector3 destination)
    {
        float distance = Vector3.Distance(transform.position, destination);
        return Mathf.FloorToInt(distance) <= moveRange;
    }

    public void Attack()
    {
        if (target != null && CanAttack())
        {
            float distance = Vector3.Distance(transform.position, target.position);
            if (distance <= attackRange + 2f)
            {
                Building targetBuilding = target.GetComponent<Building>();
                Unit targetUnit = target.GetComponent<Unit>();

                if (targetBuilding != null)
                {
                    Debug.Log($"{gameObject.name} наносит {attackPower} урона {targetBuilding.gameObject.name}");
                    targetBuilding.TakeDamage(attackPower);
                    if (targetBuilding.health <= 0)
                    {
                        Debug.Log($"{targetBuilding.gameObject.name} был уничтожен!");
                        Destroy(targetBuilding.gameObject);
                    }
                }
                else if (targetUnit != null)
                {
                    Debug.Log($"{gameObject.name} наносит {attackPower} урона {targetUnit.gameObject.name}");
                    targetUnit.TakeDamage(attackPower);
                    if (targetUnit.health <= 0)
                    {
                        Debug.Log($"{targetUnit.gameObject.name} был убит!");
                        unitManager.UnregisterUnit(targetUnit);
                        Destroy(targetUnit.gameObject);
                    }
                }

                hasAttacked = true;
            }
            else
            {
                Debug.LogError("Цель вне досягаемости.");
            }
        }
        else
        {
            Debug.LogError("Цель отсутствует или атака уже совершена.");
        }
    }

    public void TakeDamage(int damage)
    {
        health -= damage;
        UpdateHealthBar();
        Debug.Log($"{gameObject.name} получил {damage} урона, оставшееся здоровье: {health}");
    }

    public void Select()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.green; // Изменяем цвет для выделения
        }
    }

    public void Deselect()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor; // Возвращаем оригинальный цвет
        }
    }
}
