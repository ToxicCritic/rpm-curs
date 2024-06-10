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
    public TMP_Text victoryText;

    private int currentTurnIndex = 1; // Начинаем с 1
    private string[] playerNames = { "Орки", "Люди", "Нежить", "Эльфы" };
    private bool[] playersActive;

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
    }

    void Start()
    {
        if (unitManagers.Count != 4 || playerResourceManagers.Count != 4)
        {
            Debug.LogError("UnitManagers and PlayerResourceManagers count should be equal to 4.");
        }

        playersActive = new bool[4];
        for (int i = 0; i < 4; i++)
        {
            playersActive[i] = true;
        }

        buildingManager.Initialize();
        StartTurn();
        endTurnButton.onClick.AddListener(EndTurn);
    }

    void StartTurn()
    {
        // Пропускаем ход неактивных игроков
        while (!playersActive[currentTurnIndex - 1])
        {
            currentTurnIndex = (currentTurnIndex % unitManagers.Count) + 1;
        }

        Debug.Log($"Starting turn for player {currentTurnIndex}");
        unitManagers[currentTurnIndex - 1].StartTurn(currentTurnIndex - 1);
        buildingManager.SetPlayer(currentTurnIndex);
        playerResourceManagers[currentTurnIndex - 1].StartTurn();
        UpdateTurnText();
    }

    public void EndTurn()
    {
        Debug.Log($"Ending turn for player {currentTurnIndex}");

        unitManagers[currentTurnIndex - 1].EndTurn();
        playerResourceManagers[currentTurnIndex - 1].EndTurn();

        do
        {
            currentTurnIndex = (currentTurnIndex % unitManagers.Count) + 1;
        } while (!playersActive[currentTurnIndex - 1]);

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

    public void CheckVictoryCondition()
    {
        int activePlayers = 0;
        int lastActivePlayer = -1;

        for (int i = 0; i < playersActive.Length; i++)
        {
            if (playersActive[i])
            {
                activePlayers++;
                lastActivePlayer = i;
            }
        }

        if (activePlayers == 1)
        {
            DeclareVictory(lastActivePlayer + 1);
        }
    }

    void DeclareVictory(int playerIndex)
    {
        victoryText.gameObject.SetActive(true);
        victoryText.text = $"Победитель: {playerNames[playerIndex - 1]}!";
        Debug.Log($"Победитель: {playerNames[playerIndex - 1]}!");

        endTurnButton.gameObject.SetActive(false);
    }

    public void DeactivatePlayer(int playerIndex)
    {
        playersActive[playerIndex - 1] = false;
        CheckVictoryCondition();
    }
}
