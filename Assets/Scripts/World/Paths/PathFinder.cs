using UnityEngine;
using System.Collections.Generic;
using PathInterfaces;
using Assets.World.Paths;
using Assets.Utils;
using Assets.World;
using System;

public class PathFinder
{
	Vector2Int UpperLimits, LowerLimits;

	public Vector2Int SearchPerimeterUpper { get; private set; }
	public Vector2Int SearchPermieterLower { get; private set; }

	public VertexHub Hub;

	public int[,] StreetMap;
	public float[,] Heights;
	public int Resolution;
	private int RoadFlatArea, RoadSmoothArea;
	private PathTools.ConnectivityLabel Connectivity;
	private IPathSearch SearchAlgo;
	private DGetStepCost StepCosts;
	private MapTools.Flatten RoadFlatten;
	private MapTools.KernelAppliance TerrainSmoother;
	public List<NavigationPath> paths;
	public WayVertex StartingPoint;

	private Vector2Int CurrentGoal;
	private WayVertex GoalVertex;
	private TerrainChunk Chunk;

	public PathFinder(DGetStepCost stepCosts, TerrainChunk chunk, PathTools.ConnectivityLabel connectivity)
	{
		Chunk = chunk;
		paths = new List<NavigationPath>();
		Hub = new VertexHub(chunk);
		this.Resolution = chunk.Resolution;
		StreetMap = new int[Resolution, Resolution];
		Heights = chunk.Heights;
		RoadFlatArea = 4;
		RoadSmoothArea = 6;
		LowerLimits = new Vector2Int(0, 0);
		UpperLimits = new Vector2Int(Resolution - 1, Resolution - 1);
		SearchPerimeterUpper = UpperLimits;
		SearchPermieterLower = LowerLimits;
		StepCosts = stepCosts;
		Connectivity = connectivity;
		RoadFlatten = new MapTools.Flatten(new MapTools.VariableDistCircle(LowerLimits, UpperLimits, 1, RoadFlatArea), Heights);
		TerrainSmoother = new MapTools.KernelAppliance(
			new MapTools.VariableDistCircle(LowerLimits, UpperLimits, 1, RoadSmoothArea),
			new MapTools.VariableDistCircle(LowerLimits, UpperLimits, 1, 2),
			new OctileDistKernel(),
			Heights
		);
	}

	public void SetSearch(IPathSearch search)
	{
		SearchAlgo = search;
	}

	public bool MakePath(Vector2Int start, Vector2Int end)
	{
		// Check that start and end are on the same label
		if (Connectivity.Labels[start.y, start.x] != Connectivity.Labels[end.y, end.x])
		{
			Assets.Utils.Debug.Log(string.Format("{0} ({2}) and {1} ({3}) are not connected!", start, end, Connectivity.Labels[start.y, start.x], Connectivity.Labels[end.y, end.x]));
			return false;
		}
		float dist = Vector2Int.Distance(start, end);

		// Find the path using the search algorithm
		CurrentGoal = end;
		GoalVertex = null;
		if (Hub.Contains(end))
			GoalVertex = Hub.Get(end);
		List<Vector2Int> path = SearchAlgo.Search(ref start, ref end);

		if (path.Count == 0)
		{
			return false;
		}
		// Remember Path
		AddToStreetMap(path);
		return true;
	}

	/* For use with Pathfinding on heightmap.
     * Doubles the cost of traveling outside of streets. Used to enforce reusage of streets.
     */
	public float StepCostsRoad(int ax, int ay, int bx, int by)
	{
		float boundaryFactor = 1f;
		if (ax == 0 || ay == 0 || ax == Resolution - 1 || ay == Resolution - 1) boundaryFactor *= 4f;
		if (bx == 0 || by == 0 || bx == Resolution - 1 || by == Resolution - 1) boundaryFactor *= 4f;

		if (StreetMap[bx, by] > 0) return boundaryFactor * StepCosts(ax, ay, bx, by);
		if (StreetMap[ax, ay] > 0 && StreetMap[bx, by] < 1) return boundaryFactor * 8 * StepCosts(ax, ay, bx, by);
		return 8f * boundaryFactor * StepCosts(ax, ay, bx, by);
	}

	public bool IsGoal(int ax, int ay)
	{
		return StreetMap[ax, ay] > 0;
	}

