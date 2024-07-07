using System.Collections.Generic;
using UnityEngine;
using UtilsInterface;
using PathInterfaces;

public class MapTools
{
	public static bool Aprox(Vector3 a, Vector3 b, float error = 1f)
	{
		return (Mathf.Abs(a.x - b.x) <= error
				&& Mathf.Abs(a.y - b.y) <= error
				&& Mathf.Abs(a.z - b.z) <= error);

	}

	// Draw a line on a grid from a to b
	public static IEnumerable<Vector2Int> BresenhamLine(Vector2Int a, Vector2Int b)
	{
		float delta_x = b.x - a.x;
		float delta_y = b.y - a.y;
		float delta_r = delta_y / delta_x;
		delta_r = delta_r > 0 ? delta_r : -delta_r;
		float error = 0;
		int y_step = delta_y > 0 ? 1 : -1;
		int y = a.y;
		for (int x = a.x; x <= b.x; x++)
		{
			yield return new Vector2Int(x, y);
			error += delta_r;
			while (error > .5f)
			{
				y += y_step;
				error--;
			}
		}
	}

	// Draw a line on a grid from a to b
	public static IEnumerable<Vector2Int> BresenhamOrthogonalLine(Vector2Int a, Vector2Int b)
	{
		float delta_x = b.x - a.x;
		float delta_y = b.y - a.y;

		float delta_r = delta_y / delta_x;
		delta_r = delta_r > 0 ? delta_r : -delta_r;
		float error = 0;
		int y_step = delta_y > 0 ? 1 : -1;
		int y = a.y;
		for (int x = a.x; x <= b.x; x++)
		{
			yield return new Vector2Int(x, y);
			error += delta_r;
			while (error > .5f)
			{
				y += y_step;
				if (x != b.x && y != b.y)
					yield return new Vector2Int(x, y);
				error--;
			}
		}
	}

	//Checks wheter node is in rectangle spanned by lowerLeft and UpperRight
	public static bool InGrid(Vector2 Node, Vector2 lowerLeft, Vector2 UpperRight)
	{
		if (Node.x < UpperRight.x && Node.x >= lowerLeft.x)
		{
			if (Node.y < UpperRight.y && Node.y >= lowerLeft.y) return true;
		}
		return false;
	}

	public class VariableDistNeighbors : IGetNeighbors
	{
		private readonly int lowerX, lowerY, upperX, upperY, StepSize, Width;
		private readonly bool IncludeSelf;

		public VariableDistNeighbors(Vector2Int boundA, Vector2Int boundB, int stepSize = 1, int width = 1, bool includeSelf = false)
		{

			lowerX = Mathf.Min(boundA.x, boundB.x);
			lowerY = Mathf.Min(boundA.y, boundB.y);
			upperX = Mathf.Max(boundA.x, boundB.x);
			upperY = Mathf.Max(boundA.y, boundB.y);
			IncludeSelf = includeSelf;
			Width = width;
			StepSize = stepSize;
		}
		public Location2D[] AllocateArray()
		{
			return new Location2D[(1 + 2 * Width) * (1 + 2 * Width)];
		}

		// Gets all coordinates adjacednt to a given nodes on a grid. gridSize is the resolution of the grid.
		// optionally use: StepSize to jump over direct adjacent nodes. width: How many neighbours of neighbours to include.
		// includeSelf: if true include node in the list of neighbours.
		public void GetNeighbors(int x, int y, ref Location2D[] Neighbors)
		{
			int i = 0;
			for (int xi = x - (Width * StepSize); xi <= x + (Width * StepSize); xi += StepSize)
			{
				for (int yi = y - (Width * StepSize); yi <= y + (Width * StepSize); yi += StepSize)
				{
					Neighbors[i].x = xi;
					Neighbors[i].y = yi;
					Neighbors[i].valid = (Neighbors[i].x >= lowerX && Neighbors[i].x <= upperX
					&& Neighbors[i].y >= lowerY && Neighbors[i].y <= upperY);
					if (Neighbors[i].valid && xi == x && yi == y) Neighbors[i].valid = IncludeSelf;
					i++;
				}
			}
		}
	}

	public class VariableDistCircle : IGetNeighbors
	{
		private readonly int Target;
		private readonly IGetNeighbors NeighborSource;

		public VariableDistCircle(Vector2Int boundA, Vector2Int boundB, int stepSize, int radius, bool includeSelf = true)
		{
			NeighborSource = new VariableDistNeighbors(boundA, boundB, stepSize, radius, includeSelf);
			Target = radius * radius;
		}
		public Location2D[] AllocateArray()
		{
			return NeighborSource.AllocateArray();
		}

