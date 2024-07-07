using System.Collections.Generic;
using UnityEngine;
using UtilsInterface;

namespace PathInterfaces
{
	public delegate bool DIsWalkable(int x, int y);
	public delegate bool DIsGoal(int ax, int ay);
	public delegate float DGetStepCost(int ax, int ay, int bx, int by);

	public interface IPathSearch
	{
		List<Vector2Int> Search(ref Vector2Int start, ref Vector2Int goal);
	}

	public interface IGetNeighbors : IFixedSizeArrayIterator<Location2D>
	{
		void GetNeighbors(int x, int y, ref Location2D[] NeighborsArray);
	}
}