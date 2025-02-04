using System.Collections.Generic;
using UnityEngine;

public class TurnCountUIController : MonoBehaviour
{

    [SerializeField] private List<TurnCountUI> turns;
    private int turnCount = 0;
    [SerializeField] private RectTransform container;
    [SerializeField] private int size;
    [SerializeField] private TurnCountUI turnUI;
    // Start is called before the first frame update
    public void CreateTurnOrdes(int size)
    {
        turns = new List<TurnCountUI>();
        for (int i = 0; i < size; i++)
        {
            TurnCountUI turn = Instantiate(turnUI);
            turn.transform.SetParent(container);
            turn.gameObject.transform.localScale = Vector3.one;
            turn.gameObject.SetActive(false);
            turns.Add(turn);
        }
    }
    public void InitalizeTurns(int size)
    {
        ResetTurns();
        for (int i = 0; i < container.childCount; i++)
        {
            if (i < size)
            {
                container.GetChild(i).gameObject.SetActive(true);
            }
        }
        this.size = size;
        turnCount = 0;
    }

    public void Activate(bool isActive)
    {
        if (isActive)
        {
            this.gameObject.SetActive(true);
        }
        else
        {
            this.gameObject.SetActive(false);
        }
    }
    public void CompleteTurn(int movesMade)
    {

        for (int i = 0; i < turns.Count; i++)
        {
            if (i == turnCount)
            {
                turns[i].Activate(true);
            }
            else if (i > turnCount)
            {
                turns[i].Activate(false);

            }
        }
        turnCount += movesMade;
    }
    public void ResetTurns()
    {

        foreach (TurnCountUI turn in turns)
        {
            turn.Activate(false);
        }
        turnCount = 0;
    }
    public void SetTurnCount(int val)
    {
        turnCount = val;
        for (int i = 0; i < turns.Count; i++)
        {
            if (i == turnCount)
            {
                turns[i].Activate(true);
            }
            else
            {
                turns[i].Activate(false);

            }
        }
    }

    public void UndoTurn()
    {
        turnCount--;
        if (turnCount < 0 || turnCount > turns.Count)
        {
            return;
        }
        turns[turnCount].Activate(false);
    }
}
