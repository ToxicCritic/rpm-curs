using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.IO;

public class PlayerResourceManager : MonoBehaviour
{
    public int playerIndex;
    public int wood;
    public int stone;
    public int gold;

    public BuildingManager buildingManager;
    public TMP_Text woodText;
    public TMP_Text stoneText;
    public TMP_Text goldText;

    private List<Mine> mines = new List<Mine>();


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
                    buildingManager.UpdateBuildPanel();
                    return true;
                }
                break;
            case "stone":
                if (stone >= amount)
                {
                    stone -= amount;
                    UpdateResourceUI();
                    buildingManager.UpdateBuildPanel();
                    return true;
                }
                break;
            case "gold":
                if (gold >= amount)
                {
                    gold -= amount;
                    UpdateResourceUI();
                    buildingManager.UpdateBuildPanel();
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
        buildingManager.UpdateBuildPanel();
    }

    public void UpdateResourceUI()
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

    public string GetResourcesData(int playerIndex)
    {
        return $"{playerIndex},{wood},{stone},{gold}";
    }

    public void LoadPlayerResources(string[] data)
    {
        if (data.Length >= 4) 
        {
            playerIndex = int.Parse(data[1]);
            wood = int.Parse(data[2]);
            stone = int.Parse(data[3]);
            gold = int.Parse(data[4]);
        }
        else
        {
            Debug.LogError("Ќедостаточно данных дл€ загрузки ресурсов игрока.");
        }
    }
}
