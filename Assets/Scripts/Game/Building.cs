using UnityEngine;

public class Building : MonoBehaviour
{
    public enum BuildingType
    {
        ResourceGatherer,
        Barracks,
        Mine,
        Tower,
        Fortress // Добавлено крепость
    }

    public BuildingType buildingType;
    public int playerIndex;
    public int health;
    public int maxHealth;
    public bool hasProducedUnit = false;

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
        if (TurnManager.Instance.GetCurrentPlayerIndex() == playerIndex)
        {
            buildingManager.ShowUnitCreationPanel(this);
        }
    }

    public void TakeDamage(int damage)
    {
        health -= damage;
        if (health <= 0)
        {
            Die();
        }
        UpdateHealthBar();
    }

    public void Die()
    {
        buildingManager.UnregisterBuilding(this);
        Destroy(gameObject);
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

        if (buildingType == BuildingType.Fortress)
        {
            healthBarInstance.transform.localPosition = new Vector3(0.5f, 1.8f, -0.2f); // Позиционирование над крепостью
        }
        else
        {
            healthBarInstance.transform.localPosition = new Vector3(0, 0.8f, -0.2f); // Позиционирование над зданием
        }

        Debug.Log($"Health bar created and positioned at: {healthBarInstance.transform.position}");
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
