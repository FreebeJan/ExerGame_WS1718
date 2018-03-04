﻿using System;
using System.Collections.Generic;
using Assets.Utils;
using System.Linq;
using System.Text;
using UnityEngine;
using Assets.World.Jumps;

namespace Assets.World.Paths
{
	/// <summary>
	/// class to find points suitable for jumping.
	/// </summary>
	static class JumpPointFinder
	{
		private static float jump_y(float vy, float t, float gravity = 9.81f)
		{
			return -.5f * gravity * t * t + vy * t;
		}

		public static List<Vector3> JumpPoints(Vector3 start, Vector3 dir, float vy, float vx, int steps, float t_max, float gravity = 9.81f)
		{
			List<Vector3> points = new List<Vector3>(steps);
			for (float t = 0f; t <= t_max; t += (t_max / (float)steps))
			{
				points.Add(start + dir * t + new Vector3(0, jump_y(vy, t, gravity), 0));
			}
			return points;
		}

		public static bool getPerfectSpeed(Vector3 start, Vector3 end, float gravity, ref float v, ref float t)
		{
			Vector3 dir = new Vector3(end.x - start.x, 0, end.z - start.z);
			float tar_y = end.y - start.y;
			float tar_x = dir.magnitude;

			float a = -gravity * tar_x * tar_x / (2f * (tar_y - tar_x));
			if (a > 0)
			{
				v = (float)Math.Sqrt(a);
				t = tar_x / v;
				return true;
			}
			return false;

		}

		/// <summary>
		/// Returns -1 if jump is too short, 0 if jump hits exactly the end and 1 if jump is too long.
		/// </summary>
		/// <param name="start"></param>
		/// <param name="end"></param>
		/// <param name="speed"></param>
		/// <param name="rayTarget"></param>
		/// <param name="landingPoint"></param>
		/// <param name="gravity"></param>
		/// <returns></returns>
		public static int CheckPhysics(Vector3 start, Vector3 end, float speed, ref Vector3 rayTarget, ref Vector3 landingPoint, float gravity = 9.81f)
		{
			// direction of the jump (without y)
			Vector3 dir = new Vector3(end.x - start.x, 0, end.z - start.z);
			float dy = end.y - start.y;
			float x = dir.magnitude;
			dir.Normalize();
			float vx = (float)Math.Cos(Math.PI * 0.25d) * speed;
			float vy = (float)Math.Sin(Math.PI * 0.25d) * speed;
			UnityEngine.Debug.LogFormat("{0}, {1}", vx, vy);
			if (vx == 0) return -1;

			float t = x / vx;
			float y = jump_y(vy, t, gravity);

			float peak_x = vy * vx / (gravity);
			float peak_t = peak_x / vx;
			float peak_y = jump_y(vy, peak_t, gravity);

			UnityEngine.Debug.LogFormat("peakx {0}, dir {1}, peak_y {2}", peak_x, dir, peak_y);
			rayTarget = start + peak_x * dir;
			rayTarget.y += peak_y;

			float intersection_x = 2 * vx * vx / gravity * (vy / vx - dy / x);
			float intersection_t = intersection_x / vx;
			float intersection_y = jump_y(vy, intersection_t, gravity);
			landingPoint = start + intersection_x * dir;
			landingPoint.y += intersection_y;

			UnityEngine.Debug.LogFormat("{0}, {1} {2}", start, rayTarget, end);
			return y > dy ? 1 : (y == dy ? 0 : -1);
		}


		public static List<Vector2> RampOffsets(Vector2 pos, Vector2 dir, ref TerrainChunk chunk) 
		{
			float sr = chunk.Settings.CenterStreetNeighborOffset;
			float ro = chunk.Settings.JumpOffsetX;
			List<Vector2> rampOffsets = new List<Vector2>();
			Vector2 offsetDir = dir;
			if (!chunk.Objects.Collides(new CircleBound(pos + (sr + ro) * offsetDir, sr))) rampOffsets.Add(offsetDir);
			offsetDir = new Vector2(dir.y, -dir.x);
			if (!chunk.Objects.Collides(new CircleBound(pos + (sr + ro) * offsetDir, sr))) rampOffsets.Add(offsetDir);
			offsetDir = new Vector2(-dir.y, dir.x);
			if (!chunk.Objects.Collides(new CircleBound(pos + (sr + ro) * offsetDir, sr))) rampOffsets.Add(offsetDir);
			return rampOffsets;
		}

