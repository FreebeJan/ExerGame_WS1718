using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Assets.Utils;

namespace Assets.World.Paths
{
	/** A finite Automaton to add a new path to the already exisiting network.
     * 
     **/
	public static class PathBranchAutomaton
	{
		/// <summary>
		/// Stores the given path up to the node n and connects it to its WayVertices.
		/// Returns the remainder (starting from n) of the path.
		/// </summary>
		/// <param name="V"></param>
		/// <param name="n"></param>
		/// <param name="label"></param>
		/// <param name="path"></param>
		/// <param name="Hub"></param>
		/// <param name="paths"></param>
		/// <returns></returns>
		public static Utils.LinkedList<Vector2Int> EndPathAt(WayVertex V, Utils.LinkedListNode<Vector2Int> n, int label, Utils.LinkedList<Vector2Int> path, ref VertexHub Hub, List<NavigationPath> paths)
		{
			if (path == null) return null;
			Utils.LinkedList<Vector2Int> newPath = null;
			if (n.Next != null)
			{
				newPath = new Utils.LinkedList<Vector2Int>();
				path.SplitAt(ref n, ref newPath, true);
			}
			if (n.Value != V.Pos)
			{

				if (newPath != null && newPath.First != null)
				{
					newPath.AddFirst(V.Pos);
				}
				path.AddLast(V.Pos);
			}
			WayVertex Start;
			Start = Hub.Get(path.First.Value);
			var navpath = new NavigationPath(Start)
			{
				Waypoints = path,
				Label = label,
				End = V
			};
			navpath.Finalize(Hub);
			paths.Add(navpath);
			return newPath;
		}

		public static void BranchPaths(Utils.LinkedList<Vector2Int> path, int[,] StreetMap, ref VertexHub Hub, List<NavigationPath> paths, TerrainChunk terrain)
		{
			CircleBound VertexFinder = new CircleBound(new Vector2(), 1.41f * terrain.Settings.TerrainLOD[terrain.LOD].gridElementWidth);
			Vector2 WorldCoords = new Vector2();
			// check sanity
			int lastLabel, Status;

			// initialisation
			var node = path.First;
			var lastnode = node;
			int myLabel = paths.Count() + 1;
			lastLabel = StreetMap[node.Value.x, node.Value.y];
			Status = lastLabel == 0 ? 0 : 1;

			// Begin traversing automaton
			while (node != null)
			{

				int label = StreetMap[node.Value.x, node.Value.y];

				if (label != lastLabel && label != myLabel && label != 0)
				{
					NavigationPath colPath = paths[label - 1];
					WayVertex colVertex = null;
					var oldPathNeedsSplit = false;
					if (Hub.Contains(node.Value)) colVertex = Hub.Get(node.Value);
					else oldPathNeedsSplit = true;

					if (colVertex != null)
					{
						EndPathAt(colVertex, node, myLabel, path, ref Hub, paths);
					}
					else
					{
						colVertex = Hub.Get(node.Value);
						EndPathAt(colVertex, node, myLabel, path, ref Hub, paths);
					}

					if (oldPathNeedsSplit)
					{
						var branch = new NavigationPath();
						SplitAndRemount(label, node.Value, paths, Hub, StreetMap, ref branch);
					}
					return;
				}
				Mark(node, myLabel, StreetMap);
				lastnode = node;
				node = node.Next;
				lastLabel = label;
			}
			EndPathAt(Hub.Get(lastnode.Value), lastnode, myLabel, path, ref Hub, paths);

		}

		/* Marks Streetmap at node coordinates as visited by this path.
         */
		private static void Mark(Utils.LinkedListNode<Vector2Int> node, int label, int[,] StreetMap)
		{
			StreetMap[node.Value.x, node.Value.y] = label;
		}

		private static void SplitAndRemount(int label, Vector2Int Value, List<NavigationPath> pathStorage, VertexHub Hub, int[,] Streetmap, ref NavigationPath CollisionBranch)
		{
			pathStorage[label - 1].Unmount();
			if (pathStorage[label - 1].Split(Value, ref CollisionBranch, ref Hub))
			{
				pathStorage[label - 1].Finalize(Hub);
				// remember split.
				CollisionBranch.Label = pathStorage.Count() + 1;
				Utils.LinkedListNode<Vector2Int> BranchNode = CollisionBranch.Waypoints.First;
				while (BranchNode != null)
				{
					Mark(BranchNode, CollisionBranch.Label, Streetmap);
					BranchNode = BranchNode.Next;
				}
				pathStorage.Add(CollisionBranch);
				CollisionBranch.Finalize(Hub);
			}
			else
			{
				pathStorage[label - 1].Finalize(Hub);
			}
		}
	}
}
