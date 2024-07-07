using UnityEngine;
using Assets.Utils;

namespace Assets.World.Clustering
{
	interface ICluster
	{
		// clusters data iff labels[x,y] == label. Returns Graph of 
		GraphNode Cluster(float[,] data, int[,] labels, int label, int[,] clusterMap);
	}
}
