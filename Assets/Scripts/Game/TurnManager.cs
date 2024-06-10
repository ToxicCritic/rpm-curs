using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance { get; private set; }

    public List<UnitManager> unitManagers;
    public List<PlayerResourceManager> playerResourceManagers;
    public BuildingManager buildingManager;
    public Button endTurnButton;
    public TMP_Text turnText;
    private int currentTurnIndex = 1; // Начинаем с 1
    private string[] playerNames = { "Орки", "Люди", "Нежить", "Эльфы" };
    private bool[] activePlayers;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        activePlayers = new bool[playerNames.Length];
        for (int i = 0; i < activePlayers.Length; i++)
        {
            activePlayers[i] = true;
        }
    }

    void Start()
    {
        if (unitManagers.Count != 4 || playerResourceManagers.Count != 4)
        {
            Debug.LogError("UnitManagers and PlayerResourceManagers count should be equal to 4.");
        }

        StartTurn();
        endTurnButton.onClick.AddListener(EndTurn);
    }

    void StartTurn()
    {
        if (activePlayers[currentTurnIndex - 1])
        {
            Debug.Log($"Starting turn for player {currentTurnIndex}");
            unitManagers[currentTurnIndex - 1].StartTurn(currentTurnIndex);
            buildingManager.SetPlayer(currentTurnIndex);
            buildingManager.StartTurn(currentTurnIndex);
            playerResourceManagers[currentTurnIndex - 1].StartTurn();
            UpdateTurnText();
        }
        else
        {
            EndTurn();
        }
    }

    public void EndTurn()
    {
        Debug.Log($"Ending turn for player {currentTurnIndex}");
        unitManagers[currentTurnIndex - 1].EndTurn();
        playerResourceManagers[currentTurnIndex - 1].EndTurn();
        currentTurnIndex = (currentTurnIndex % unitManagers.Count) + 1;
        Debug.Log($"Next turn index: {currentTurnIndex}");
        StartTurn();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            EndTurn();
        }
    }

    void UpdateTurnText()
    {
        if (currentTurnIndex > 0 && currentTurnIndex <= playerNames.Length)
        {
            turnText.text = $"Ход: {playerNames[currentTurnIndex - 1]}";
            Debug.Log($"Updated turn text: {turnText.text}");
        }
        else
        {
            Debug.LogError($"Invalid turn index: {currentTurnIndex}");
        }
    }

    public PlayerResourceManager GetCurrentPlayerResourceManager()
    {
        return playerResourceManagers[currentTurnIndex - 1];
    }

    public int GetCurrentPlayerIndex()
    {
        return currentTurnIndex;
    }

    public UnitManager GetUnitManagerForPlayer(int playerIndex)
    {
        return unitManagers[playerIndex - 1];
    }

    public void DeactivatePlayer(int playerIndex)
    {
        activePlayers[playerIndex - 1] = false;
    }
}
