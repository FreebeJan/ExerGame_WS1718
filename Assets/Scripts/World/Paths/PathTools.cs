using UnityEngine;
using System.Collections.Generic;
using PathInterfaces;
using Assets.World.Heightmap;

public static class PathTools
{
	public static bool NodeEquatlity(int ax, int ay, int bx, int by)
	{
		return ax == bx && ay == by;
	}

	public class Bounded8Neighbours : IGetNeighbors
	{
		public int lowerX, lowerY, upperX, upperY;
		private static Vector2Int[] NeigborSteps = new Vector2Int[]
		{
			new Vector2Int(1,0), new Vector2Int(1,1), new Vector2Int(0,1), new Vector2Int(-1,1), new Vector2Int(-1,0),
			new Vector2Int(-1,-1), new Vector2Int(0,-1), new Vector2Int(1, -1)
		};

		public Bounded8Neighbours(ref Vector2Int boundA, ref Vector2Int boundB)
		{
			lowerX = Mathf.Min(boundA.x, boundB.x);
			lowerY = Mathf.Min(boundA.y, boundB.y);
			upperX = Mathf.Max(boundA.x, boundB.x);
			upperY = Mathf.Max(boundA.y, boundB.y);
		}

		public void GetNeighbors(int x, int y, ref Location2D[] Neighbors)
		{
			for (int i = 0; i < NeigborSteps.Length; i++)
			{
				Neighbors[i].x = NeigborSteps[i].x + x;
				Neighbors[i].y = NeigborSteps[i].y + y;
				Neighbors[i].valid = (Neighbors[i].x >= lowerX && Neighbors[i].x <= upperX
					&& Neighbors[i].y >= lowerY && Neighbors[i].y <= upperY);
			}
		}

		public Location2D[] AllocateArray()
		{
			return new Location2D[8];
		}
	}

	public class NormalYThresholdWalkable
	{
		public float thresholdPercentile;
		private Vector3[,] Normals;

		public int lowerX, lowerY, upperX, upperY;

		public NormalYThresholdWalkable(float percentile, Vector3[,] Normals, int Resolution, ref Vector2Int boundA, ref Vector2Int boundB)
		{
			lowerX = Mathf.Min(boundA.x, boundB.x);
			lowerY = Mathf.Min(boundA.y, boundB.y);
			upperX = Mathf.Max(boundA.x, boundB.x);
			upperY = Mathf.Max(boundA.y, boundB.y);
			thresholdPercentile = percentile;
			this.Normals = Normals;
		}

		public bool IsWalkable(int x, int y)
		{
			if (x < lowerX || x > upperX || y < lowerY || y > upperY)
			{
				//Debug.Log(string.Format("{0}, {1} not walkable because it is outside of the grid.", x, y));
				return false;
			}

			return Normals[y, x].y >= thresholdPercentile;
		}
	}

	public class SteepNessThresholdWalkable
	{
		public float thresholdPercentile;
		public float[,] Heights;
		public int lowerX, lowerY, upperX, upperY;
		IGetNeighbors NeighborSource;
		private Location2D[] Neighbours;

		public SteepNessThresholdWalkable(float percentile, float[,] heights, IGetNeighbors neighbors, ref Vector2Int boundA, ref Vector2Int boundB)
		{
			lowerX = Mathf.Min(boundA.x, boundB.x);
			lowerY = Mathf.Min(boundA.y, boundB.y);
			upperX = Mathf.Max(boundA.x, boundB.x);
			upperY = Mathf.Max(boundA.y, boundB.y);
			NeighborSource = neighbors;
			Heights = heights;
			thresholdPercentile = percentile;
			Neighbours = NeighborSource.AllocateArray();
			//Debug.Log(string.Format("lx{0} ly{1} ux{2} ux{3}", lowerX, lowerY, upperX, upperY));
		}

		public bool IsWalkable(int x, int y)
		{
			if (x < lowerX || x > upperX || y < lowerY || y > upperY)
			{
				//Debug.Log(string.Format("{0}, {1} not walkable because it is outside of the grid.", x, y));
				return false;
			}
			NeighborSource.GetNeighbors(x, y, ref Neighbours);
			foreach (Location2D neighbor in Neighbours)
			{
				if ((Heights[y, x] - Heights[neighbor.y, neighbor.x]) > thresholdPercentile)
				{
					//Debug.Log(string.Format("{0}, {1} not walkable because its neighbour ({3}, {4}) has a too big height diff {5} > {6}", x, y, neighbor.x, neighbor.y, Heights[x, y], - Heights[neighbor.x, neighbor.y], thresholdPercentile));
					return false;
				}
			}
			return true;
		}
	}

