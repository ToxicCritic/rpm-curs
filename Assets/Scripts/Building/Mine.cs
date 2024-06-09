using UnityEngine;

public class Mine : MonoBehaviour
{
    public int goldPerTurn = 10;

    public void AddGold(PlayerResourceManager resourceManager)
    {
        resourceManager.AddResource("gold", goldPerTurn);
    }
}