	/* Smooth terrain around road with the defined Kernels
     * Flatten road itself with another kernel.
     */
	public void RoadSmooth(Vector2Int linePoint)
	{
		TerrainSmoother.Apply(linePoint.y, linePoint.x);
		RoadFlatten.Apply(linePoint.y, linePoint.x);
	}


	/* Add Path to StreetMap. 
     * If path xy intersetcs existing path za at point i split the paths to xi, iy, zi, ia.
     */
	public void AddToStreetMap(List<Vector2Int> path, int width = 1)
	{
		var WayPoints = new Assets.Utils.LinkedList<Vector2Int>(path);
		PathBranchAutomaton.BranchPaths(WayPoints, StreetMap, ref Hub, paths, Chunk);
	}

	public void CreateNetwork(TerrainChunk terrain, TerrainChunkEdge[] terrainEdges)
	{
		List<Vector2Int> DiscardedPoints = new List<Vector2Int>();
		List<List<Vector2Int>> Points = new List<List<Vector2Int>>();
		for (int i = 0; i < Connectivity.NumLabels; i++)
		{
			Points.Add(new List<Vector2Int>());
		}

		int Offset = 0;
		foreach (TerrainChunkEdge edge in terrainEdges)
		{
			//Assets.Utils.Debug.Log(string.Format("Inspecting edge to {0} from {1}", edge.ChunkPos2, edge.ChunkPos1));
			edge.GenerateRoadPoints();
			foreach (int roadPoint in edge.RoadPoints)
			{
				Vector2Int p = MapTools.UnfoldToPerimeter(roadPoint + Offset, Resolution - 1);
				//Assets.Utils.Debug.Log(string.Format("Got point {0} for {1} and Offset {2}", p, roadPoint, Offset));
				int label = Connectivity.Labels[p.y, p.x];
				if (label >= 0)
				{
					Points[label].Add(p);
				}
				else
				{
					DiscardedPoints.Add(p);
				}
			}
			Offset += Resolution - 1;
		}

		for (int i = 0; i < Connectivity.NumLabels; i++)
		{
			if (Points[i].Count > 0)
			{
				ConnectPoints(Points[i], terrain.ChunkSeed, i);
			}
		}
		int maxLength = 0; // length of the entry path.
		foreach (NavigationPath path in paths)
		{
			path.Finalize(Hub);
			if (maxLength < path.Waypoints.Count)
			{
				StartingPoint = path.Start;
			}
			Assets.Utils.LinkedListNode<Vector2Int> node = path.Waypoints.First;
			int i = 0;
			while (node != null)
			{
				bool success = terrain.Objects.Put(
					new QuadTreeData<ObjectData>(terrain.ToWorldCoordinate(node.Value.x, node.Value.y), QuadDataType.street, new ObjectData { collection = path.Label - 1, label = i })
				);
				if (!success)
				{
					Assets.Utils.Debug.Log(string.Format("Could not add Street at {0} to QuadTree with {1}.", node.Value, terrain.Objects.Boundary));
				}
				node = node.Next;
				i++;
			}
			PathStraighter.Straighten(ref path.Waypoints);
			path.TranslateToWorldSpace(terrain);
		}
	}

	public void FinalizePaths(TerrainChunk terrain)
	{
		foreach (NavigationPath path in paths)
		{
			path.Finalize(Hub);
			path.TranslateToWorldSpace(terrain);
		}
		//paths.Clear();
	}

	private void ConnectPoints(List<Vector2Int> Points, long Seed, int ConnectivityLabel)
	{
		if (Points.Count == 1)
			MakePath(Points[0], Points[0]);
		else if (Points.Count == 2)
		{
			MakePath(Points[0], Points[1]);
		}
		else
		{
			int hash = ConnectivityLabel;
			unchecked
			{
				hash += (int)(5039 * Seed);
			}
			System.Random prng = new System.Random(hash);
			int r1 = prng.Next(Points.Count);
			Points.Sort((b, a) => Vector2Int.Distance(a, Points[r1]).CompareTo(Vector2Int.Distance(b, Points[r1])));
			int f1 = 0;
			for (int i = 0; i < Points.Count; i++)
			{

				if (i == r1) continue;
				if (MakePath(Points[r1], Points[i]))
				{
					f1 = i;
					break;
				}
			}
			for (int k = 0; k < Points.Count; k++)
			{

				if (k == r1 || k == f1) continue;
				MakePath(Points[k], Points[r1]);
			}
		}
	}
}
