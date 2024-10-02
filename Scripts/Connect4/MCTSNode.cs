using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;
using UnityEngine.UIElements;

public class MCTSNode
{
    private Connect4State state;
    private float totalPayoff;
    private int timesTried;
    private MCTSNode parentNode;
    private List<MCTSNode> childNodes;
    private int move;

    public MCTSNode(Connect4State state, MCTSNode parentNode, int move)
    {
        this.state = state;
        this.parentNode = parentNode;
        this.move = move;

        this.totalPayoff = 0.0f;
        this.timesTried = 0;
        this.childNodes = new List<MCTSNode>();
    }

    public MCTSNode(Connect4State state, MCTSNode parentNode)
    {
        this.state = state;
        this.parentNode = parentNode;

        this.totalPayoff = 0.0f;
        this.timesTried = 0;
        this.childNodes = new List<MCTSNode>();
        this.move = -1;
    }

    public MCTSNode()
    {
        this.state = null;
        this.parentNode = null;
        this.totalPayoff = 0.0f;
        this.timesTried = 0;
        this.childNodes = new List<MCTSNode>();
        this.move = -1;
    }

    public List<MCTSNode> GetChildren()
    {
        return childNodes;
    }

    public MCTSNode GetParent()
    {
        return parentNode;
    }

    public bool IsFullyExpanded()
    {
        if (state.GetPossibleMoves().Count > childNodes.Count)
        {
            return false;
        }
        return true;
    }

    public int GetTries()
    {
        return timesTried;
    }

    public Connect4State GetState()
    {
        return state;
    }

    public void AddChild(MCTSNode child)
    {
        childNodes.Add(child);
    }

    public void IncrementTries()
    {
        timesTried++;
    }

    public void UpdateAvgPayoff(float result)
    {
        totalPayoff += result;
    }

    public float AvgPayoff()
    {
        return totalPayoff / timesTried;
    }

    public int GetMove()
    {
        return move;
    }

    public void PruneParent()
    {
        parentNode = null;
    }
}