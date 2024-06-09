using UnityEngine;

public class Building : MonoBehaviour
{
    public enum BuildingType
    {
        ResourceGatherer,
        Barracks,
        Mine,
        Tower,
        Fortress
    }

    public BuildingType buildingType;
    public int health;
    public int maxHealth;
    public int playerIndex; // »ндекс игрока, которому принадлежит здание
    private bool hasProducedUnit;

    private BuildingManager buildingManager;

    public GameObject fullHealthBarPrefab;
    public GameObject threeQuarterHealthBarPrefab;
    public GameObject halfHealthBarPrefab;
    public GameObject quarterHealthBarPrefab;
    public GameObject criticalHealthBarPrefab;

    private GameObject healthBarInstance;

    void Start()
    {
        buildingManager = FindObjectOfType<BuildingManager>();
        buildingManager.RegisterBuilding(this);
        UpdateHealthBar();
    }

    void OnMouseDown()
    {
        if (playerIndex == TurnManager.Instance.GetCurrentPlayerIndex())
        {
            buildingManager.ShowUnitCreationPanel(this);
        }
    }

    public void TakeDamage(int damage)
    {
        health -= damage;
        UpdateHealthBar();
        if (health <= 0)
        {
            Die();
        }
    }

    public void Die()
    {
        buildingManager.UnregisterBuilding(this);
        Destroy(gameObject);
    }

    public void UpdateHealthBar()
    {
        if (healthBarInstance != null)
        {
            Destroy(healthBarInstance);
        }

        float healthPercentage = (float)health / maxHealth;

        if (healthPercentage > 0.75f)
        {
            healthBarInstance = Instantiate(fullHealthBarPrefab, transform);
        }
        else if (healthPercentage > 0.5f)
        {
            healthBarInstance = Instantiate(threeQuarterHealthBarPrefab, transform);
        }
        else if (healthPercentage > 0.25f)
        {
            healthBarInstance = Instantiate(halfHealthBarPrefab, transform);
        }
        else if (healthPercentage > 0.1f)
        {
            healthBarInstance = Instantiate(quarterHealthBarPrefab, transform);
        }
        else
        {
            healthBarInstance = Instantiate(criticalHealthBarPrefab, transform);
        }

        if (healthBarInstance != null)
        {
            if (buildingType == BuildingType.Fortress)
            {
                // ≈сли здание крепость, то позиционируем полоску здоровь€ по центру крепости
                healthBarInstance.transform.localPosition = new Vector3(0.5f, 1.8f, -0.2f); // ѕозиционирование по центру крепости
            }
            else
            {
                healthBarInstance.transform.localPosition = new Vector3(0, 0.8f, -0.2f); // ѕозиционирование над зданием
            }
            Debug.Log($"Health bar created and positioned at: {healthBarInstance.transform.position}");
        }
        else
        {
            Debug.LogError("Failed to create health bar instance.");
        }
    }

    public void StartTurn()
    {
        hasProducedUnit = false;
    }

    public bool CanProduceUnit()
    {
        return !hasProducedUnit;
    }

    public void ProduceUnit()
    {
        hasProducedUnit = true;
    }
}
