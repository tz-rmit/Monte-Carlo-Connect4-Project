// Adapted from: https://github.com/SebLague/Pathfinding-2D

using UnityEngine;
using System.Collections.Generic;
using System;
using Unity.PlasticSCM.Editor.WebApi;
using UnityEngine.UI;
using System.Collections;
using System.Linq;

public class Pathfinding : MonoBehaviour
{
	public static AStarGrid grid;
	static Pathfinding instance;
	public float waterCostMultiplier;

	// For path smoothing. (Note: You may need to add more variables here.)
	public bool UsePathSmoothing;

	public LayerMask obstacleMask;
	public LayerMask waterMask;

	void Awake()
	{
		grid = GetComponent<AStarGrid>();
		instance = this;
	}

	public static Node[] RequestPath(Vector2 from, Vector2 to)
	{
		return instance.FindPath(from, to);
	}

	Node[] FindPath(Vector2 from, Vector2 to)
	{
		Node[] waypoints = new Node[0];
		bool pathSuccess = false;

		Node startNode = grid.NodeFromWorldPoint(from);
		Node targetNode = grid.NodeFromWorldPoint(to);
		startNode.parent = startNode;

		if (!startNode.walkable)
		{
			startNode = grid.ClosestWalkableNode(startNode);
		}
		if (!targetNode.walkable)
		{
			targetNode = grid.ClosestWalkableNode(targetNode);
		}

		if (startNode.walkable && targetNode.walkable)
		{
			Heap<Node> openSet = new Heap<Node>(grid.MaxSize);
			HashSet<Node> closedSet = new HashSet<Node>();
			openSet.Add(startNode);

			while (openSet.Count > 0)
			{
				Node currentNode = openSet.RemoveFirst();
				closedSet.Add(currentNode);

				if (currentNode == targetNode)
				{
					pathSuccess = true;
					break;
				}

				foreach (Node neighbour in grid.GetNeighbours(currentNode))
				{
					if (!neighbour.walkable || closedSet.Contains(neighbour))
					{
						continue;
					}

					float newMovementCostToNeighbour = currentNode.gCost + GCost(currentNode, neighbour);

					if (newMovementCostToNeighbour < neighbour.gCost || !openSet.Contains(neighbour))
					{
						neighbour.gCost = newMovementCostToNeighbour;
						neighbour.hCost = Heuristic(neighbour, targetNode);
						neighbour.parent = currentNode;

						if (!openSet.Contains(neighbour))
						{
							openSet.Add(neighbour);
						}
						else
						{
							openSet.UpdateItem(neighbour);
						}
					}
				}
			}
		}

		if (pathSuccess)
		{
			waypoints = RetracePath(startNode, targetNode);
		}

		if (UsePathSmoothing)
		{
			waypoints = SmoothPath(waypoints);
		}

		return waypoints;
	}

	Node[] RetracePath(Node startNode, Node endNode)
	{
		List<Node> path = new List<Node>();
		Node currentNode = endNode;

		while (currentNode != startNode)
		{
			path.Add(currentNode);
			currentNode = currentNode.parent;
		}

		Node[] waypoints = path.ToArray();
		Array.Reverse(waypoints);
		return waypoints;
	}

	private Node[] SmoothPath(Node[] path)
	{
		int currIndex = 0;
		int castIndex = 1;


		while (currIndex < path.Length - 1)
		{
            RaycastHit2D hit = new RaycastHit2D();

            while (!hit && castIndex < path.Length)
			{
                LayerMask mask = obstacleMask;
                // if the current node is not in water add water to the mask
                if (path[currIndex].water == false)
                {
                    mask = mask | waterMask;
                }

                hit = Physics2D.CircleCast(
                    path[currIndex].worldPosition,
                    GameObject.Find("Frog").GetComponent<CircleCollider2D>().radius * GameObject.Find("Frog").transform.localScale.x,
                    path[castIndex].worldPosition - path[currIndex].worldPosition,
                    (path[castIndex].worldPosition - path[currIndex].worldPosition).magnitude,
                    mask);

				if (!hit)
				{
                    // make a b-line for land
                    if (path[currIndex].water && !path[castIndex].water)
                    {
                        castIndex++;
                        break;
                    }
					else
					{
                        castIndex++;
                    }
                }
            }

            path = SmoothSubpath(path, currIndex, castIndex - 1);
            currIndex++;
            castIndex = currIndex + 1;
        }

		return path;
	}

	private float GCost(Node nodeA, Node nodeB)
	{
		// if both nodes are water
		float gCost = 1.0f;
		if (nodeA.water && nodeB.water)
		{
			gCost *= waterCostMultiplier;
		}
		// if only one is water
		else if (nodeA.water || nodeB.water)
		{
			gCost *= waterCostMultiplier / 2.0f;
		}

		// if diagonal
		if (nodeA.gridX != nodeB.gridX && nodeA.gridY != nodeB.gridY)
		{
			gCost *= (float)Math.Sqrt(2);
		}

		return gCost;
	}

	private float Heuristic(Node nodeA, Node nodeB)
	{
		// Manhattan distance
		return Mathf.Abs(nodeB.gridX - nodeA.gridX) + Mathf.Abs(nodeB.gridY - nodeA.gridY);
	}

	// Smooths path between start and end
	// start and end remain unchanged
	private Node[] SmoothSubpath(Node[] path, int start, int end)
	{
        // nothing to do or args are in wrong order
        if (end - start <= 1)
        {
            return path;
        }

        // size of return array
        int newSize = path.Length - ((end - start) - 1);

		Node[] newPath = new Node[newSize];

		// copy path up to and including start index
		for (int i = 0; i <= start; i++)
		{
			newPath[i] = path[i];
		}

		// copy path from end index to the end of the path
		for (int i = start + 1, j = end; j < path.Length; i++, j++)
		{
			newPath[i] = path[j];
		}

		return newPath;
	}

	private string PrintPath(Node[] path)
	{
		string ret = string.Empty;
		foreach (Node node in path)
		{
			ret += node.PrintFormat();
		}

		return ret;
	}
}