		public static bool CheckPoint(ref Vector3 pos, ref Vector3 dir, ref Vector2 relpos, ref Vector2 reldir, float minDist, float maxDist, float minSpeed, float maxSpeed,
									  float gravity, ref TerrainChunk chunk, ref List<JumpData> JumpList)
		{
			float StreetRadius = chunk.Settings.StreetRadius;
			// start a raycast with minDist distance.
			QuadTreeData<ObjectData> collision = chunk.Objects.Raycast(relpos + reldir * minDist, reldir, maxDist - minDist, QuadDataType.street, 1e-1f);
			if (collision != null)
			{

				Vector3 JumpStart = pos + Vector3.up * chunk.Settings.JumpOffsetY;
				NavigationPath colpath = chunk.GetPathFinder().paths[collision.contents.collection];
				Vector3 colPos = colpath.WorldWaypoints[collision.contents.label];

				int next_label = collision.contents.label + 1;
				if (collision.contents.label == colpath.WorldWaypoints.Length - 1) { next_label -= 2; }
				Vector2 colnode = new Vector2(colPos.x, colPos.z);
				Vector2 colNext = new Vector2(colpath.WorldWaypoints[next_label].x, colpath.WorldWaypoints[next_label].z);
				Vector3 colDir = (colNext - colnode).normalized;
				if (Vector3.Distance(colPos, pos) < minDist) return false;

				float jumpdirAngle = Vector2.Angle(reldir, colDir);
				if (jumpdirAngle > 45 && jumpdirAngle < 135) return false;
				
				float v = 0;
				float t = 0;
				if (getPerfectSpeed(JumpStart, colPos, gravity, ref v, ref t))
				{
					v /= (float)Math.Cos(Math.PI * .25f);
					if (v < minSpeed || v > maxSpeed) return false;
					Vector3 rayExactTarget = new Vector3();
					Vector3 ExactLandingPoint = new Vector3();
					int rd = CheckPhysics(pos, colPos, v, ref rayExactTarget, ref ExactLandingPoint, gravity);
					JumpList.Add(
						new JumpData()
						{
							PerfectSpeed = v,
							PerfectTime = t,
							LandingPos = ExactLandingPoint,
							RayTarget = rayExactTarget,
							Pos = pos,
							Dir = (colPos - pos).normalized,
						}
					);
					chunk.Objects.Put(new QuadTreeData<ObjectData>(
						pos + dir, QuadDataType.jump,
						new ObjectData() { label = JumpList.Count - 1 }
						)
					);
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// FindJumps iterates over all pahts of a certain terrainchunk and 
		/// </summary>
		/// <param name="paths"></param>
		/// <param name="objects"></param>
		/// <param name="stepSize"></param>
		/// <param name="minDist"></param>
		/// <param name="maxDist"></param>
		/// <param name="chunk"></param>
		public static List<JumpData> FindJumps(ref List<NavigationPath> paths, int stepSize, float minDist, float maxDist, TerrainChunk chunk)
		{
			float gravity = chunk.Settings.Gravity;
			float minSpeed = chunk.Settings.MinJumpSpeed;
			float maxSpeed = chunk.Settings.MaxJumpSpeed;

			float sr = chunk.Settings.StreetRadius;
			float ro = chunk.Settings.JumpOffsetX;

			int pathCountBefore = paths.Count;

			Vector3[] vDir = new Vector3[2];
			Vector2[] vRelDir = new Vector2[2];

			List<JumpData> JumpList = new List<JumpData>();
			for (int i = 0; i < pathCountBefore; i++)
			{
				if (paths[i].Waypoints.Count < stepSize * 2 + 2) continue;
				for (int j = stepSize + 1; j < paths[i].Waypoints.Count - 2; j += stepSize)
				{
					// Point, predecessor and successor in 3d
					Vector3 PrevNode = paths[i].WorldWaypoints[j - 1];
					Vector3 node = paths[i].WorldWaypoints[j];
					Vector3 NextNode = paths[i].WorldWaypoints[j + 1];

					// Point, predecessor and successor in 2d
					Vector2 prev = new Vector2(PrevNode.x, PrevNode.z);
					Vector2 dest = new Vector2(NextNode.x, NextNode.z);
					Vector2 origin = new Vector2(node.x, node.z);

					vDir[0] = (node - PrevNode).normalized; // forward from current node
					vDir[1] = (node - NextNode).normalized; // backwards from next node
															// same in 2d coords.
					vRelDir[0] = (origin - prev).normalized;
					vRelDir[1] = (origin - dest).normalized;

					// skip points that are not turning points.
					float angle = Vector2.Angle(vRelDir[0], vRelDir[1]);
					if (angle <= 30 || angle >= 150) continue;

					for (int d = 0; d < vDir.Length; d++)
					{
						foreach (Vector2 Offset in RampOffsets(origin, vRelDir[d], ref chunk)) 
						{
							Vector2 pos = origin + (sr + ro) * Offset;
							Vector2 dir = (vRelDir[d] + Offset).normalized;
							Vector3 dir3 = new Vector3(dir.x, vDir[d].y, dir.y);
							dir3.Normalize();
							Vector3 pos3 = new Vector3(pos.x, node.y, pos.y);
							CheckPoint(ref pos3, ref dir3, ref pos, ref dir, minDist, maxDist, minSpeed, maxSpeed, gravity, ref chunk, ref JumpList);
						}
					}
				}
			}
			return JumpList;
		}
	}
}
