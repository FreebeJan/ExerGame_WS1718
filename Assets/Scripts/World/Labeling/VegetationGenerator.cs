using UnityEngine;
using System.Collections.Generic;
using Assets.Utils;
using Assets.World;

public class VegetationGenerator
{
	public int TreeTerraceRadius = 4;
	public int TreeTerraceSmoothRadius = 2;

	public VegetationGenerator() { }

	public List<TreeInstance> GenerateTrees(TerrainChunk terrain, long Seed, int[,] streetMap, Vector3[,] Normals)
	{
		GameSettings Settings = terrain.Settings;
		System.Random prng = new System.Random((int)Seed);

		int x_res, y_res;
		x_res = terrain.Heights.GetLength(1);
		y_res = terrain.Heights.GetLength(0);


		int HeightmapResolution = Settings.TerrainLOD[terrain.LOD].HeightmapResolution;
		int streetdist = Settings.TerrainLOD[terrain.LOD].StreetNeighborOffset;
		float elementWidth = Settings.TerrainLOD[terrain.LOD].gridElementWidth;
		float StreetDistance = elementWidth * (streetdist + 1);
		List<TreeInstance> trees = new List<TreeInstance>();
		CircleBound BigCirc = new CircleBound(new Vector2(), StreetDistance);
		Vector2Int LowerLimits = new Vector2Int(0, 0);
		Vector2Int UpperLimits = new Vector2Int(HeightmapResolution - 1, HeightmapResolution - 1);
		MapTools.VariableDistCircle VDistCirc = new MapTools.VariableDistCircle(
			LowerLimits, UpperLimits, 1, TreeTerraceRadius
		);
		MapTools.LevelOnCenter Level = new MapTools.LevelOnCenter(VDistCirc, terrain.Heights);
		MapTools.KernelAppliance TerrainSmoother = new MapTools.KernelAppliance(
			new MapTools.VariableDistCircle(LowerLimits, UpperLimits, 1, TreeTerraceRadius),
			new MapTools.VariableDistCircle(LowerLimits, UpperLimits, 1, TreeTerraceSmoothRadius),
			new OctileDistKernel(),
			terrain.Heights
		);

		float TerraceOffset = TreeTerraceRadius * Settings.TerrainLOD[terrain.LOD].gridElementWidth;
		float offset_perc = TerraceOffset / Settings.Size;

		Vector2 WorldCoords = new Vector2();
		for (int i = Settings.MaxTreeLevel; i >= Settings.TerrainLOD[terrain.LOD].TreeLevel; i--)
		{
			int MaxTreeCount = 0;
			float MinHelth = 0, MinDist = 0, MaxDist = 0;
			Settings.TreeLevelParams(i, ref MaxTreeCount, ref MinHelth, ref MinDist, ref MaxDist);
			float maxStepPercentage = Mathf.Max(MaxDist / (float)Settings.Size, 1f / Mathf.Sqrt(MaxTreeCount));
			float minStepPercentage = MinDist / (float)Settings.Size;
			float halfStep = (maxStepPercentage + minStepPercentage) / 2f;
			float Tries = Mathf.Ceil(MaxTreeCount * maxStepPercentage);
			Tries = Tries < 1 ? 1 : Tries;

			CircleBound TreeCollider = new CircleBound(new Vector2(), MinDist);
			for (float x = halfStep; x <= 1f - halfStep; x += maxStepPercentage)
			{
				for (float y = halfStep; y <= 1f - halfStep; y += maxStepPercentage)
				{
					float k = 0;
					while (k < Tries)
					{
						float tx = x + (float)prng.NextDouble() * (maxStepPercentage + minStepPercentage) - halfStep;
						float ty = y + (float)prng.NextDouble() * (maxStepPercentage + minStepPercentage) - halfStep;

						if (tx - offset_perc - 1e-3 < 0 || tx + offset_perc + 1e-3 > 1f) { k += .5f; continue; };
						if (ty - offset_perc - 1e-3 < 0 || ty + offset_perc + 1e-3 > 1f) { k += .5f; continue; };
						terrain.ToWorldCoordinate(tx, ty, ref WorldCoords);

						// check that tree does not stand next to a street.
						BigCirc.Center = WorldCoords;
						if (terrain.Objects.Collides(BigCirc, QuadDataType.street)) { k += .5f; continue; };
						// Check that tree does not collide with an other tree.
						TreeCollider.Center = WorldCoords;
						if (terrain.Objects.Collides(TreeCollider, QuadDataType.vegetation)) { k += .5f; continue; };
						Vector2Int local = terrain.ToLocalCoordinate(WorldCoords.x, WorldCoords.y);

						Vector3 normal = Normals[local.y, local.x];
						float height = terrain.Heights[local.y, local.x];
						float steepness = 1f - normal.y * normal.y;

						// do not place tree on very steep terrain.
						if (steepness > .5) { k += .5f; continue; };

						// do not place tree where it cannot grow.
						if (height > Settings.VegetationLevel || height < Settings.WaterLevel) { k += .5f; continue; };

						// calculate health.

						float VegSpot = (Settings.VegetationLevel + Settings.WaterLevel) / 2f;
						float VegZone = 1f - Mathf.Abs(height - VegSpot);
						float health = VegZone * (terrain.Moisture[local.y, local.x] + normal.y * normal.y) / 2f;
						if (health < MinHelth) { k += .5f; continue; };

						// Do weighted cointoss to decide whether to place a tree here.
						if (prng.NextDouble() < 1f - Mathf.InverseLerp(MinHelth, 1f, health)) { k += .25f; continue; };

						// Create tree terrace
						Level.Apply(local.y, local.x);
						TerrainSmoother.Apply(local.y, local.x);

						// create tree.
						int protoindex = prng.Next(0, GameSettings.TreeProtoTypes.Length);
						trees.Add(new TreeInstance
						{
							prototypeIndex = protoindex,
							position = new Vector3(tx, 0f, ty),
							heightScale = health,
							widthScale = health,
							rotation = (float)prng.Next(0, 360) * Mathf.Deg2Rad
						});
						// Remember tree location
						bool success = terrain.Objects.Put(
							new QuadTreeData<ObjectData>(WorldCoords, QuadDataType.vegetation, new ObjectData { collection = 0, label = 0 })
						);
						if (!success)
						{
							Assets.Utils.Debug.Log(string.Format("Could not add Tree at {0} to QuadTree.", WorldCoords));
						}
						k += 1f;
					}
				}
			}
		}
		return trees;
	}
}
