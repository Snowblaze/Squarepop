using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    private const int MaxGoals = 3;

    public static GameManager instance;
    
    public List<GoalData> goalsList = new List<GoalData>(MaxGoals);

    [SerializeField]
    private int moveCounter;
    [SerializeField]
    private Text moveCounterTxt;
    [SerializeField]
    private Transform goalsPanel;
    [SerializeField]
    private Text endText;
    [SerializeField]
    private GameObject endPanel;
    [SerializeField]
    private Goal goalPrefab;

    public int MoveCounter
    {
        get
        {
            return moveCounter;
        }

        set
        {
            moveCounter = value;
            if (moveCounter <= 0)
                EndGame(false);
            moveCounterTxt.text = moveCounter.ToString();
        }
    }

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);
    }

    private void Start()
    {
        OnGameStart();
    }

    private void OnValidate()
    {
        if (goalsList.Count > MaxGoals)
        {
            Debug.LogWarning("Goals max size is of " + MaxGoals);
            goalsList.RemoveRange(MaxGoals, goalsList.Count - MaxGoals);
        }
    }

    private void EndGame(bool success)
    {
        endText.text = success ? "You won!" : "You lost.";
        endPanel.SetActive(true);
    }

    private void OnGameStart()
    {
        GenerateGoals();
        MoveCounter = moveCounter;
    }

    private void GenerateGoals()
    {
        foreach(var goal in goalsList)
        {
            var goalScript = Instantiate(goalPrefab, goalsPanel);
            goalScript.Init(goal);
            goalScript.OnFinish += CheckIfEnded;
            goalScript.Subscribe(BoardManager.instance.provider);
        }
    }

    private void CheckIfEnded()
    {
        if (goalsList.Any(x => x.Count > 0)) return;

        EndGame(true);
    }
}
