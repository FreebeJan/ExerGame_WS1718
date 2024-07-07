using UnityEngine;
using System.Collections.Generic;
using Assets.Utils;

namespace Assets.World.Paths
{
	/**
     * Wrapper around a Dictionary Vector2Int -> WayVertex.
     * Used to Find points to connect paths to.
     **/
	public class VertexHub
	{
		private Dictionary<Vector2Int, int> Storage;
		private TerrainChunk chunk;
		private CircleBound bound;
		public List<WayVertex> vertices;

		public void Cleanup()
		{
			foreach (WayVertex V in vertices)
			{
				V.Cleanup();
			}
		}

		public VertexHub(TerrainChunk chunk, float lookupRadius = 3f)
		{
			this.chunk = chunk;
			this.bound = new CircleBound(new Vector2(), lookupRadius);
			vertices = new List<WayVertex>();
			Storage = new Dictionary<Vector2Int, int>(32);
		}

		public bool Get(Vector2 Wpos, out WayVertex V) {

			bound.Center = Wpos;
			List<QuadTreeData<ObjectData>> dataps = new List<QuadTreeData<ObjectData>>();
			chunk.Objects.GetCollisions(bound, QuadDataType.wayvertex, dataps);
			if (dataps.Count == 0)
			{
				V = null;
				return false;
			}
			else
			{
				dataps.Sort((a, b) => Vector2.Distance(a.location, Wpos).CompareTo(Vector2.Distance(b.location, Wpos)));
				V = vertices[dataps[0].contents.label];
				return true;
			}
		}

		public WayVertex Get(Vector2Int pos)
		{
			if (Storage.ContainsKey(pos)) {
				return vertices[Storage[pos]];
			}
			else {
				Vector2 WPos = new Vector2();
				WayVertex V;
				chunk.ToWorldCoordinate(pos.x, pos.y, ref WPos); 
				if (this.Get(WPos, out V))
				{
					if (V != null)
						return V;
				}
				Storage[pos] = vertices.Count;
				chunk.Objects.Put(
					new QuadTreeData<ObjectData>(
						WPos, QuadDataType.wayvertex,
						new ObjectData() { label = vertices.Count}
					)
				);
				WayVertex WV = new WayVertex(pos, WPos, vertices.Count);
				vertices.Add(WV);
				return WV;
			}
		}

		public bool Contains(Vector2Int pos)
		{
			if (Storage.ContainsKey(pos)) return true;
			Vector2 WPos = new Vector2();
			WayVertex V;
			chunk.ToWorldCoordinate(pos.x, pos.y, ref WPos);
			return Contains(WPos);
		}

		public bool Contains(Vector2 Wpos)
		{
			bound.Center = Wpos;
			return chunk.Objects.Collides(bound, QuadDataType.wayvertex);
		}
	}
}
