using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;
using Assets.Utils;

namespace Assets.World.Paths
{

	public struct PathWithDirection
	{
		public NavigationPath path;
		public bool forward;

		public PathWithDirection(NavigationPath path, bool forward)
		{
			this.path = path;
			this.forward = forward;
		}

		public override int GetHashCode()
		{
			return path.GetHashCode();
		}

		public void reverse()
		{
			forward = true;
			NavigationPath newPath = new NavigationPath();
			newPath.WorldWaypoints = new Vector3[this.path.WorldWaypoints.Length];
			newPath.Waypoints = new Utils.LinkedList<Vector2Int>();
			newPath.End = this.path.Start;
			newPath.Start = this.path.End;
			Utils.LinkedListNode<Vector2Int> node = this.path.Waypoints.First;
			for (int i = path.WorldWaypoints.Length - 1; i >= 0; i--)
			{
				newPath.WorldWaypoints[path.WorldWaypoints.Length - 1 - i] = this.path.WorldWaypoints[i];
				newPath.Waypoints.AddLast(node.Value);
				node = node.Next;
			}
			this.path = newPath;

		}

		public PathWithDirection reversed()
		{
			NavigationPath newPath = new NavigationPath();
			newPath.WorldWaypoints = new Vector3[path.WorldWaypoints.Length];
			newPath.Waypoints = new Utils.LinkedList<Vector2Int>();
			newPath.End = path.Start;
			newPath.Start = path.End;
			Utils.LinkedListNode<Vector2Int> node = path.Waypoints.First;
			for (int i = path.WorldWaypoints.Length - 1; i >= 0; i--)
			{
				newPath.WorldWaypoints[path.WorldWaypoints.Length - 1 - i] = path.WorldWaypoints[i];
				newPath.Waypoints.AddLast(node.Value);
				node = node.Next;
			}
			return new PathWithDirection(newPath, true);

		}

	}

	public class WayVertex : IEquatable<WayVertex>
	{
		public Vector2Int Pos;
		public Vector2 WPos;
		private List<PathWithDirection> Paths;
		//private HashSet<PathWithDirection> Paths;
		public int? LocalId;
		public int FirstForeignPath = int.MaxValue;

		public WayVertex(Vector2Int pos, Vector2 wpos, int? id = null)
		{
			Pos = pos;
			WPos = wpos;
			Paths = new List<PathWithDirection>(4);
			LocalId = id;
		}

		public void UnmountAllAliens()
		{
			var NewPaths = new List<PathWithDirection>();
			int i = 0;
			foreach (var path in Paths) 
			{
				if (i == FirstForeignPath) break;
				NewPaths.Add(path);
			}
			Paths = NewPaths;
		}

		public int Find(NavigationPath path)
		{
			int hash = path.GetHashCode();
			for (int i=0; i < Paths.Count; i++)
			{
				if (Paths[i].GetHashCode() == hash)
					return i;
			}
			return -1;
		}

		public bool Contains(NavigationPath path)
		{
			return Find(path) != -1;
		}

		public bool Unmount(NavigationPath path)
		{
			PathWithDirection dpath = new PathWithDirection(path, true);
			int i = Find(path);
			if (i != -1)
			{
				Paths.RemoveAt(i);
				return true;
			}
			string msg = string.Format("Could not remove path {0} -> {1} because it is not in WayVertex {2}, with members:\n", path.Start.Pos, path.End.Pos, this.Pos);
			foreach (var memberpath in GetPaths())
			{
				msg += string.Format("\t * {0} -> {1}\n", memberpath.path.Start.Pos, memberpath.path.End.Pos);
			}
			UnityEngine.Debug.Log(msg);
			return false;
		}

		public bool Mount(NavigationPath path, bool forward = true, bool foreign = false)
		{
			PathWithDirection dpath = new PathWithDirection(path, forward);
			if (Contains(path)) return false;
			Paths.Add(dpath);
			if (foreign) FirstForeignPath = FirstForeignPath > Paths.Count -1 ? Paths.Count - 1: FirstForeignPath;
			return true;
		}

