// Adapted from: https://github.com/SebLague/Pathfinding-2D

using UnityEngine;

public class Node : IHeapItem<Node> {
	
	public bool walkable;
	public bool water;
	public Vector2 worldPosition;
	public int gridX;
	public int gridY;

	public float gCost;
	public float hCost;
	public Node parent;
	int heapIndex;
	
	public Node(bool _walkable, bool _water, Vector2 _worldPos, int _gridX, int _gridY)
	{
		walkable = _walkable;
		water = _water;
		worldPosition = _worldPos;
		gridX = _gridX;
		gridY = _gridY;
	}

	public Node Clone()
	{
		return new Node(walkable, water, worldPosition, gridX, gridY);
	}

	public float fCost
	{
		get {
			return gCost + hCost;
		}
	}

	public int HeapIndex
	{
		get {
			return heapIndex;
		}
		set {
			heapIndex = value;
		}
	}

	public int CompareTo(Node nodeToCompare)
	{
		int compare = fCost.CompareTo(nodeToCompare.fCost);
		if (compare == 0)
		{
			compare = hCost.CompareTo(nodeToCompare.hCost);
		}
		return -compare;
	}

	public string PrintFormat()
	{
		return "(" + gridX.ToString() + ", " + gridY.ToString() + ")";
	}
}
