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
    public int playerIndex; // ������ ������

    public GameObject fullHealthBarPrefab;
    public GameObject threeQuarterHealthBarPrefab;
    public GameObject halfHealthBarPrefab;
    public GameObject quarterHealthBarPrefab;
    public GameObject criticalHealthBarPrefab;

    private GameObject healthBarInstance;

    private BuildingManager buildingManager;

    private bool hasProducedUnit; // ��������� ���������� ��� ������������ ������������ ������

    void Start()
    {
        buildingManager = FindObjectOfType<BuildingManager>();
        if (buildingManager != null)
        {
            buildingManager.RegisterBuilding(this);
        }

        // ������� ������� ��������
        UpdateHealthBar();
    }

    void OnDestroy()
    {
        if (buildingManager != null)
        {
            buildingManager.UnregisterBuilding(this);
        }
    }

    void OnMouseDown()
    {
        if (buildingManager != null && buildingType != BuildingType.Fortress)
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
        else
        {
            UpdateHealthBar();
        }
    }

    public void Die()
    {
        // ���������� ������ ���������� ������
        Destroy(gameObject);
    }

    void UpdateHealthBar()
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

        // ��������, ��� ������� �������� ��������� ��������������� ��� �������
        if (healthBarInstance != null)
        {
            if (buildingType == BuildingType.Fortress)
            {
                healthBarInstance.transform.localPosition = new Vector3(0, 2.0f, -0.2f); // ���������������� ��� ���������
            }
            else
            {
                healthBarInstance.transform.localPosition = new Vector3(0, 1.0f, -0.2f); // ���������������� ��� ������� �������
            }
        }
        else
        {
            Debug.LogError("Failed to create health bar instance.");
        }
    }

    public void StartTurn()
    {
        hasProducedUnit = false; // ����� ��������� ��� ������ ����
    }

    public bool CanProduceUnit()
    {
        return !hasProducedUnit; // ��������, ����� �� ������ ����������� �����
    }

    public void ProduceUnit()
    {
        hasProducedUnit = true; // ��������� ��������� ����� ������������ �����
    }
}