		// Gets all coordinates adjacednt to a given nodes on a grid. gridSize is the resolution of the grid.
		// optionally use: StepSize to jump over direct adjacent nodes. width: How many neighbours of neighbours to include.
		// includeSelf: if true include node in the list of neighbours.
		public void GetNeighbors(int x, int y, ref Location2D[] Neighbors)
		{
			NeighborSource.GetNeighbors(x, y, ref Neighbors);
			for (int i = 0; i < Neighbors.Length; i++)
			{
				if (!Neighbors[i].valid) continue;
				Neighbors[i].valid = (Neighbors[i].x - x) * (Neighbors[i].x - x) + (Neighbors[i].y - y) * (Neighbors[i].y - y) <= Target;
			}
		}
	}

	// Applies Kernel to smoothen Data on Neighbourhood.
	public class KernelAppliance
	{
		private readonly IGetNeighbors NeighbourSource, InnerNeighbourSource;
		private readonly IKernel Kernel;
		public float[,] Data;
		private Location2D[] Neighbours, InnerNeighbours;

		public KernelAppliance(IGetNeighbors neighborSource, IGetNeighbors innerNeighborSource, IKernel kernel, float[,] data)
		{
			NeighbourSource = neighborSource;
			InnerNeighbourSource = innerNeighborSource;
			Kernel = kernel;
			Data = data;
			Neighbours = NeighbourSource.AllocateArray();
			InnerNeighbours = InnerNeighbourSource.AllocateArray();
			//Debug.Log(string.Format("Setting up Kernel Appliance. Got Inner Array of Lenght {0} Outer Array {1}", InnerNeighbours.Length, Neighbours.Length));
		}

		public void Apply(int x, int y)
		{
			NeighbourSource.GetNeighbors(x, y, ref Neighbours);
			foreach (Location2D neighbor in Neighbours)
			{
				if (!neighbor.valid) continue;
				InnerNeighbourSource.GetNeighbors(neighbor.x, neighbor.y, ref InnerNeighbours);
				Data[neighbor.x, neighbor.y] = Kernel.ApplyKernel(neighbor.x, neighbor.y, InnerNeighbours, Data);
			}
		}
	}

	public class Flatten
	{
		private readonly IGetNeighbors NeighbourSource;
		public float[,] Data;
		private Location2D[] Neighbours;

		public Flatten(IGetNeighbors neighborSource, float[,] data)
		{
			NeighbourSource = neighborSource;
			Data = data;
			Neighbours = NeighbourSource.AllocateArray();
		}

		public void Apply(int x, int y)
		{
			NeighbourSource.GetNeighbors(x, y, ref Neighbours);
			float avg = 0;
			float count = 0;
			foreach (Location2D neighbor in Neighbours)
			{
				if (!neighbor.valid) continue;
				avg += Data[neighbor.x, neighbor.y];
				count++;
			}
			if (count > 0) avg /= count;
			foreach (Location2D neighbor in Neighbours)
			{
				if (!neighbor.valid) continue;
				Data[neighbor.x, neighbor.y] = avg;
			}
		}
	}


	public class LevelOnCenter
	{
		private readonly IGetNeighbors NeighbourSource;
		public float[,] Data;
		private Location2D[] Neighbours;

		public LevelOnCenter(IGetNeighbors neighborSource, float[,] data)
		{
			NeighbourSource = neighborSource;
			Data = data;
			Neighbours = NeighbourSource.AllocateArray();
		}

		public void Apply(int x, int y)
		{
			NeighbourSource.GetNeighbors(x, y, ref Neighbours);
			float avg = Data[x, y];
			foreach (Location2D neighbor in Neighbours)
			{
				if (!neighbor.valid) continue;
				if (Data[neighbor.x, neighbor.y] >= avg) continue;
				Data[neighbor.x, neighbor.y] = avg;
			}
		}
	}

	// This function is not correct!
	public static float OctileDistance(int ax, int ay, int bx, int by)
	{
		float dx = ax - bx;
		dx = 0 < dx ? dx : -dx;
		float dy = ay - by;
		dy = 0 < dy ? dy : -dy;
		float min = dy < dx ? dy : dx;
		return (dx + dy) + (1.41f - 2f) * min;
		//return 1.41f * (dx < dy ? dx : dy) + (0 < dx - dy ? dx - dy : dy - dx);
	}

	public static Vector2Int UnfoldToPerimeter(int x, int SideLength)
	{
		int side = x / SideLength;
		int pos = x - (side * SideLength);
		switch (side)
		{
			case 0:
				return new Vector2Int(pos, 0);
			case 1:
				return new Vector2Int(SideLength, pos);
			case 2:
				return new Vector2Int(pos, SideLength);
			default:
				return new Vector2Int(0, pos);
		}
	}
}