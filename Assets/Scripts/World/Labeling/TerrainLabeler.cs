﻿// Inspired by https://alastaira.wordpress.com/2013/11/14/procedural-terrain-splatmapping/

using UnityEngine;
using System.Linq; // used for Sum of array
using Assets.Utils;

public static class TerrainLabeler
{

	public static void MapTerrain(TerrainChunk terrain, float[,] moisture, float[,] Heights, Vector3[,] Normals, float[,,] SplatMap, int[,] streetMap, float WaterLevel, float VegetationMaxHeight, Vector2 TerrainOffset)
	{
		// Splatmap data is stored internally as a 3d array of floats, so declare a new empty array ready for your custom splatmap data:
		CircleBound streetCollider = new CircleBound(new Vector2(), terrain.Settings.StreetRadius);
		CircleBound treeCollider = new CircleBound(new Vector2(), 1.5f);
		int N = SplatMap.GetLength(2) - 1;
		int c = Mathf.FloorToInt(Mathf.Pow(N, 1f / 3f));
		Vector3[] TextureCoords = new Vector3[N];
		for (int i=0; i<N; i++) {
			TextureCoords[i].Set(
				(i % 3) + .5f,
				((i / 3) % 3) + .5f,
				(i / 9) + .5f
			);
		}
		float triggerdist = .9f;
		float terrainsmoothing = terrain.Settings.SplatMixing;
		float part = 1f / (SplatMap.GetLength(0) * SplatMap.GetLength(1));
		/**
		float meanSteepNess = 0;
		float meanHeight = 0;
		float minS = float.PositiveInfinity;
		float minH = float.PositiveInfinity;
		float maxS = float.NegativeInfinity;
		float maxH = float.NegativeInfinity;
		**/
		for (int y = 0; y < SplatMap.GetLength(0); y++)
		{

			// Normalise x/y coordinates to range 0-1 
			float y_01 = (float)y / ((float)SplatMap.GetLength(0) - 1);
			float fy_hm = y_01 * (Heights.GetLength(0) - 1);
			int y_hm = Mathf.CeilToInt(fy_hm);
			for (int x = 0; x < SplatMap.GetLength(1); x++)
			{
				float x_01 = (float)x / ((float)SplatMap.GetLength(1) - 1);
				float fx_hm = x_01 * (Heights.GetLength(1) - 1);
				int x_hm = Mathf.CeilToInt(fx_hm);


				// Setup an array to record the mix of texture weights at this point
				float[] splatWeights = new float[SplatMap.GetLength(2)];

				streetCollider.Center = terrain.ToWorldCoordinate(x_01, y_01);

				treeCollider.Center = streetCollider.Center;

				if (terrain.Objects.Collides(streetCollider, QuadDataType.street)) // street
				{
					splatWeights[N] = 1f;
				}

				else
				{
					Vector3 normal = Normals[y_hm, x_hm];
					float height = Heights[y_hm, x_hm];
					float moist = moisture[y_hm, x_hm];
					float steepness = (1f - (normal.y * normal.y));
					height = Mathf.InverseLerp(.1f, .9f, height);
					//steepness = Mathf.InverseLerp(0f, .9f, steepness);
					// Debugging
					/**
					meanHeight += height * part;
					meanSteepNess += steepness * part;
					minH = minH <= height ? minH : height;
					maxH = maxH >= height ? maxH : height;
					minS = minS <= steepness ? minS : steepness;
					maxS = maxS >= steepness ? maxS : steepness;
					**/

					float dh, dm, ds;
					dh = height * c;
					dh = dh >= c ? c - 1 : dh;
					dm = moist * c;
					dm = dm >= c ? c - 1 : dm;
					ds = steepness * c;
					ds = ds == c ? c - 1 : ds;
					Vector3 dataPoint = new Vector3(dh, dm, ds);
					for (int i=0; i<N; i++) {
						float d = Vector3.Distance(dataPoint, TextureCoords[i]);
						if (d + 1e-2 > triggerdist) continue;
						splatWeights[i] = (triggerdist - d) / triggerdist;
						splatWeights[i] *= splatWeights[i]; // value high weights over lower values
					}
				}
				// Sum of all textures weights must add to 1, so calculate normalization factor from sum of weights
				float z = splatWeights.Sum();
				// Loop through each terrain texture
				for (int i = 0; i < SplatMap.GetLength(2); i++)
				{

					// Normalize so that sum of all texture weights = 1
					splatWeights[i] /= z;

					// Assign this point to the splatmap array
					SplatMap[y, x, i] = splatWeights[i];
				}
			}
		}
		//UnityEngine.Debug.LogFormat("MeanHeight {0}, MeanSteepness {1}, minH {2}, maxH{3}, minS {4}, maxS {5}", meanHeight, meanSteepNess, minH, maxH, minS, maxS);
	}
}
