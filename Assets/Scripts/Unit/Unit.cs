using UnityEngine;

public class Unit : MonoBehaviour
{
    public int playerIndex;
    public int unitIndex;
    public int health;
    public int maxHealth;
    public int attackPower;
    public int attackRange;
    public int moveRange;

    public bool hasMoved;
    public bool hasAttacked;
    public float positionX;
    public float positionY;

    private Transform target;

    private SpriteRenderer spriteRenderer;
    private Color originalColor;

    private UnitManager unitManager;
    public PlayerResourceManager resourceManager; 

    public GameObject fullHealthBarPrefab;
    public GameObject threeQuarterHealthBarPrefab;
    public GameObject halfHealthBarPrefab;
    public GameObject quarterHealthBarPrefab;
    public GameObject criticalHealthBarPrefab;

    private GameObject healthBarInstance;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalColor = spriteRenderer.color;
        unitManager = TurnManager.Instance.GetUnitManagerForPlayer(playerIndex); 
        unitManager.RegisterUnit(this);

        UpdateHealthBar();
    }

    void UpdateHealthBar()
    {
        if (healthBarInstance != null)
        {
            Destroy(healthBarInstance);
        }

        float healthPercentage = (float)health / maxHealth;

        if (healthPercentage > 0.8f)
        {
            healthBarInstance = Instantiate(fullHealthBarPrefab, transform);
        }
        else if (healthPercentage > 0.6f)
        {
            healthBarInstance = Instantiate(threeQuarterHealthBarPrefab, transform);
        }
        else if (healthPercentage > 0.4f)
        {
            healthBarInstance = Instantiate(halfHealthBarPrefab, transform);
        }
        else if (healthPercentage > 0.2f)
        {
            healthBarInstance = Instantiate(quarterHealthBarPrefab, transform);
        }
        else 
        {
            healthBarInstance = Instantiate(criticalHealthBarPrefab, transform);

        }

        if (healthBarInstance != null)
        {
            healthBarInstance.transform.localPosition = new Vector3(0, 1.0f, -0.2f);
            Debug.Log($"Health bar created and positioned at: {healthBarInstance.transform.position}");
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
        if (target != null && CanAttack() && Vector3.Distance(transform.position, target.position) <= attackRange + 2)
        {
            Unit targetUnit = target.GetComponent<Unit>();
            if (targetUnit != null)
            {
                Debug.Log($"{gameObject.name} наносит {attackPower} урона {targetUnit.gameObject.name}");
                targetUnit.TakeDamage(attackPower);
                if (targetUnit.health <= 0)
                {
                    Debug.Log($"{targetUnit.gameObject.name} был убит!");
                    transform.position = target.position;
                    unitManager.UnregisterUnit(targetUnit);
                    Destroy(target.gameObject);
                }
                hasAttacked = true;
            }
            else
            {
                Building targetBuilding = target.GetComponent<Building>();
                if (targetBuilding != null)
                {
                    Debug.Log($"{gameObject.name} наносит {attackPower} урона {targetBuilding.gameObject.name}");
                    targetBuilding.TakeDamage(attackPower);
                    if (targetBuilding.health <= 0)
                    {
                        Debug.Log($"{targetBuilding.gameObject.name} было разрушено!");
                        Destroy(targetBuilding.gameObject);
                    }
                    hasAttacked = true;
                }
                else
                {
                    Debug.LogError("Цель не является юнитом или зданием.");
                }
            }
        }
        else
        {
            Debug.LogError("Цель вне досягаемости или атака уже совершена.");
        }
    }

    public void CollectResource()
    {
        if (!hasAttacked && target != null && (target.tag.Contains("Tree") || target.tag.Contains("Rock")) && Vector3.Distance(transform.position, target.position) <= attackRange + 1)
        {
            string resourceType = "";

            if (target.tag.Contains("Tree"))
            {
                resourceType = "wood";
            }
            else if (target.tag.Contains("Rock"))
            {
                resourceType = "stone";
            }

            if (!string.IsNullOrEmpty(resourceType))
            {
                resourceManager.AddResource(resourceType, 10); 
                Destroy(target.gameObject); 
                hasAttacked = true;
                Debug.Log($"{this.name} собрал {resourceType}");
            }
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
            spriteRenderer.color = Color.green;
        }
    }

    public void Deselect()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
        }
    }
}
