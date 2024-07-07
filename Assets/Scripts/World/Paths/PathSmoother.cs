using System.Collections.Generic;
using UnityEngine;

namespace Assets.World.Paths
{
	static class PathStraighter
	{
		/** Skips certain nodes to straighten the path.
         * Nodes are skipped if a previous node can reach a node further down the pathe in lookahead steps by following it's previous direction.
         * params:
         * - path: the linked list to straighten.
         * - lookahead (int) optional: the number of nodes to look ahead.
         **/
		public static void Straighten(ref Utils.LinkedList<Vector2Int> path, int lookahead = 10)
		{
			Vector2Int dir = new Vector2Int();
			bool hasDir = false;

			if (path.Count <= 2) return;

			// forward pass.
			Utils.LinkedListNode<Vector2Int> node = path.First.Next;
			while (node != null && node != path.Last)
			{
				if (hasDir)
				{
					Vector2Int Diff;
					Utils.LinkedListNode<Vector2Int> LookAt = node.Next;
					for (int j = 1; j < lookahead; j++)
					{
						if (LookAt == null) break;
						Diff = LookAt.Value - node.Value;
						bool xfits, yfits;
						xfits = yfits = false;
						if (dir.x == 0)
						{
							if (Diff.x != 0) continue;
							xfits = true;
						}
						if (dir.y == 0)
						{
							if (Diff.y != 0) continue;
							yfits = true;
						}
						if (!xfits) xfits = Diff.x % dir.x == 0;
						if (!yfits) yfits = Diff.y % dir.y == 0;
						if (xfits && yfits)
						{
							node.Next = LookAt;
							LookAt.Previous = node;
						}
					}
				}
				node = node.Next;
				dir = node.Value - node.Previous.Value;
				hasDir = true;
			}
			// backwards pass.
			node = path.Last.Previous;
			while (node != null && node != path.First)
			{
				if (hasDir)
				{
					Vector2Int Diff;
					Utils.LinkedListNode<Vector2Int> LookAt = node.Previous;
					for (int j = 1; j < lookahead; j++)
					{
						if (LookAt == null) break;
						Diff = LookAt.Value - node.Value;
						bool xfits, yfits;
						xfits = yfits = false;
						if (dir.x == 0)
						{
							if (Diff.x != 0) continue;
							xfits = true;
						}
						if (dir.y == 0)
						{
							if (Diff.y != 0) continue;
							yfits = true;
						}
						if (!xfits) xfits = Diff.x % dir.x == 0;
						if (!yfits) yfits = Diff.y % dir.y == 0;
						if (xfits && yfits)
						{
							node.Previous = LookAt;
							LookAt.Next = node;
						}
					}
				}
				node = node.Previous;
				dir = node.Value - node.Next.Value;
				hasDir = true;
			}
			path.RevalidateCount();
		}
	}
}
