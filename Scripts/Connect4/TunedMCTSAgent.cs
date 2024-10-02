using UnityEngine;
using System.Collections.Generic;
using UnityEditor;
using System;
using UnityEditor.Experimental.GraphView;

public class TunedMCTSAgent : Agent
{
    public int totalSims = 2500;
    public float simTime = 1.0f;
    public float C = 1.0f;
    private MCTSNode rootNode = null;

    public override int GetMove(Connect4State state)
    {
        // TODO: Override this method with an MCTS implementation.
        // Currently, it just returns a random move.
        // You can add other methods to the class if you like.

        if (rootNode == null)
        {
            rootNode = new MCTSNode(state.Clone(), null);
        }
        else
        {
            foreach (MCTSNode child in rootNode.GetChildren())
            {
                if (child.GetState().Equals(state))
                {
                    rootNode = child;
                    rootNode.PruneParent();
                    break;
                }
            }
            // there is a case where it will find no child node and perform a very bad move
            // but that would only happen given an extremely low budget
        }
    
        return MCTS();
        
    }

    private int MCTS()
    {
        float time = 0.0f;

        MCTSNode root = rootNode;
        while (time < simTime)
        {
            float startTime = Time.realtimeSinceStartup;

            MCTSNode node = TreePolicy(root);
            Connect4State.Result result = DefualtPolicy(node.GetState());
            Backup(node, result);

            float loopTime = Time.realtimeSinceStartup - startTime;
            time += loopTime;
        }
        rootNode = BestChild(root, 0.0f);
        return rootNode.GetMove();
    }

    private MCTSNode TreePolicy(MCTSNode node)
    {
        MCTSNode traverseNode = node;
        while (traverseNode.GetState().GetResult() == Connect4State.Result.Undecided)
        {
            if (!traverseNode.IsFullyExpanded())
            {
                return Expand(traverseNode);
            }
            else
            {
                traverseNode = BestChild(traverseNode, C);
            }
        }
        return traverseNode;
    }

    private MCTSNode Expand(MCTSNode node)
    {
        Connect4State state = node.GetState().Clone();
        List<int> moves = state.GetPossibleMoves();

        // remove moves already expanded
        List<int> movesCopy = new List<int>(moves);
        foreach (int move in movesCopy)
        {
            foreach (MCTSNode child in node.GetChildren())
            {
                if (child.GetMove() == move)
                {
                    moves.Remove(move);
                }
            }
        }

        int moveToMake = moves[UnityEngine.Random.Range(0, moves.Count)];
        state.MakeMove(moveToMake);

        MCTSNode newNode = new MCTSNode(state, node, moveToMake);
        node.AddChild(newNode);

        return newNode;
    }

    private MCTSNode BestChild(MCTSNode node, float c)
    {
        MCTSNode bestNode = node.GetChildren()[0];
        float bestUCB1 = 0.0f;
        foreach (MCTSNode child in node.GetChildren())
        {
            float UCB1 = child.AvgPayoff() + (c * Mathf.Sqrt((2 * Mathf.Log(node.GetTries())) / child.GetTries()));
            if (UCB1 > bestUCB1)
            {
                bestNode = child;
                bestUCB1 = UCB1;
            }
        }
        return bestNode;
    }

    private Connect4State.Result DefualtPolicy(Connect4State state)
    {
        Connect4State stateCopy = state.Clone();
        while (stateCopy.GetResult() == Connect4State.Result.Undecided)
        {
            List<int> moves = stateCopy.GetPossibleMoves();
            // Switch comments to turn on gaussian random moves
            stateCopy.MakeMove(moves[UnityEngine.Random.Range(0, moves.Count)]);
            //stateCopy.MakeMove(moves[RandomGaussian(0, moves.Count - 1, 0.5f)]);
        }

        return stateCopy.GetResult();
    }

    private void Backup(MCTSNode node, Connect4State.Result result)
    {
        float reward = CalcReward(node, result);
        float enemyReward = RewardOpposite(reward);
        while (node != null)
        {
            node.IncrementTries();
            node.UpdateAvgPayoff(reward);
            node = node.GetParent();
            if (node != null)
            {
                node.IncrementTries();
                node.UpdateAvgPayoff(enemyReward);
                node = node.GetParent();
            }
        }
    }

    private float CalcReward(MCTSNode node, Connect4State.Result result)
    {
        float reward = 0.0f;
        if (node.GetState().GetPlayerTurn() == 1 && result == Connect4State.Result.YellowWin
            || node.GetState().GetPlayerTurn() == 0 && result == Connect4State.Result.RedWin)
        {
            reward = 1.0f;
        }
        else if (result == Connect4State.Result.Draw)
        {
            reward = 0.5f;
        }
        return reward;
    }

    private float RewardOpposite(float reward)
    {
        float opp = 0.0f;
        if (reward == 0.0f)
        {
            opp = 1.0f;
        }
        return opp;
    }

    // Box-Muller method adapted from
    // https://discussions.unity.com/t/normal-distribution-random/66530/2
    public static int RandomGaussian(int minValue, int maxValue, float deviations)
    {
        float u, v, S;

        do
        {
            u = 2.0f * UnityEngine.Random.value - 1.0f;
            v = 2.0f * UnityEngine.Random.value - 1.0f;
            S = u * u + v * v;
        }
        while (S >= 1.0f);

        // Standard Normal Distribution
        float std = u * Mathf.Sqrt(-2.0f * Mathf.Log(S) / S);

        // Normal Distribution centered between the min and max value
        // and clamped following the "three-sigma rule"
        float mean = (minValue + maxValue) / 2.0f;
        float sigma = (maxValue - mean) / deviations;
        return (int)Mathf.Clamp(std * sigma + mean, minValue, maxValue);
    }
}
