using UnityEngine;
using System.Collections.Generic;

public class TerrainChunkEdge
{
	public List<int> RoadPoints;
	public Vector2Int ChunkPos1, ChunkPos2;
	private readonly long WorldSeed;
	private readonly float CornerAvoidance;
	private readonly int ChunkResolution;

	public override int GetHashCode()
	{
		int hash = 0;

		// to make Edged invariant in order of the inputs sort them here.
		if (ChunkPos1.x > ChunkPos2.x)
		{
			Vector2Int Swap = new Vector2Int(ChunkPos1.x, ChunkPos1.y);
			ChunkPos1 = ChunkPos2;
			ChunkPos2 = Swap;
		}
		if (ChunkPos1.x == ChunkPos2.x && ChunkPos1.y < ChunkPos2.y)
		{
			int yswap = ChunkPos1.y;
			ChunkPos1.y = ChunkPos2.y;
			ChunkPos2.y = yswap;
		}
		// create hash that is withh high probability unique
		unchecked // overflow is not a problem.
		{
			// the multiplicative factors need to be prime.
			hash = ChunkPos1.x;
			hash = ChunkPos1.y + hash * 881;
			hash = 2719 * hash + ChunkPos2.x;
			hash = hash * 2729 + ChunkPos2.y;
			return hash * 3307 + (int)WorldSeed;
		}
	}

	public TerrainChunkEdge(Vector2Int ChunkPos1, Vector2Int ChunkPos2, long WorldSeed, int ChunkResolution, float cornerAvoidance=.10f)
	{
		this.ChunkPos1 = ChunkPos1;
		this.ChunkPos2 = ChunkPos2;
		this.ChunkResolution = ChunkResolution;
		this.WorldSeed = WorldSeed;
		this.CornerAvoidance = cornerAvoidance;
		//Debug.Log(string.Format("Got Hash {0} for edge {1} - {2}", this.GetHashCode(), ChunkPos1, ChunkPos2));
	}

	// Generate points on this edge. Uses the edges hash as seed. IF points are closer to each other then the value indicated by Delta these points will be merged.
	public void GenerateRoadPoints(int MaxRoads = 5)
	{

		float Delta = (1f - 2 * CornerAvoidance) / (float)(MaxRoads/2f);
		System.Random prng = new System.Random(this.GetHashCode());
		int numRoads = prng.Next(1, MaxRoads);
		RoadPoints = new List<int>(numRoads);
		List<float> PointCandidates = new List<float>(numRoads);
		int i = 0;
		while (i < numRoads)
		{
			float candidate = (CornerAvoidance + (float)prng.NextDouble() * (1 -  2f * CornerAvoidance)); // Only allow points that are not on the corners of the terrain.
			if (PointCandidates.Contains(candidate)) continue;
			PointCandidates.Add(candidate);
			i++;
		}

		// Merge Points that are close to each other
		RoadPoints.Sort();
		i = 1;
		int list_length = PointCandidates.Count;
		while (i < list_length)
		{
			if (PointCandidates[i] <= PointCandidates[i - 1] + Delta)
			{
				PointCandidates[i] = (PointCandidates[i] + PointCandidates[i - 1]) / 2f;
				PointCandidates.RemoveAt(i - 1);
				list_length--;
			}
			else
			{
				i++;
			}
		}
		for (int n=0; n < PointCandidates.Count; n++)
		{
			RoadPoints.Add(Mathf.RoundToInt(PointCandidates[n] * ChunkResolution));
		}
	}
}
