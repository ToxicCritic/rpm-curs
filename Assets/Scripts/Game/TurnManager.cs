using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TurnManager : MonoBehaviour
{
    public List<Unit>[] playerUnits = new List<Unit>[4];
    private int currentPlayerIndex;
    private int currentUnitIndex;

    public Text currentPlayerText;

    void Start()
    {
        for (int i = 0; i < 4; i++)
        {
            playerUnits[i] = new List<Unit>();
        }

        currentPlayerIndex = 0;
        currentUnitIndex = 0;
        StartTurn();
    }

    void StartTurn()
    {
        UpdateCurrentPlayerText();
        if (playerUnits[currentPlayerIndex].Count > 0)
        {
            playerUnits[currentPlayerIndex][currentUnitIndex].StartTurn();
        }
    }

    void EndTurn()
    {
        if (playerUnits[currentPlayerIndex].Count > 0)
        {
            playerUnits[currentPlayerIndex][currentUnitIndex].EndTurn();
            currentUnitIndex++;

            if (currentUnitIndex >= playerUnits[currentPlayerIndex].Count)
            {
                currentUnitIndex = 0;
                currentPlayerIndex++;
                if (currentPlayerIndex >= playerUnits.Length)
                {
                    currentPlayerIndex = 0;
                }
            }
        }
        else
        {
            currentPlayerIndex++;
            if (currentPlayerIndex >= playerUnits.Length)
            {
                currentPlayerIndex = 0;
            }
        }

        StartTurn();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            EndTurn();
        }
    }

    void UpdateCurrentPlayerText()
    {
        currentPlayerText.text = $"Player {currentPlayerIndex + 1}'s Turn";
    }
}
