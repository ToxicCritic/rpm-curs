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

    public void LoadPlayerResources(StreamReader reader, string[] data)
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
}
