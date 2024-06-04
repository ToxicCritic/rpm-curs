using UnityEngine;
using TMPro;

public class ResourceManager : MonoBehaviour
{
    public static ResourceManager Instance;

    public int wood;
    public int stone;
    public int gold;

    public TMP_Text woodText;
    public TMP_Text stoneText;
    public TMP_Text goldText;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Объект сохраняется при смене сцен
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        UpdateResourceUI();
    }

    public void AddResource(string type, int amount)
    {
        switch (type)
        {
            case "wood":
                wood += amount;
                break;
            case "stone":
                stone += amount;
                break;
            case "gold":
                gold += amount;
                break;
        }
        UpdateResourceUI();
    }

    public bool SpendResource(string type, int amount)
    {
        switch (type)
        {
            case "wood":
                if (wood >= amount)
                {
                    wood -= amount;
                    UpdateResourceUI();
                    return true;
                }
                break;
            case "stone":
                if (stone >= amount)
                {
                    stone -= amount;
                    UpdateResourceUI();
                    return true;
                }
                break;
            case "gold":
                if (gold >= amount)
                {
                    gold -= amount;
                    UpdateResourceUI();
                    return true;
                }
                break;
        }
        return false;
    }

    public bool CanAfford(int woodCost, int stoneCost, int goldCost)
    {
        return wood >= woodCost && stone >= stoneCost && gold >= goldCost;
    }

    public bool CanAffordUnit(UnitCost cost)
    {
        return gold >= cost.gold;
    }

    void UpdateResourceUI()
    {
        woodText.text = $"{wood}";
        stoneText.text = $"{stone}";
        goldText.text = $"{gold}";
    }
}