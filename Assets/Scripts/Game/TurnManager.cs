using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Collections;
using UnityEngine.SceneManagement;

public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance { get; private set; }

    public List<UnitManager> unitManagers;
    public List<PlayerResourceManager> playerResourceManagers;
    public BuildingManager buildingManager; 
    public Button endTurnButton;
    public TMP_Text turnText;

    public TMP_Text gameOverText;
    public Image gameOverOutline;
    public Image gameOverBackground;

    public int currentTurnIndex = 1; 
    private string[] playerNames = { "Орки", "Люди", "Нежить", "Эльфы" };

    public HashSet<int> activePlayers = new HashSet<int> { 1, 2, 3, 4 };

    private static readonly string saveDirectory = Path.Combine(Application.dataPath, "Saves");
    private static readonly string saveFile = Path.Combine(saveDirectory, "game_save.csv");

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

        if (!Directory.Exists(saveDirectory))
        {
            Directory.CreateDirectory(saveDirectory);
            Debug.Log($"Save directory created at {saveDirectory}");
        }
    }

    void Start()
    {
        if (unitManagers.Count != 4 || playerResourceManagers.Count != 4)
        {
            Debug.LogError("UnitManagers and PlayerResourceManagers count should be equal to 4.");
        }
        currentTurnIndex = PlayerPrefs.GetInt("SelectedRace", 1);
        StartTurn();
        playerResourceManagers[currentTurnIndex - 1].UpdateResourceUI();
        endTurnButton.onClick.AddListener(EndTurn);
    }

    void StartTurn()
    {
        if (!activePlayers.Contains(currentTurnIndex))
        {
            EndTurn(); 
            return;
        }

        Debug.Log($"Starting turn for player {currentTurnIndex}");
        unitManagers[currentTurnIndex - 1].StartTurn(currentTurnIndex); 
        buildingManager.SetPlayer(currentTurnIndex); 
        buildingManager.StartTurnForBuildings(currentTurnIndex);
        playerResourceManagers[currentTurnIndex - 1].StartTurn();
        UpdateTurnText();
    }

    public void EndTurn()
    {
        Debug.Log($"Ending turn for player {currentTurnIndex}");
        unitManagers[currentTurnIndex - 1].EndTurn();
        buildingManager.EndTurnForBuildings(currentTurnIndex);
        playerResourceManagers[currentTurnIndex - 1].EndTurn(); 

        currentTurnIndex = GetNextActivePlayer();

        if (currentTurnIndex == -1)
        {
            Debug.LogError("No active players left!");
            return;
        }

        Debug.Log($"Next turn index: {currentTurnIndex}");
        StartTurn();
    }

    private int GetNextActivePlayer()
    {
        int nextTurnIndex = (currentTurnIndex % unitManagers.Count) + 1;

        while (!activePlayers.Contains(nextTurnIndex))
        {
            nextTurnIndex = (nextTurnIndex % unitManagers.Count) + 1;

            if (nextTurnIndex == currentTurnIndex)
            {
                return -1;
            }
        }

        return nextTurnIndex;
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
        if (activePlayers == null)
        {
            Debug.LogError("ActivePlayers is null before attempting to deactivate a player.");
        }
        else
        {
            activePlayers.Remove(playerIndex);
            Debug.Log($"Player {playerNames[playerIndex - 1]} has been deactivated.");
        }

        if (activePlayers.Count == 1)
        {
            int lastActivePlayer = activePlayers.First();
            gameOverText.text = $"Игра окончена! Победитель: {playerNames[lastActivePlayer - 1]}";
            gameOverBackground.gameObject.SetActive(true);
            gameOverOutline.gameObject.SetActive(true);
            turnText.gameObject.SetActive(false);
            endTurnButton.gameObject.SetActive(false);
            Debug.Log($"Game over! Player {lastActivePlayer} has won the game!");
            buildingManager.DestroyPlayerBuildingsAndUnits(lastActivePlayer);

            StartCoroutine(EndGameRoutine());
        }
    }

    private IEnumerator EndGameRoutine()
    {
        yield return new WaitForSeconds(5);

        if (File.Exists(saveFile))
        {
            File.Delete(saveFile);
            Debug.Log("Game save file deleted.");
        }

        SceneManager.LoadScene("MainMenu");
    }

    public void SaveTurnToFile(StreamWriter writer)
    {
        writer.WriteLine($"TurnData,{currentTurnIndex}");
        foreach (int player in activePlayers)
        {
            writer.WriteLine($"ActivePlayer,{player}");
        }

    }

    public void LoadTurnFromFile(string[] data)
    {
        if (data[0] == "TurnData")
        {
            PlayerPrefs.SetInt("SelectedRace", int.Parse(data[1]));
            Debug.Log($"Ход игрока загружен: {currentTurnIndex}");
        }
        else if (data[0] == "ActivePlayer")
        {
            activePlayers.Add(int.Parse(data[1]));
        }
    }
}
