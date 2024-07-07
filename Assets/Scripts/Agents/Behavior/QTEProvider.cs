using Assets.World.Paths;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Agents.Behavior
{
	class QTEProvider : ITargetProvider
	{
		private PathWithDirection CurrentPath;
		// Used for QTE
		// ChoiceEnd : Choices of possibles roads at the end of our road
		// ChoiceStart : Choices of possibles roads at the beginning of our road (if we're going backward)
		public List<PathWithDirection> ChoicesEnd;
		public int QTEChoice = 0;
		public bool QTENeedsChoice = true;
		public bool QTEAtStart;

		public QTESys QTE_Sys;
		private int Node;

		public QTEProvider(PathWithDirection dpath, QTESys qte_sys)
		{
			if (!dpath.forward)
			{ // if !forward we reverse the path to make it simplier
				CurrentPath = dpath.reversed();
			    ChoicesEnd = CurrentPath.path.End.GetPaths(CurrentPath);

			} else {
				CurrentPath = dpath;
			}
			QTENeedsChoice = true;
			QTE_Sys = qte_sys;
		}

		public Vector3 GetNextTarget()
		{
			Node = RetrieveNext(Node);
			return CurrentPath.path.WorldWaypoints[Node];
		}

		/** Function 
		Name  : GetNextPath
		Semantics : Compute the next player's path and return the first node's number
		Return type : int : the first node's num
		Parameter : int : current node's number to know if our current path is reversed or not
		*/
		private int GetNextPath(int node)
		{
			PathWithDirection dpath;

			Vector3 pointOfComparison;

			/*
			Debug.Log("Current path");
			Debug.Log(string.Format("Departure point of this path : {0}", path.WorldWaypoints[0]));
			Debug.Log(string.Format("Arrival point of this path : {0}", path.WorldWaypoints[path.WorldWaypoints.Length - 1]));   
			*/

			// we are at the end of the path

			pointOfComparison = CurrentPath.path.WorldWaypoints[CurrentPath.path.WorldWaypoints.Length - 1];


			// Filtering of the choices to keep only relevant choices
			ChoicesEnd = ChoicesFiltering(ChoicesEnd, pointOfComparison);
			if (ChoicesEnd.Count == 1)
			{
				dpath = ChoicesEnd[0];
			}
			else
			{
				//Debug.Log(string.Format("QTE CHOICE : {0}", QTE_Sys.getReturn()));
				dpath = ChoicesEnd[QTE_Sys.getReturn()];
				QTE_Sys.stop();
			}

			//Debug.Log(string.Format("Found Path of lenght {0}", dpath.path.WorldWaypoints.Length));


			// Registration/Replacement of the new path 
			CurrentPath = dpath;
			Debug.Log(string.Format("Start : {0}", CurrentPath.path.Start.Pos)); // /!\ Start.Pos and End.Pos are in cluster's coordinate
			Debug.Log(string.Format("End : {0}", CurrentPath.path.End.Pos));

			// Creation of the new set of possibles choices at the beginning and the end of the new path

			if (!CurrentPath.forward)
			{
				CurrentPath.reverse();
			}
			ChoicesEnd = CurrentPath.path.End.GetPaths(CurrentPath);
			QTENeedsChoice = true;
			return 1;

		}

		/** Function
			Name : ChoicesFiltering
			Semantics : Filter paths choices according to a point of comparison : only paths connected to the point are keeped
						and the previous path is deleted if it's contained in choices && if there're at least one other option
			Parameters : List<PathWithDirection> : list of all the choices of path we have
					   : Vector3 : point of comparison for our filter
			Return : List<PathWithDirection> : list of all the choices after filtering
			Post-condition : all PathWithDirection returned must be connected to the pt of comparison (with an error margin)
		 */
		private List<PathWithDirection> ChoicesFiltering(List<PathWithDirection> choices, Vector3 pointOfComparison)
		{
			/* TRAITEMENT Of ChoiceEnd & ChoicesStart */
			// Suppression of paths that doesn't begin or end at the pointOfComparison (with an error margin)

			//Debug.Log(string.Format("Point de comparaison : {0}", pointOfComparison));

			Debug.Log(string.Format("num of path BEFORE filtering : {0} ", choices.Count));
            if (choices.Count == 1){
                return new List<PathWithDirection>(choices);
            }
			List<PathWithDirection> newChoices = new List<PathWithDirection>(); // List of our new choices
			int currentPathIndex = -1;

			for (int i = 0; i < choices.Count; i++)
			{
				/*Debug.Log(string.Format("Chemin n°{0}", i));
				Debug.Log(string.Format("Point de départ de ce chemin : {0}", choices[i].path.WorldWaypoints[0]));
				Debug.Log(string.Format("Point d'arrivée de ce chemin : {0}", choices[i].path.WorldWaypoints[choices[i].path.WorldWaypoints.Length - 1]));*/


				if (MapTools.Aprox(pointOfComparison, choices[i].path.WorldWaypoints[0], 2f)
					|| MapTools.Aprox(pointOfComparison, choices[i].path.WorldWaypoints[choices[i].path.WorldWaypoints.Length - 1], 2f))
				{

					// the choice is added to our new choices
					newChoices.Add(choices[i]);
					if (choices[i].path.Equals(CurrentPath.path))
					{
						//Debug.Log("CURRENT PATH FOUND");
						currentPathIndex = newChoices.Count-1;
					}
				}
			}
			// If there's only one solution we keep the previous path (turn around), else we remove it
			if (newChoices.Count > 1 && currentPathIndex != -1)
			{
				newChoices.RemoveAt(currentPathIndex);
			}

			//Debug.Log(string.Format("{0} choices found", newChoices.Count));
			return newChoices;
		}

		/** Function 
			Name : RetrieveNext
			Semantics : Compute the next node number to know where to move the player
			Parameters : int : current number of node in our path
			Return type : int : the next node number to which the player is heading
		 */
		private int RetrieveNext(int node, bool reverse = false)
		{

			// We'll always go from pathPoints[0] to pathPoints[end]
			// The path will be reversed when the player change the forward direction (pressing "space")
			// !reverse -> pathPoints[0] -> pathPoints[end] + UP = pathPoints[0] -> pathPoints[end]

			/*Debug.Log(string.Format("NODE : {0}", node));
			Debug.Log(string.Format("isFinished : {0}", QTE_Sys.isFinished()));
			Debug.Log(string.Format("QTENeedsChoice : {0}", QTENeedsChoice));*/
			// if we are almost at the end of the path --> QTE

			if (node < CurrentPath.path.WorldWaypoints.Length - 22 && !QTE_Sys.isFinished())
			{
				QTE_Sys.stop();
			}

			if (node >= CurrentPath.path.WorldWaypoints.Length - 22 && QTE_Sys.isFinished() && QTENeedsChoice)
			{
				// Debug.Log("node >= path.WorldWaypoints.Length - 22 && QTE_Sys.isFinished()");
				//Debug.Log(string.Format("isFinished : {0}", QTE_Sys.isFinished()));

				ChoicesEnd = CurrentPath.path.End.GetPaths(CurrentPath);
				Vector3 pointOfComparison = CurrentPath.path.WorldWaypoints[CurrentPath.path.WorldWaypoints.Length - 1];
				// Filtering of the choices to keep only relevant choices
				ChoicesEnd = ChoicesFiltering(ChoicesEnd, pointOfComparison);
				if (ChoicesEnd.Count != 1)
				{
					QTE_Sys.QTE_Initialisation(ChoicesEnd.Count - 1, ChoicesEnd, CurrentPath, pointOfComparison);
				}
				QTENeedsChoice = false;

			}
			// if we are at the end of the path
			if (node >= CurrentPath.path.WorldWaypoints.Length - 1)
			{

				//Debug.Log("End of the road");
				// we find another path 
				// & we return the number of the next node
				return GetNextPath(node);
			}
			// else we just return the next node number
			return node + 1;
		}

		public Vector3 GetCurrentPos()
		{
			return CurrentPath.path.WorldWaypoints[Node];
		}

		public void TurnAround()
		{
			CurrentPath.reverse();
			Node = CurrentPath.path.WorldWaypoints.Length - Node;
			ChoicesEnd = CurrentPath.path.End.GetPaths(CurrentPath);
			QTENeedsChoice = true;
		}
	}
}