	public class CachedWalkable
	{
		private DIsWalkable Walkable;
		public bool[,] WalkableCache;
		public int lowerX, lowerY, upperX, upperY;
		public int Resolution;

		public CachedWalkable(DIsWalkable walkable, Vector2Int boundA, Vector2Int boundB, int Resolution)
		{
			this.Resolution = Resolution;
			Walkable = walkable;
			WalkableCache = new bool[Resolution, Resolution];
			lowerX = Mathf.Min(boundA.x, boundB.x);
			lowerY = Mathf.Min(boundA.y, boundB.y);
			upperX = Mathf.Max(boundA.x, boundB.x);
			upperY = Mathf.Max(boundA.y, boundB.y);
			for (int x = lowerX; x <= upperX; x++)
			{
				for (int y = lowerY; y <= upperY; y++)
				{
					WalkableCache[x, y] = Walkable(x, y);
				}
			}
		}

		public bool IsWalkable(int x, int y)
		{
			if (x < lowerX || x > upperX || y < lowerY || y > upperY)
			{
				//Debug.Log(string.Format("{0}, {1} not walkable because it is outside of the grid.", x, y));
				return false;
			}
			return WalkableCache[x, y];
		}
	}

	public static bool AlwaysWalkable(int x, int y)
	{
		return true;
	}

	public class Octile8GridHeightStepCost
	{
		private readonly int Steps, Weight;
		public readonly float[,] Heights;

		public Octile8GridHeightStepCost(int steps, int weight, float[,] heights)
		{
			Steps = steps;
			Heights = heights;
			Weight = weight;
		}

		public float StepCosts(int ax, int ay, int bx, int by)
		{
			float hdiff = Heights[ax, ay] - Heights[bx, by];
			hdiff = hdiff > 0 ? hdiff : -hdiff;
			int cost = 1 + ((int)(hdiff * Steps));
			if ((ax - bx != 0) && (ay - by != 0))
			{
				return 1.41f * cost * Weight;
			}
			return cost * Weight;
		}
	}

	public class Octile8GridSlopeStepCost
	{
		private readonly int Steps, Weight;
		public readonly float[,] Heights;

		public Octile8GridSlopeStepCost(int steps, int weight, float[,] heights)
		{
			Steps = steps * steps;
			Heights = heights;
			Weight = weight;
		}

		public float StepCosts(int ax, int ay, int bx, int by)
		{
			float slope = (Heights[ax, ay] - Heights[bx, by]) * (Heights[ax, ay] - Heights[bx, by]);
			int cost = 1 + ((int)(slope * Steps));
			if ((ax - bx != 0) && (ay - by != 0))
			{
				return 1.41f * cost * Weight;
			}
			return cost * Weight;
		}
	}

	public class ConnectivityLabel
	{
		private DIsWalkable IsWalkable;
		public int[,] Labels;
		public int[] LabelSizes;
		public int NumLabels;
		private int nextLabel;
		private int Resolution;
		private readonly int lowerX, lowerY, upperX;

		public ConnectivityLabel(int Resolution, IGetNeighbors neighborSource, DIsWalkable isWalkable)
		{
			this.Resolution = Resolution;
			Labels = new int[Resolution, Resolution];
			IsWalkable = isWalkable;
			lowerX = 0;
			lowerY = 0;
			upperX = Resolution - 1;
			CalculateLabels();
		}

