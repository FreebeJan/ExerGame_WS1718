﻿using UnityEngine;
using NoiseInterfaces;

public class VegetationGenerator
{
    public int NumberOfLayers = 3;

    public VegetationGenerator(int NumberOfLayers)
    {
        // TODO: read Number of layers from terrain instead!
        this.NumberOfLayers = NumberOfLayers;
    }

    public TerrainData PaintGras(INoise2DProvider noise, TerrainData terrainData)
    {
	for (int l = 0; l < NumberOfLayers; l++)
        {

            int[,] detailMap = new int[terrainData.detailWidth, terrainData.detailHeight];
            for (int y = 0; y < terrainData.detailHeight; y++)
            {
                float y_01 = (float)y / (float)terrainData.detailHeight;
                for (int x = 0; x < terrainData.detailWidth; x++)
                {
                    float x_01 = (float)x / (float)terrainData.detailWidth;
                    detailMap[x, y] = 0; // (int) (noise.Evaluate(new Vector2(x_01, y_01)) + 1f);
                }
            }
            terrainData.SetDetailLayer(0, 0, l, detailMap);
        }
        
    
	int Seed = 42; // Randomly chosen by chance!
	System.Random prng = new System.Random((int) Seed);

	TreePrototype proto = new TreePrototype();
	proto.prefab = GameObject.Find("FirstPersonTree");
	
	TreePrototype[] protos = terrainData.treePrototypes;
	System.Array.Resize<TreePrototype>(ref protos, terrainData.treePrototypes.Length + 1);
	protos[terrainData.treePrototypes.Length] = proto;
	
	terrainData.treePrototypes = protos;
	
	TreeInstance[] trees = new TreeInstance[512];
	for(int i = 0; i < trees.Length; i++)
	{
		trees[i] = new TreeInstance();
		trees[i].prototypeIndex = 0;
		
		int x = prng.Next(0, terrainData.heightmapWidth);
		int y = prng.Next(0, terrainData.heightmapHeight);
		trees[i].position = new Vector3((float) x / terrainData.heightmapWidth, 
					terrainData.GetHeight(x, y) / terrainData.size.y,
					(float) y / terrainData.heightmapHeight);
		trees[i].heightScale = (float) prng.Next(256, 512) / 512.0f;
		trees[i].widthScale = trees[i].heightScale;
		
		trees[i].rotation = (float) prng.Next(0, 360) * Mathf.Deg2Rad;
	}	
	
	terrainData.treeInstances = trees;
	return terrainData;
    }
}