		public bool Connects(int labelA, int labelB)
		{
			bool foundA, foundB;
			foundA = foundB = false;
			foreach (PathWithDirection p in GetPaths())
			{
				if (p.forward)
				{
					foundA = foundA | p.path.Label == labelA;
					foundB = foundB | p.path.Label == labelB;
				}
				if (foundA && foundB) return true;
			}
			return false;
		}

		public List<PathWithDirection> GetPaths()
		{
			return Paths.ToList<PathWithDirection>();
		}

		public List<PathWithDirection> GetPaths(PathWithDirection ExcPath)
		{
			List<PathWithDirection> paths = Paths.ToList<PathWithDirection>();

			if (paths.Count > 1)
			{
				paths.Remove(ExcPath);
                paths.Remove(ExcPath.reversed());
			}
			return paths;
		}


		public PathWithDirection GetLongest()
		{
			PathWithDirection longest = new PathWithDirection();
			int longestCount = 0;
			Assets.Utils.Debug.Log(string.Format("Get Longest Path for WayVertex at {0} with {1} paths in total.", Pos, Paths.Count));

			foreach (PathWithDirection dpath in Paths.AsEnumerable())
			{
				if (Paths.Count == 1) return dpath;
				if (dpath.path.WorldWaypoints.Length > longestCount)
				{
					longest = dpath;
					longestCount = dpath.path.WorldWaypoints.Length;
				}
			}
			return longest;
		}

		public PathWithDirection GetLongest(PathWithDirection ExcPath)
		{
			PathWithDirection longest = new PathWithDirection();
			int longestCount = 0;
			Assets.Utils.Debug.Log(string.Format("Get Longest Path for WayVertex at {0} with {1} paths in total.", Pos, Paths.Count));
			foreach (PathWithDirection dpath in Paths.AsEnumerable())
			{
				if (Paths.Count == 1) return dpath;
				if (dpath.GetHashCode() == ExcPath.GetHashCode()) continue;
				if (dpath.path.WorldWaypoints.Length > longestCount)
				{
					longest = dpath;
					longestCount = dpath.path.WorldWaypoints.Length;
				}
			}
			return longest;
		}

		internal int Count()
		{
			return Paths.Count;
		}

		public bool Equals(WayVertex other)
		{
			return Pos.Equals(other.Pos);
		}

		internal void Cleanup()
		{
			foreach (PathWithDirection p in GetPaths())
			{
				// remove paths that have been added wrongly.
				if (p.path.Waypoints.Count == 0) Unmount(p.path);
				if (p.path.Start != this && p.path.End != this)
				{
					Unmount(p.path);
					p.path.Mount();
				}
				if (p.path.Waypoints.First.Value != this.Pos && p.path.Waypoints.Last.Value != this.Pos)
				{
					Unmount(p.path);
					p.path.Mount();
				}
			}
		}

		public override int GetHashCode()
		{
			return (WPos.x.GetHashCode() + 3989 * WPos.y.GetHashCode());
		}
	}

	public class NavigationPath : IEquatable<NavigationPath>
	{
		public WayVertex Start, End;
		public Utils.LinkedList<Vector2Int> Waypoints;
		public Vector3[] WorldWaypoints; // Waypoints in World Space
		public int Label { get; set; }

		public NavigationPath()
		{
			Start = End = null;
			Waypoints = new Utils.LinkedList<Vector2Int>();
			WorldWaypoints = new Vector3[0];
			Label = -1;
		}

		public NavigationPath(WayVertex Start)
		{
			this.Start = Start;
			End = null;
			Waypoints = new Utils.LinkedList<Vector2Int>();
			WorldWaypoints = new Vector3[0];
			Label = -1;
		}

		public NavigationPath(List<Vector2Int> points, VertexHub hub)
		{
			Start = hub.Get(points[0]);
			End = hub.Get(points[points.Count - 1]);
			Waypoints = new Utils.LinkedList<Vector2Int>(points);
			WorldWaypoints = new Vector3[0];
			Label = -1;
		}

		public bool Mount()
		{
			if (Start == null || End == null)
			{
				Assets.Utils.Debug.Log(String.Format("Cannot mount path with missing start or end point."));
				return false;
			}
			bool success = true;
			success = success && Start.Mount(this);
			success = success && End.Mount(this, false);
			return success;
		}

