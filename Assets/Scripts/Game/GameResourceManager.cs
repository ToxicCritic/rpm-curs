using System.IO;
using UnityEngine;

public class GameResourceManager : MonoBehaviour
{
    public PlayerResourceManager[] playerResourceManagers;

    public void SavePlayerResources(StreamWriter writer)
    {
        foreach (var playerResourceManager in playerResourceManagers)
        {
            writer.WriteLine($"PlayerResources,{playerResourceManager.GetResourcesData(playerResourceManager.playerIndex)}");
        }
    }

    public void LoadPlayerResources(string[] data)
    {
        int playerIndex = int.Parse(data[1]);
        foreach (var playerResourceManager in playerResourceManagers)
        {
            if (playerResourceManager.playerIndex == playerIndex)
            {
                playerResourceManager.LoadPlayerResources(data);
            }
        }
    }

    public PlayerResourceManager GetResourceManagerForPlayer(int playerIndex)
    {
        foreach (var manager in playerResourceManagers)
        {
            if (manager.playerIndex == playerIndex)
            {
                return manager;
            }
        }
        Debug.LogError($"PlayerResourceManager для игрока {playerIndex} не найден.");
        return null;
    }
}
