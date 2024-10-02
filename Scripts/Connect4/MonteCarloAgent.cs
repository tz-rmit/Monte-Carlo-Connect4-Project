using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Purchasing;
using System.Threading;
using System.Linq;
using Unity.IO.LowLevel.Unsafe;

public class MonteCarloAgent : Agent
{
    public int totalSims = 2500;

    public override int GetMove(Connect4State state)
    {
        // figure out what colour I am (yellow = 0)
        int colour = state.GetPlayerTurn();

        List<int> moves = state.GetPossibleMoves();

        if (moves.Count == 1)
        {
            return moves[0];
        }

        // list of the number of wins for each move
        int[] moveWins = new int[7];

        // for each move
        foreach (int move in moves)
        {
            // simulate random moves and store the count of wins
            int sum = 0;
            Connect4State initialState = state.Clone();
            initialState.MakeMove(move);
            for (int i = 0; i < totalSims / moves.Count; i++)
            {
                
                if (RandomSim(initialState) == (Connect4State.Result)colour + 1)
                {
                    sum++;
                }
            }
            moveWins[move] = sum;
        }
        int max = moveWins.Max();
        return moveWins.ToList().IndexOf(max);
    }

    private Connect4State.Result RandomSim(Connect4State state)
    {
        state = state.Clone();
        Connect4State.Result result = state.GetResult();
        while (result == Connect4State.Result.Undecided)
        {
            List<int> moves = state.GetPossibleMoves();
            state.MakeMove(moves[Random.Range(0, moves.Count)]);
            result = state.GetResult();
        }

        return result;
    }
}
