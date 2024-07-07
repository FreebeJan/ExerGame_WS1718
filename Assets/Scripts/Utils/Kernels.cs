using UnityEngine;
using UtilsInterface;
using System.Collections.Generic;

public class OctileDistKernel : IKernel
{
	public float ApplyKernel(int x, int y, IEnumerable<Location2D> nodes, float[,] data)
	{
		float avg = 0f;
		float normalizer = 0f;
		foreach (Location2D node in nodes)
		{
			if (!node.valid) continue;
			float dist = MapTools.OctileDistance(x, y, node.x, node.y);
			avg += data[node.x, node.y] / (1f + dist);
			normalizer += 1f / (1f + dist);
		}
		if (normalizer == 0) return 0;
		return avg / normalizer;
	}
}

public class AvgKernel : IKernel
{
	public float ApplyKernel(int x, int y, IEnumerable<Location2D> nodes, float[,] data)
	{
		float avg = 0f;
		float normalizer = 0f;
		foreach (Location2D node in nodes)
		{
			if (!node.valid) continue;
			avg += data[node.x, node.y];
			normalizer += 1f;
		}
		if (normalizer == 0) return 0;
		return avg / normalizer;
	}
}