		public bool Unmount()
		{
			bool success = true;
			if (Start != null) success = success && Start.Unmount(this);
			if (End != null) success = success && End.Unmount(this);
			return success;
		}

		public void Finalize(VertexHub Hub)
		{
			Waypoints.RevalidateCount();
			if (Waypoints.Count == 0 || Waypoints.First == null || Waypoints.Last == null) return;
			if (Start != null && Start.Pos != Waypoints.First.Value)
			{
				Start.Unmount(this);
				Start = null;
			}
			if (Start == null)
			{
				Start = Hub.Get(Waypoints.First.Value);
			}
			if (End != null && End.Pos != Waypoints.Last.Value)
			{
				End.Unmount(this);
				End = null;
			}
			if (End == null)
				End = Hub.Get(Waypoints.Last.Value);
			this.Mount();
		}

		public void TranslateToWorldSpace(TerrainChunk chunk)
		{
			Vector2 FlatWorldCoordinate = new Vector2();
			Utils.LinkedListNode<Vector2Int> node = Waypoints.First;
			Waypoints.RevalidateCount();
			WorldWaypoints = new Vector3[Waypoints.Count];
			int i = 0;
			while (node != null)
			{
				chunk.ToWorldCoordinate(
					node.Value.x,
					node.Value.y,
					ref FlatWorldCoordinate
				);

				WorldWaypoints[i++].Set(
					FlatWorldCoordinate.x,
					chunk.Heights[(int)node.Value.y, (int)node.Value.x] * chunk.Settings.Depth,
					FlatWorldCoordinate.y
				);
				node = node.Next;
			}
		}

		public override int GetHashCode()
		{
			unchecked // overflow is not a problem.
			{
				return Start.GetHashCode() + End.GetHashCode();
			}
		}

		/** Draws a line representing the world coordinates 
         * Not thread safe Debug method.
         * WARNING: Only run this on the main thread.
         * 
         * parameter: 
         *  pathsTotal (int): Number of paths being drawn in total. Used for distincitve color mapping.
         */
		[System.Diagnostics.Conditional("DEBUG")]
		public void DrawDebugLine(int pathsTotal, float duration = 250f, bool zTest = false, float delta = .1f)
		{
#if (DEBUG_SHOW_PATHS)
			Color MyColor = Colors.RainbowColor(Label, pathsTotal + 1);
			Vector3 Offset = new Vector3(
				delta * ((float)(Label) / (float)(pathsTotal + 1)),
				1f,
				delta * ((float)(Label) / (float)(pathsTotal + 1))
			);
			for (int i = 1; i < WorldWaypoints.Length; i++)
			{
				UnityEngine.Debug.DrawLine(
					WorldWaypoints[i - 1] + Offset,
					WorldWaypoints[i] + Offset,
					MyColor,
					duration,
					zTest
				);
			}
#endif
		}

		public bool Equals(NavigationPath other)
		{
			return (Start == other.Start && End == other.End) || (Start == other.End && End == other.Start);
		}

		public bool SplitAt(ref Assets.Utils.LinkedListNode<Vector2Int> At, ref NavigationPath newPath, ref VertexHub Hub)
		{
			int length = Waypoints.Count;
			if (Waypoints.SplitAt(ref At, ref newPath.Waypoints, true))
			{
				// set path boundaries
				End = Hub.Get(Waypoints.Last.Value);
				Start = Hub.Get(Waypoints.First.Value);
				newPath.Start = Hub.Get(newPath.Waypoints.First.Value);
				newPath.End = Hub.Get(newPath.Waypoints.Last.Value);
				return true;
			}
			else return false;
		}

		public bool Split(Vector2Int Value, ref NavigationPath newPath, ref VertexHub Hub)
		{
			if (Waypoints.SplitAt(Value, ref newPath.Waypoints, true))
			{
				// set path boundaries
				End = Hub.Get(Waypoints.Last.Value);
				Start = Hub.Get(Waypoints.First.Value);
				newPath.Start = Hub.Get(newPath.Waypoints.First.Value);
				newPath.End = Hub.Get(newPath.Waypoints.Last.Value);
				return true;
			}
			else return false;
		}
	}
}
