using Assets.World.Paths;
using UnityEngine;

namespace Assets.Scripts.Agents.Behavior
{
	class SimpleAIPathTracing : ITargetProvider
	{
		private bool Forward; // Direction given by the path.
		private NavigationPath Path;
		private int Node;

		public SimpleAIPathTracing(PathWithDirection dpath)
		{
			Path = dpath.path;
			Forward = dpath.forward;
			if (!Forward)
			{
				Node = Path.WorldWaypoints.Length - 1;
			}
			else
			{
				Node = 0;
			}
		}

		public Vector3 GetCurrentPos()
		{
			return Path.WorldWaypoints[Node];
		}

		public Vector3 GetNextTarget()
		{
			Node = RetrieveNext(Node);
			return Path.WorldWaypoints[Node];
		}

		public void TurnAround()
		{
			Debug.Log("AI cannot turn around,\nEVER!\n\n:P");
			//TODO: implement :D
		}

		/** Function 
        Name  : GetNextPath
        Semantics : Compute the next player's path and return the first node's number
        Return type : int : the first node's num
        Parameter : int : current node's number to know if our current path is reversed or not
		*/
		private int GetNextPath(WayVertex vertex, int node)
		{
			PathWithDirection dpath = vertex.GetLongest(new PathWithDirection(Path, Forward));
			if (dpath.path.WorldWaypoints != null)
			{
				//Debug.Log(string.Format("Found Path of lenght {0}", dpath.path.WorldWaypoints.Length));
				Path = dpath.path;
				Forward = dpath.forward;
				if (!Forward)
				{
					return Path.WorldWaypoints.Length - 1;
				}
				else
				{
					return 0;
				}
			}
			Forward = !Forward;
			return node;
		}

		/// <summary>
		/// Retrieves the next Node to follow. Calls GetNextPath when end of current path is reached.
		/// </summary>
		/// <param name="node">the id of the current node</param>
		/// <param name="reverse">whether to follow the path in reverse order.</param>
		/// <returns></returns>
		private int RetrieveNext(int node, bool reverse = false)
		{

			if (Forward ^ reverse)
			{
				if (node >= Path.WorldWaypoints.Length - 2)
				{
					return GetNextPath(Path.End, node);
				}
				return node + 1;
			}
			else
			{
				if (node <= 1)
				{
					return GetNextPath(Path.Start, node);
				}
				return node - 1;
			}
		}
	}
}