		public void CalculateLabels()
		{
			int labelCounter = 1;
			int[] Predecessors = new int[4];
			int validPredecessors;
			List<UnionFindNode<int>> UnionFindTree = new List<UnionFindNode<int>>();

			for (int y = 0; y < Resolution; y++)
			{
				for (int x = 0; x < Resolution; x++)
				{
					if (!IsWalkable(x, y))
					{
						Labels[y, x] = -1;
						continue;
					}
					validPredecessors = 0;
					if (x - 1 >= lowerX)
					{
						if (Labels[y, x - 1] > 0) Predecessors[validPredecessors++] = Labels[y, x - 1];
						if (y - 1 >= lowerY)
						{
							if (Labels[y - 1, x - 1] > 0) Predecessors[validPredecessors++] = Labels[y - 1, x - 1];
						}
					}
					if (y - 1 >= lowerY)
					{
						if (Labels[y - 1, x] > 0) Predecessors[validPredecessors++] = Labels[y - 1, x];
						if (x + 1 <= upperX)
						{
							if (Labels[y - 1, x + 1] > 0) Predecessors[validPredecessors++] = Labels[y - 1, x + 1];
						}
					}
					// Cases
					switch (validPredecessors)
					{
						case 0:
							UnionFindTree.Add(new UnionFindNode<int>(labelCounter));
							Labels[y, x] = labelCounter;
							labelCounter++;
							break;
						case 1:
							Labels[y, x] = Predecessors[0];
							break;
						case 2:
							Labels[y, x] = Predecessors[0];
							if (Predecessors[0] != Predecessors[1])
							{
								UnionFindNode<int>.Union(UnionFindTree[Predecessors[0] - 1], UnionFindTree[Predecessors[1] - 1]);
							}
							break;
						case 3:
							Labels[y, x] = Predecessors[0];
							if (Predecessors[0] != Predecessors[1])
							{
								UnionFindNode<int>.Union(UnionFindTree[Predecessors[0] - 1], UnionFindTree[Predecessors[1] - 1]);
							}
							if (Predecessors[0] != Predecessors[2])
							{
								UnionFindNode<int>.Union(UnionFindTree[Predecessors[0] - 1], UnionFindTree[Predecessors[2] - 1]);
							}
							if (Predecessors[1] != Predecessors[2])
							{
								UnionFindNode<int>.Union(UnionFindTree[Predecessors[1] - 1], UnionFindTree[Predecessors[2] - 1]);
							}
							break;
						case 4:
							Labels[y, x] = Predecessors[0];
							if (Predecessors[0] != Predecessors[1])
							{
								UnionFindNode<int>.Union(UnionFindTree[Predecessors[0] - 1], UnionFindTree[Predecessors[1] - 1]);
							}
							if (Predecessors[0] != Predecessors[2])
							{
								UnionFindNode<int>.Union(UnionFindTree[Predecessors[0] - 1], UnionFindTree[Predecessors[2] - 1]);
							}
							if (Predecessors[1] != Predecessors[2])
							{
								UnionFindNode<int>.Union(UnionFindTree[Predecessors[1] - 1], UnionFindTree[Predecessors[2] - 1]);
							}
							if (Predecessors[0] != Predecessors[3])
							{
								UnionFindNode<int>.Union(UnionFindTree[Predecessors[0] - 1], UnionFindTree[Predecessors[3] - 1]);
							}
							if (Predecessors[1] != Predecessors[3])
							{
								UnionFindNode<int>.Union(UnionFindTree[Predecessors[1] - 1], UnionFindTree[Predecessors[3] - 1]);
							}
							if (Predecessors[2] != Predecessors[3])
							{
								UnionFindNode<int>.Union(UnionFindTree[Predecessors[2] - 1], UnionFindTree[Predecessors[3] - 1]);
							}
							break;
					}
				}
			}
			Dictionary<int, int> valueRemap = new Dictionary<int, int>();
			NumLabels = 0;
			for (int i = 0; i < UnionFindTree.Count; i++)
			{
				UnionFindTree[i] = UnionFindTree[i].Find();
				if (!valueRemap.ContainsKey(UnionFindTree[i].Value))
				{
					valueRemap.Add(UnionFindTree[i].Value, NumLabels++);
				}
			}
			LabelSizes = new int[NumLabels];
			/**
             * Debug.Log("Used labels in connectivity map:");
            foreach (int key in valueRemap.Keys)
            {
                Debug.Log(valueRemap[key]);
            }
            **/
			for (int y = 0; y < Resolution; y++)
			{
				for (int x = 0; x < Resolution; x++)
				{
					if (Labels[y, x] >= 0)
					{
						int value = valueRemap[UnionFindTree[Labels[y, x] - 1].Value];
						Labels[y, x] = value;
						LabelSizes[value]++;
					}
				}
			}
		}
	}
}
