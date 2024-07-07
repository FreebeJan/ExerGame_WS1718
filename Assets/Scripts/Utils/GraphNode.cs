using System.Collections.Generic;
using UnityEngine;

namespace Assets.Utils
{
	public struct GraphNode
	{
		public int Value;
		public Vector2Int location;
		public List<GraphNode> Neighbours;

		public GraphNode(int value, Vector2Int position)
		{
			Value = value;
			location = position;
			Neighbours = new List<GraphNode>(); // Only for centers
		}

		public void addNeighbours(GraphNode n)
		{
			Neighbours.Add(n);
		}
	}
}
