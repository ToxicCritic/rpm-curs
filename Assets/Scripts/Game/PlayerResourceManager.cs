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

    public TMP_Text woodText;
    public TMP_Text stoneText;
    public TMP_Text goldText;

    private List<Mine> mines = new List<Mine>();


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

    public void SaveResourcesToFile(StreamWriter writer)
    {
        writer.WriteLine($"{playerIndex},{wood},{stone},{gold}");
    }


    // Метод для загрузки ресурсов игрока из файла
    public void LoadResourcesFromFile(string filePath)
    {
        try
        {
            using (StreamReader reader = new StreamReader(filePath))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    string[] data = line.Split(',');

                    if (data.Length < 4)
                    {
                        Debug.LogError("Invalid resource data format");
                        continue;
                    }

                    int playerIndex = int.Parse(data[0]);
                    int wood = int.Parse(data[1]);
                    int stone = int.Parse(data[2]);
                    int gold = int.Parse(data[3]);

                    Debug.Log($"Resources for player {playerIndex} loaded: wood={wood}, stone={stone}, gold={gold}");
                }
            }
        }
        catch (IOException ex)
        {
            Debug.LogError($"Error loading resources from file: {ex.Message}");
        }
    }

    public string GetResourcesData(int playerIndex)
    {
        return $"{playerIndex},{wood},{stone},{gold}";
    }

}
