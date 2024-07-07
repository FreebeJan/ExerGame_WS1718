using Assets.Utils;
using Assets.World;
using Assets.World.Heightmap;
using Assets.World.Jumps;
using Assets.World.Paths;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class TerrainChunk
{
	public Vector2Int GridCoords;

	public Vector3 TerrainPos;
	public Vector2 TerrainOffset;

	public int ChunkSeed;

	public int Resolution { get; private set; }

	public int FlushedJumps;

	public float[,] Heights, Moisture;
	public int[,] ClusterMap;
	public Vector3[,] Normals;
	public int ClusterCount;
	public Terrain ChunkTerrain;

	private long WorldSeed;
	public TerrainChunkEdge[] TerrainEdges;
	public GameSettings Settings;
	public QuadTree<ObjectData> Objects;
	private TerrainData ChunkTerrainData;

	public float[,,] SplatmapData;

	private VegetationGenerator vGen;
	private PathFinder paths;
	public PathTools.ConnectivityLabel Connectivity;
	public GameObject UnityTerrain;

	public List<TreeInstance> Trees { get; private set; }
	internal List<JumpData> JumpList { get; private set; }
	internal List<GameObject> Items { get; private set; }
	public int LOD { get; private set; }

	public bool isFinished = false;
	public GrasField grasField;

	public TerrainChunk N, E, S, W;
	private int FlushStep;
	internal bool needsUnload;
	internal bool isReplacement;

	public override int GetHashCode()
	{
		unchecked
		{
			int hash = GridCoords.x;
			hash = GridCoords.y + hash * 881;
			return hash * 2719 + (int)WorldSeed;
		}
	}

	public TerrainChunk(GameSettings settings)
	{
		Settings = settings;
		WorldSeed = Settings.WorldSeed;
		vGen = new VegetationGenerator();
		Heights = new float[0, 0];
		SplatmapData = new float[0, 0, 0];
		Trees = new List<TreeInstance>(0);
	}

	public RectangleBound GetBoundary()
	{
		float halfSize = (float)Settings.Size / 2f;
		Vector2 center = new Vector2(GridCoords.x + .5f, GridCoords.y + .5f) * (float)Settings.Size;
		return new RectangleBound(center, halfSize);
	}

	public Vector2 ToWorldCoordinate(int x, int y)
	{
		return new Vector2(
			GridCoords.x + ((float)x + .5f) / (float)Resolution,
			GridCoords.y + ((float)y + .5f) / (float)Resolution
		) * (float)Settings.Size;
	}

	public Vector2 ToWorldCoordinate(float x, float y)
	{
		return new Vector2(GridCoords.x + x, GridCoords.y + y) * (float)Settings.Size;
	}

	public Vector2Int ToLocalCoordinate(float x, float z)
	{
		int ix = Mathf.RoundToInt((x / (float)Settings.Size - GridCoords.x) * (float)Resolution - .5f);
		int iy = Mathf.RoundToInt((z / (float)Settings.Size - GridCoords.y) * (float)Resolution - .5f);
		return new Vector2Int(ix, iy);
	}

	public void ToWorldCoordinate(int x, int y, ref Vector2 Out)
	{
		Out.x = GridCoords.x + ((float)x + .5f) / (float)Resolution;
		Out.y = GridCoords.y + ((float)y + .5f) / (float)Resolution;
		Out *= (float)Settings.Size;
	}

	public void ToWorldCoordinate(float x, float y, ref Vector2 Out)
	{
		Out.x = GridCoords.x + x;
		Out.y = GridCoords.y + y;
		Out *= (float)Settings.Size;
	}


	public void Build(Vector2Int gridCoords, Vector3 PlayerPos, int? ForceLOD = null)
	{
		if (ForceLOD == null)
		{
			LOD = Settings.RetrieveLOD(gridCoords, PlayerPos);
		}
		else
		{
			if (ForceLOD.Value >= Settings.TerrainLOD.Length)
			{
				LOD = Settings.TerrainLOD.Length - 1;
			}
			else if (ForceLOD.Value < 0)
			{
				LOD = 0;
			}
			else
			{
				LOD = ForceLOD.Value;
			}

		}
		GridCoords = gridCoords;
		TerrainPos = new Vector3(GridCoords.x, 0, GridCoords.y) * (float)Settings.Size;
		TerrainOffset = new Vector3(GridCoords.x, GridCoords.y) * (float)Settings.Size;
		ChunkSeed = GetHashCode();

		Resolution = Settings.TerrainLOD[LOD].HeightmapResolution;
		Objects = new QuadTree<ObjectData>(GetBoundary());
		isFinished = false;
		FlushStep = 0;
		/**
		Stopwatch stopWatch = new Stopwatch();
		stopWatch.Start();
		**/
		// Resetting all the Arrays
		Heights = new float[Resolution, Resolution];
		Normals = new Vector3[Resolution, Resolution];
		Moisture = new float[Resolution, Resolution];
		SplatmapData = new float[Resolution, Resolution, GameSettings.SplatPrototypes.Length];
		;
		TerrainEdges = new TerrainChunkEdge[4]
		{
			new TerrainChunkEdge(GridCoords, GridCoords + new Vector2Int(0,-1), WorldSeed, Resolution), // S
			new TerrainChunkEdge(GridCoords, GridCoords + new Vector2Int(1,0), WorldSeed, Resolution), // E
			new TerrainChunkEdge(GridCoords, GridCoords + new Vector2Int(0,1), WorldSeed, Resolution), // N
			new TerrainChunkEdge(GridCoords, GridCoords + new Vector2Int(-1,0), WorldSeed, Resolution) // W
		};

		/**
		Assets.Utils.Debug.Log(string.Format("Took {0} ms to prepare arrays and edges at {1}", stopWatch.ElapsedMilliseconds, GridCoords), LOGLEVEL.META);
		stopWatch.Reset();
		stopWatch.Start();
		**/
		Settings.GetHeightMapGenerator(GridCoords * (Resolution - 1)).ManipulateHeight(ref Heights, Resolution, Settings.Size);

		/**
		Assets.Utils.Debug.Log(string.Format("Took {0} ms to create Heightmap at {1}", stopWatch.ElapsedMilliseconds, GridCoords), LOGLEVEL.META);

		stopWatch.Reset();
		stopWatch.Start();
		**/
		Settings.Moisture.GetHeightSource(GridCoords * (Resolution - 1), WorldSeed, 99).ManipulateHeight(ref Moisture, Resolution, Settings.Size);

		/**
		Assets.Utils.Debug.Log(string.Format("Took {0} ms to create  Moisture at {1}", stopWatch.ElapsedMilliseconds, GridCoords), LOGLEVEL.META);

		stopWatch.Reset();
		stopWatch.Start();
		**/
		NormalsFromHeightMap.GenerateNormals(Heights, Normals, Settings.Depth, (float)Settings.Size / (float)Resolution);

		/**
		Assets.Utils.Debug.Log(string.Format("Took {0} ms to create Normals at {1}", stopWatch.ElapsedMilliseconds, GridCoords), LOGLEVEL.META);

		stopWatch.Reset();
		stopWatch.Start();
		**/
		Vector2Int lowerBound = new Vector2Int(0, 0);
		Vector2Int upperBound = new Vector2Int(Resolution - 1, Resolution - 1);
		PathTools.Bounded8Neighbours neighbours = new PathTools.Bounded8Neighbours(ref lowerBound, ref upperBound);
		PathTools.NormalYThresholdWalkable walkable_src = new PathTools.NormalYThresholdWalkable(
			Mathf.Cos(Mathf.Deg2Rad * 35),
			Normals,
			Resolution, ref lowerBound, ref upperBound);
		PathTools.CachedWalkable walkable = new PathTools.CachedWalkable(walkable_src.IsWalkable, lowerBound, upperBound, Resolution);
		PathTools.Octile8GridSlopeStepCost AStarStepCost = new PathTools.Octile8GridSlopeStepCost(5000, 10, Heights);

		/**
		Assets.Utils.Debug.Log(string.Format("Took {0} ms to prepare pathfinding at {1}", stopWatch.ElapsedMilliseconds, GridCoords), LOGLEVEL.META);
		stopWatch.Reset();
		stopWatch.Start();
		**/
		Connectivity = new PathTools.ConnectivityLabel(Resolution, neighbours, walkable.IsWalkable);


		if (Settings.TerrainLOD[LOD].HasStreets)
		{
			/**
			ClusterMap = new int[Resolution, Resolution];
			for (int i = 0; i < ClusterMap.GetLength(0); i++)
			{
				for (int j = 0; j < ClusterMap.GetLength(1); j++)
				{
					ClusterMap[i, j] = -1;
				}
			}
			float ClusterSize = ((float)(Resolution * Resolution)) * .25f;
			int offset = 0;
			int clusters = 0;
			for (int i = 0; i < Connectivity.NumLabels; i++)
			{
				clusters = Mathf.CeilToInt((float)Connectivity.LabelSizes[i] / ClusterSize);
				Assets.Utils.Debug.Log(string.Format("Run Kmeans with {0} clusters for Label of size {1}", clusters, Connectivity.LabelSizes[i]));
				KMeansClustering.Cluster(Heights, Connectivity.Labels, ClusterMap, i, ref clusters, offset, Settings.Depth);
				offset += clusters;
			}
			ClusterCount = offset;

			float[] clusterHeight = new float[ClusterCount];
			Vector3[] clusterNormal = new Vector3[ClusterCount];
			float[] clusterMoisture = new float[ClusterCount];
			int[] clusterSize = new int[ClusterCount];

			for (int y = 0; y < Heights.GetLength(0); y++)
			{
				for (int x = 0; x < Heights.GetLength(1); x++)
				{
					int label = ClusterMap[y, x];
					if (label == -1) continue;
					clusterSize[label]++;
					clusterHeight[label] += Heights[y, x];
					clusterNormal[label] += Normals[y, x];
					clusterMoisture[label] += Moisture[y, x];
				}
			}
			for (int c = 0; c < ClusterCount; c++)
			{
				if (clusterSize[c] <= 1) continue;
				clusterHeight[c] /= clusterSize[c];
				clusterNormal[c] /= clusterSize[c];
				clusterMoisture[c] /= clusterSize[c];
			}
			
			**/
			/**
			Assets.Utils.Debug.Log(string.Format("Took {0} ms to create ConnectivityMap K-Means and ClusterMap at {1}", stopWatch.ElapsedMilliseconds, GridCoords), LOGLEVEL.META);
			
			stopWatch.Reset();
			stopWatch.Start();
			**/

		}
		paths = new PathFinder(AStarStepCost.StepCosts, this, Connectivity);

		if (Settings.TerrainLOD[LOD].HasStreets)
		{
			AStar search = new AStar(walkable.IsWalkable, neighbours, paths.StepCostsRoad, MapTools.OctileDistance, paths.IsGoal, 2f);
			paths.SetSearch(search);
			search.PrepareSearch(Resolution * Resolution);
			paths.CreateNetwork(this, TerrainEdges);
			search.CleanUp();

			/**
			Assets.Utils.Debug.Log(string.Format("Took {0} ms to create route network at {1}", stopWatch.ElapsedMilliseconds, GridCoords), LOGLEVEL.META);
			stopWatch.Reset();
			stopWatch.Start();
			**/
		}
		if (Settings.TerrainLOD[LOD].HasTrees)
		{
			Trees = vGen.GenerateTrees(this, ChunkSeed, paths.StreetMap, Normals);
			NormalsFromHeightMap.GenerateNormals(Heights, Normals, Settings.Depth, (float)Settings.Size / Resolution);
		}
		TerrainLabeler.MapTerrain(this, Moisture, Heights, Normals, SplatmapData, paths.StreetMap, Settings.WaterLevel, Settings.VegetationLevel, gridCoords * Resolution);

		/**
		Assets.Utils.Debug.Log(string.Format("Took {0} ms to create Vegetation and Splatmap at {1}", stopWatch.ElapsedMilliseconds, GridCoords), LOGLEVEL.META);
		stopWatch.Stop();
		**/

		if (Settings.TerrainLOD[LOD].HasJumps)
			JumpList = JumpPointFinder.FindJumps(ref paths.paths, 1, Settings.MinJumpDist, Settings.MaxJumpDist, this);

		if (Settings.TerrainLOD[LOD].hasGras)
			grasField = new GrasField(this, this.GetHashCode());
	}

	public void CheckNeighbors()
	{
		UnityTerrain.GetComponent<Terrain>().SetNeighbors(
			W != null && W.Resolution == this.Resolution && W.UnityTerrain != null ? W.UnityTerrain.GetComponent<Terrain>() : null,
			N != null && N.Resolution == this.Resolution && N.UnityTerrain != null ? N.UnityTerrain.GetComponent<Terrain>() : null,
			E != null && E.Resolution == this.Resolution && E.UnityTerrain != null ? E.UnityTerrain.GetComponent<Terrain>() : null,
			S != null && S.Resolution == this.Resolution && S.UnityTerrain != null ? S.UnityTerrain.GetComponent<Terrain>() : null
		);
	}

	/// <summary>
	/// Translates the index of a grid element on the heightmap from one index to another using a TranslationFactor
	/// example:
	/// TranslateHM(0, 2, 128) = 0;
	/// TranslateHM(2, 2, 128) = 4;
	/// TranslateHM(64, 2, 128) = 128;
	/// </summary>
	/// <param name="coords"></param>
	/// <param name="TranslationFactor"></param>
	/// <param name="t_max"></param>
	/// <returns></returns>
	private static int TranslateHM(int coords, float TranslationFactor, int t_max)
	{
		int coordOut = Mathf.RoundToInt(coords * TranslationFactor);
		coordOut = coordOut < 0 ? 0 : coordOut >= t_max ? t_max - 1 : coordOut;
		return coordOut;
	}

	public static void CopyHeightMapEdge(TerrainChunk from, int fxbase, int fybase, int fwidth, int fheight, TerrainChunk to, int txbase, int tybase)
	{
		var toTerrain = to.UnityTerrain.GetComponent<Terrain>();
		var fromTerrain = from.UnityTerrain.GetComponent<Terrain>();
		var fromData = fromTerrain.terrainData.GetHeights(fxbase, fybase, fwidth, fheight);
		if (from.Resolution == to.Resolution)
		{
			toTerrain.terrainData.SetHeights(txbase, tybase, fromData);
			return;
		}
		float[,] todata = null;
		if (from.Resolution <= to.Resolution)
		{
			int Translation = Mathf.RoundToInt(to.Resolution / (float)from.Resolution);
			float revTrans = 1f / (float)Translation;

			if (fwidth == 1)
			{
				int theight = Mathf.RoundToInt((fheight - 1) * Translation);
				if (theight % 2 == 0) theight += 1;
				todata = new float[theight, 1];

				UnityEngine.Debug.LogFormat("fromData ({0}, {1}), toData ({2}, {3}), TFactor {4}", fromData.GetLength(0), fromData.GetLength(1), todata.GetLength(0), todata.GetLength(1), Translation);
				for (int t = 0; t < theight; t++)
				{
					if (t % 2 == 0)
					{
						todata[t, 0] = fromData[TranslateHM(t, revTrans, fheight), 0];
					}
					else
					{
						int prev = TranslateHM(t - 1, revTrans, fheight);
						int next = TranslateHM(t + 1, revTrans, fheight);
						todata[t, 0] = fromData[prev, 0] * .5f + fromData[next, 0] * .5f;
					}
				}
			}
			else if (fheight == 1)
			{
				int twidth = Mathf.RoundToInt((fwidth - 1) * Translation);
				if (twidth % 2 == 0) twidth += 1;
				todata = new float[1, twidth];
				UnityEngine.Debug.LogFormat("fromData ({0}, {1}), toData ({2}, {3}), TFactor {4}", fromData.GetLength(0), fromData.GetLength(1), todata.GetLength(0), todata.GetLength(1), Translation);
				for (int t = 0; t < twidth; t++)
				{
					if (t % 2 == 0)
					{
						todata[0, t] = fromData[0, TranslateHM(t, revTrans, fwidth)];
					}
					else
					{
						int prev = TranslateHM(t - 1, revTrans, fwidth);
						int next = TranslateHM(t + 1, revTrans, fwidth);
						todata[0, t] = fromData[0, prev] * .5f + fromData[0, next] * .5f;
					}
				}
			}
			toTerrain.terrainData.SetHeights(txbase, tybase, todata);

		}
		else // to.Resolution < from.resolution
		{
			int Translation = Mathf.RoundToInt(from.Resolution / (float)to.Resolution);

			if (fwidth == 1)
			{
				int theight = Mathf.RoundToInt(fheight / (float)Translation);
				if (theight % 2 == 0) theight += 1;
				todata = new float[theight, 1];
				UnityEngine.Debug.LogFormat("fromData ({0}, {1}), toData ({2}, {3}), TFactor {4}", fromData.GetLength(0), fromData.GetLength(1), todata.GetLength(0), todata.GetLength(1), Translation);
				for (int t = 0; t < theight; t++)
				{
					int curr = TranslateHM(t, Translation, fheight);
					todata[t, 0] = fromData[curr, 0];
				}
			}
			else if (fheight == 1)
			{
				int twidth = Mathf.RoundToInt(fwidth / (float)Translation);
				if (twidth % 2 == 0) twidth += 1;
				todata = new float[1, twidth];
				UnityEngine.Debug.LogFormat("fromData ({0}, {1}), toData ({2}, {3}), TFactor {4}", fromData.GetLength(0), fromData.GetLength(1), todata.GetLength(0), todata.GetLength(1), Translation);
				for (int t = 0; t < twidth; t++)
				{
					int curr = TranslateHM(t, Translation, fwidth);
					todata[0, t] = fromData[0, curr];
				}
			}

			toTerrain.terrainData.SetHeights(txbase, tybase, todata);
		}
	}

	public static void MixAlphaMaps(float[,,] target, float[,,] source, float mix = .5f)
	{
		for (int y = 0; y < source.GetLength(0); y++)
		{
			for (int x = 0; x < source.GetLength(1); x++)
			{
				for (int splat = 0; splat < source.GetLength(2); splat++)
				{
					float tsplat = target[y, x, splat];
					float ssplat = source[y, x, splat];
					target[y, x, splat] = mix * ssplat + (1 - mix) * tsplat;
					source[y, x, splat] = mix * tsplat + (1 - mix) * ssplat;
				}
			}
		}
	}

	public static void CopySplatMapEdge(TerrainChunk from, int fxbase, int fybase, int fwidth, int fheight, TerrainChunk to, int txbase, int tybase)
	{
		var toTerrain = to.UnityTerrain.GetComponent<Terrain>();
		var fromTerrain = from.UnityTerrain.GetComponent<Terrain>();
		var fromData = fromTerrain.terrainData.GetAlphamaps(fxbase, fybase, fwidth, fheight);
		float[,,] todata = null;
		if (from.Resolution == to.Resolution)
		{
			todata = toTerrain.terrainData.GetAlphamaps(txbase, tybase, fwidth, fheight);
			MixAlphaMaps(todata, fromData);
			toTerrain.terrainData.SetAlphamaps(txbase, tybase, todata);
			fromTerrain.terrainData.SetAlphamaps(fxbase, fybase, fromData);
			return;
		}
		return;
	}

	public void PushTopRight(TerrainChunk tile)
	{
		var toTerrain = tile.UnityTerrain.GetComponent<Terrain>();
		var fromTerrain = this.UnityTerrain.GetComponent<Terrain>();
		toTerrain.terrainData.SetHeights(0, 0, fromTerrain.terrainData.GetHeights(Resolution - 1, Resolution - 1, 1, 1));
		toTerrain.Flush();
	}

	public void SyncPahts(TerrainChunk tile, int edgeId)
	{
		// mount paths
		Vector2 WorldCoord = new Vector2();
		CircleBound WayVertextest = new CircleBound(WorldCoord, 2f);
		/**
		Vector3 RayStart = new Vector3(0, this.Settings.Depth, 0);
		RaycastHit hit;
		**/
		foreach (Vector2Int RoadPoint in this.GetEdgeWayPoints(edgeId))
		{
			if (!this.paths.Hub.Contains(RoadPoint)) continue;
			var FamiliarWV = this.paths.Hub.Get(RoadPoint);
			this.ToWorldCoordinate(RoadPoint.x, RoadPoint.y, ref WorldCoord);
			WayVertextest.Center = WorldCoord;
			List<QuadTreeData<ObjectData>> wvList = new List<QuadTreeData<ObjectData>>();
			if (tile.Objects.GetCollisions(WayVertextest, QuadDataType.wayvertex, wvList))
			{
				foreach (var wv in wvList)
				{
					UnityEngine.Debug.LogFormat("Synching Vertex at {0} with Vertex at {1}", WorldCoord, wv.location);
					var AlienWV = tile.paths.Hub.vertices[wv.contents.label];
					List<PathWithDirection> fpaths = FamiliarWV.GetPaths();
					List<PathWithDirection> apaths = AlienWV.GetPaths();
					foreach (PathWithDirection p in fpaths)
					{
						AlienWV.Mount(p.path, p.forward, true);
					}
					foreach (PathWithDirection p in apaths)
					{
						FamiliarWV.Mount(p.path, p.forward, true);
					}
					/**
					RayStart.x = wv.location.x;
					RayStart.z = wv.location.y; if (Physics.Raycast(RayStart, -Vector3.up, out hit, Settings.Depth, 1 << 8, QueryTriggerInteraction.Ignore))
					{
						GameObject vertexMarker = GameObject.CreatePrimitive(PrimitiveType.Cube);
						vertexMarker.transform.position = hit.point + Vector3.up * 4;
						vertexMarker.transform.localScale += new Vector3(3, 3, 3);
						int count = AlienWV.GetPaths().Count - AlienWV.FirstForeignPath;
						vertexMarker.transform.name = string.Format("Synched Vertex of Grid {0}(LOD {3}), at {1}global has {2} foreign paths", GridCoords, hit.point, count, LOD);
					}
					**/
				}
				/**
				RayStart.x = WorldCoord.x;
				RayStart.z = WorldCoord.y; if (Physics.Raycast(RayStart, -Vector3.up, out hit, Settings.Depth, 1 << 8, QueryTriggerInteraction.Ignore))
				{
					GameObject vertexMarker = GameObject.CreatePrimitive(PrimitiveType.Cube);
					vertexMarker.transform.position = hit.point + Vector3.up * 4;
					vertexMarker.transform.localScale += new Vector3(3, 3, 3);
					int count = FamiliarWV.GetPaths().Count - FamiliarWV.FirstForeignPath;
					vertexMarker.transform.name = string.Format("Synched Vertex of Grid {0}(LOD {3}), at {1}global has {2} foreign paths", GridCoords, hit.point, count, LOD);
				}
				**/
			}
		}
	}

	public void PushTop(TerrainChunk tile)
	{

		tile.S = this;
		var terrain = tile.UnityTerrain.GetComponent<Terrain>();
		var myterrain = UnityTerrain.GetComponent<Terrain>();
		//terrain.terrainData.SetHeights(0, 0, myterrain.terrainData.GetHeights(0, Resolution - 1, Resolution - 1, 1));
		CopyHeightMapEdge(this, 0, Resolution - 1, Resolution, 1, tile, 0, 0);
		CopySplatMapEdge(this, 0, Resolution - 1, Resolution, 1, tile, 0, 0);
		tile.CheckNeighbors();
		terrain.Flush();
		N = tile;
		SyncPahts(tile, 2);
	}

	public void PushRight(TerrainChunk tile)
	{

		tile.W = this;
		tile.CheckNeighbors();
		var terrain = tile.UnityTerrain.GetComponent<Terrain>();
		var myterrain = UnityTerrain.GetComponent<Terrain>();
		//terrain.terrainData.SetHeights(0, 0, myterrain.terrainData.GetHeights(Resolution - 1, 0, 1, Resolution - 1));
		CopyHeightMapEdge(this, Resolution - 1, 0, 1, Resolution, tile, 0, 0);
		CopySplatMapEdge(this, Resolution - 1, 0, 1, Resolution, tile, 0, 0);
		terrain.Flush();

		SyncPahts(tile, 1);
	}

	public void Synchronize(SurfaceManager SM)
	{
		// Push Master Values
		N = SM.GetTile(GridCoords + new Vector2Int(0, 1));
		E = SM.GetTile(GridCoords + new Vector2Int(1, 0));
		S = SM.GetTile(GridCoords + new Vector2Int(0, -1));
		W = SM.GetTile(GridCoords + new Vector2Int(-1, 0));


		if (N != null && N.isFinished)
		{
			PushTop(N);
		}
		if (E != null && E.isFinished)
		{
			PushRight(E);
		}
		if (S != null && S.isFinished)
		{
			S.PushTop(this);
		}
		if (W != null && W.isFinished)
		{
			W.PushRight(this);
		}

		var NE = SM.GetTile(GridCoords + new Vector2Int(1, 1));
		var SW = SM.GetTile(GridCoords + new Vector2Int(-1, -1));
		if (NE != null && NE.isFinished)
		{
			PushTopRight(NE);
		}
		if (SW != null && SW.isFinished)
		{
			SW.PushTopRight(this);
		}
		CheckNeighbors();

		var myterrain = UnityTerrain.GetComponent<Terrain>();
		myterrain.Flush();
	}

	public void UnsyncPaths(TerrainChunk tile, int edgeId)
	{
		// mount paths
		Vector2 WorldCoord = new Vector2();
		CircleBound WayVertextest = new CircleBound(WorldCoord, 5f);
		foreach (Vector2Int RoadPoint in this.GetEdgeWayPoints(edgeId))
		{
			if (!this.paths.Hub.Contains(RoadPoint)) continue;
			var FamiliarWV = this.paths.Hub.Get(RoadPoint);
			this.ToWorldCoordinate(RoadPoint.x, RoadPoint.y, ref WorldCoord);
			WayVertextest.Center = WorldCoord;
			List<QuadTreeData<ObjectData>> wvList = new List<QuadTreeData<ObjectData>>();
			if (tile.Objects.GetCollisions(WayVertextest, QuadDataType.wayvertex, wvList))
			{
				foreach (var wv in wvList)
				{
					var AlienWV = tile.paths.Hub.vertices[wv.contents.label];
					AlienWV.UnmountAllAliens();
					FamiliarWV.UnmountAllAliens();
				}
			}
		}
	}

	public void Unload()
	{
		if (N != null)
		{
			UnsyncPaths(N, 2);
			N.S = null;
			N.CheckNeighbors();
			this.N = null;
		}

		if (E != null)
		{
			UnsyncPaths(E, 1);
			E.W = null;
			E.CheckNeighbors();
			this.E = null;
		}
		if (S != null)
		{
			S.UnsyncPaths(this, 2);
			S.N = null;
			S.CheckNeighbors();
			this.S = null;
		}
		if (W != null)
		{
			UnsyncPaths(W, 1);
			W.E = null;
			W.CheckNeighbors();
			this.W = null;
		}
		this.UnityTerrain.SetActive(false);
		DestroyJumps();
		DestroyItems();
		GameObject.Destroy(this.UnityTerrain);
	}

	[Conditional("DEBUG")]
	public void ExportDebugImages()
	{
		/**
		Stopwatch stopWatch = new Stopwatch();
		stopWatch.Start();
		**/
		FloatImageExporter HimgExp = new FloatImageExporter(0f, 1f);
		IntImageExporter CimgExp = new IntImageExporter(-1, Connectivity.NumLabels - 1);
		//IntImageExporter KMimgExp = new IntImageExporter(-1, (ClusterCount - 1));
		IntImageExporter SimgExp = new IntImageExporter(-1, paths.paths.Count + 1);
		HimgExp.Export(string.Format("HeightmapAtl({0}, {1})", GridCoords.x, GridCoords.y), Heights);
		HimgExp.Export(string.Format("MoistureAt({0}, {1}))", GridCoords.x, GridCoords.y), Moisture);
		CimgExp.Export(string.Format("ConnectivityMapAt({0}, {1})", GridCoords.x, GridCoords.y), Connectivity.Labels);
		//KMimgExp.Export(string.Format("ClusterMapAt({0}, {1})", GridCoords.x, GridCoords.y), ClusterMap);
		SimgExp.Export(string.Format("StreetMapAt({0}, {1})", GridCoords.x, GridCoords.y), paths.StreetMap);

		/**
		Assets.Utils.Debug.Log(string.Format("Took {0} ms to export debug Images at {1}", stopWatch.ElapsedMilliseconds, GridCoords), LOGLEVEL.META);
		stopWatch.Stop();
		**/
		foreach (var path in paths.paths)
		{
			path.DrawDebugLine(paths.paths.Count);
		}
	}

	private void DestroyJumps()
	{
		if (!Settings.TerrainLOD[LOD].HasJumps) return;
		for (int i = 0; i < JumpList.Count; i++)
		{
			if (JumpList[i].Ramp != null)
			{
				GameObject.Destroy(JumpList[i].Ramp.gameObject);
			}
		}
	}

	private void DestroyItems()
	{
		if (Settings.TerrainLOD[LOD].HasPowerups)
		{
			for (int i = 0; i < Items.Count; i++)
			{
				GameObject.Destroy(Items[i]);
			}

			Items = new List<GameObject>();
		}
	}


	private void GenerateItems(Terrain terrain)
	{
		Items = new List<GameObject>();
		System.Random rnd = new System.Random();
		for (int i = 0; i < paths.paths.Count; i++)
		{
			var points = paths.paths[i].Waypoints;
			var iter = points.First;
			while (iter != null)
			{
				if (rnd.Next(0, 100) <= 10)
				{
					var pos2D = ToWorldCoordinate(iter.Value.x, iter.Value.y);
					var position = new Vector3(pos2D.x, 0.0f, pos2D.y);

					position.y = terrain.SampleHeight(position);

					var o = UnityEngine.Object.Instantiate(Settings.ItemTypes[rnd.Next(0, Settings.ItemTypes.Length)].Appearance);
					o.transform.position = position;

					Items.Add(o);
				}
				iter = iter.Next;
			}
		}
	}

	public IEnumerable<Vector2Int> GetEdgeWayPoints(int edgeid)
	{
		TerrainChunkEdge edge = TerrainEdges[edgeid];
		edge.GenerateRoadPoints();
		int Offset = (Resolution - 1) * edgeid;
		foreach (int roadPoint in edge.RoadPoints)
		{
			Vector2Int p = MapTools.UnfoldToPerimeter(roadPoint + Offset, Resolution - 1);
			int label = Connectivity.Labels[p.y, p.x];
			if (label >= 0)
			{
				yield return p;
			}
		}
	}

	public IEnumerable<Vector2Int> EdgeWayPoints()
	{
		for (int i = 0; i < TerrainEdges.Length; i++)
		{
			foreach (Vector2Int p in this.GetEdgeWayPoints(i))
			{
				yield return p;
			}
		}
	}

	public IEnumerable<int> Flush(SurfaceManager SM)
	{
		if (FlushStep < 1)
		{

			ChunkTerrainData = new TerrainData()
			{
				heightmapResolution = Resolution,
				size = new Vector3(Settings.Size, Settings.Depth, Settings.Size),
				splatPrototypes = GameSettings.SplatPrototypes,
				alphamapResolution = Resolution,
				treePrototypes = GameSettings.TreeProtoTypes,
				treeInstances = Trees.ToArray(),
				baseMapResolution = 512,
			};

			ChunkTerrainData.SetDetailResolution(0, 8);

			ChunkTerrainData.SetHeights(0, 0, Heights);
			ChunkTerrainData.SetAlphamaps(0, 0, SplatmapData);


			Trees.Clear();
			FlushStep = 1;

			yield return 1;
		}
			if (FlushStep < 2)
			{
				//ExportDebugImages();
				if (UnityTerrain != null)
				{
					//GameObject.Destroy(UnityTerrain.gameObject);
					Terrain.Destroy(UnityTerrain);
				}
				UnityTerrain = Terrain.CreateTerrainGameObject(ChunkTerrainData);
				UnityTerrain.layer = 8;
				UnityTerrain.name = string.Format("TerrainChunk at {0} LOD:{1}", GridCoords, LOD);

				GameObject SurfaceManagerObject = GameObject.Find("Surface Manager");
				if (SurfaceManagerObject != null)
					UnityTerrain.transform.SetParent(SurfaceManagerObject.transform);


				Terrain terrain = UnityTerrain.GetComponent<Terrain>();
				terrain.castShadows = true;
				terrain.heightmapPixelError = 6;
				terrain.materialType = Terrain.MaterialType.Custom;
				terrain.materialTemplate = Settings.TerrainMaterial;
				terrain.treeBillboardDistance = Settings.TreeBillBoardDistance;
				terrain.treeDistance = Settings.TreeRenderDistance;
				terrain.detailObjectDistance = Settings.DetailRenderDistance;
				terrain.heightmapPixelError = 12;
				terrain.castShadows = true;
				terrain.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.BlendProbesAndSkybox;

				UnityTerrain.transform.position = TerrainPos;

				UnityTerrain.SetActive(true);
				FlushStep = 2;

				yield return FlushStep;
			}

			if (FlushStep < 3)
			{
				Terrain terrain = UnityTerrain.GetComponent<Terrain>();
				// Generate items

				FlushStep = 3;
				if (Settings.TerrainLOD[LOD].HasPowerups)
				{
					GenerateItems(terrain);


					yield return FlushStep;
				}
			}
			if (FlushStep < 4)
			{

				FlushStep = 4;
				// Flush Gras
				if (Settings.TerrainLOD[LOD].hasGras)
				{
					grasField.Flush();
					yield return FlushStep;
				}
			}

			if (FlushStep < 5)
			{
				FlushedJumps = 0;
				// Cleanup
				Heights = new float[0, 0];
				Moisture = new float[0, 0];
				Normals = new Vector3[0, 0];
				SplatmapData = new float[0, 0, 0];


				// Debug the Placement of WayVertices on Chunk edges.
				/**
				RaycastHit hit;
				Vector3 RayStart = new Vector3(0, Settings.Depth, 0);
				foreach(var vertexEdge in this.paths.Hub.vertices)
				{
					RayStart.x = vertexEdge.WPos.x;
					RayStart.z = vertexEdge.WPos.y;
					if (Physics.Raycast(RayStart, -Vector3.up, out hit, Settings.Depth, 1 << 8, QueryTriggerInteraction.Ignore)) {
						GameObject vertexMarker = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
						vertexMarker.transform.position = hit.point;
						vertexMarker.transform.localScale += new Vector3(2, 3, 2);
						int count = vertexEdge.GetPaths().Count;
						vertexMarker.transform.name = string.Format("Vertex of Grid {0}(LOD {4}), at {1}(local), {2}global has {3} native paths", GridCoords, vertexEdge, hit.point, count, LOD);
					}
				}
				**/
				Synchronize(SM);
				FlushStep = 5;
				yield return FlushStep;
			}
			isFinished = true;
		}

		public PathFinder GetPathFinder()
		{
			return paths;
		}
	}
