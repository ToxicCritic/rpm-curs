using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class PlayerResourceManager : MonoBehaviour
{
    public static PlayerResourceManager Instance { get; private set; }

    public int wood;
    public int stone;
    public int gold;

    public TMP_Text woodText;
    public TMP_Text stoneText;
    public TMP_Text goldText;

    private List<Mine> mines = new List<Mine>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        UpdateResourceUI();
    }

    public bool CanAfford(int woodCost, int stoneCost, int goldCost)
    {
        return wood >= woodCost && stone >= stoneCost && gold >= goldCost;
    }

    public bool SpendResource(string resourceType, int amount)
    {
        switch (resourceType)
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

    public void AddResource(string resourceType, int amount)
    {
        switch (resourceType)
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

    private void UpdateResourceUI()
    {
        woodText.text = wood.ToString();
        stoneText.text = stone.ToString();
        goldText.text = gold.ToString();
    }

    public void StartTurn()
    {
        foreach (Mine mine in mines)
        {
            mine.AddGold(this);
        }
        UpdateResourceUI();
    }

    public void EndTurn()
    {
        UpdateResourceUI();
    }

    public void SavePlayerResources(int playerIndex)
    {
        PlayerPrefs.SetInt($"Player{playerIndex}_Wood", wood);
        PlayerPrefs.SetInt($"Player{playerIndex}_Stone", stone);
        PlayerPrefs.SetInt($"Player{playerIndex}_Gold", gold);
    }

    public void LoadPlayerResources(int playerIndex)
    {
        wood = PlayerPrefs.GetInt($"Player{playerIndex}_Wood", 0);
        stone = PlayerPrefs.GetInt($"Player{playerIndex}_Stone", 0);
        gold = PlayerPrefs.GetInt($"Player{playerIndex}_Gold", 0);
        UpdateResourceUI();
    }
}
