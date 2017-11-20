﻿using UnityEngine;
using NoiseInterfaces;
using HeightMapInterfaces;
using HeightPostProcessors;

public class SurfaceCreator : MonoBehaviour {

    [Range(0f, 1f)]
    public float Weight;

    [Range(2, 1024)]
    public int Resolution = 513;

    [Range(2, 1024)]
    public int Size = 256;

    [Range(1, 2048)]
    public long Seed = 42;

    [Range(1, 16)]
    public int Octaves = 5;
    
    [Range(1f, 12f)]
    public float Lacunarity = 2;

    [Range(0f, 1f)]
    public float Persistance = .5f;

    [Range(1f, 32f)]
    public float FeatureFrequency = 1f;

    public bool Enable = true;

    public Fractal2DNoise.NoiseBase NoiseType = Fractal2DNoise.NoiseBase.OpenSimplex;

    [Range(0f, 1f)]
    public float Weight2;

    [Range(1, 2048)]
    public long Seed2 = 42;

    [Range(1, 16)]
    public int Octaves2 = 5;

    [Range(1f, 12f)]
    public float Lacunarity2 = 2;

    [Range(0f, 1f)]
    public float Persistance2 = .5f;

    [Range(0f, 32f)]
    public float FeatureFrequency2 = 1f;


    public Fractal2DNoise.NoiseBase NoiseType2 = Fractal2DNoise.NoiseBase.OpenSimplex;

    public bool Enable2 = true;

    [Range(1f, 1024)]
    public float Depth = 240;

    [Range(0f, 1f)]
    public float WaterLevel = .3f;

    [Range(0f, 1f)]
    public float VegeationMaxLevel = .8f;

    public float exponent = 2f;
    public bool EnableExperimentalPaths = false;

    public Texture2D GrasTexture;
    public Texture2D SnowTexture;
    public Texture2D RockTexture;
    public Texture2D SandTexture;
    public Texture2D PathTexture;
    public Texture2D CliffTexture;
    public Texture2D GrasNormalMap;
    public Texture2D SnowNormalMap;
    public Texture2D RockNormalMap;
    public Texture2D SandNormalMap;
    public Texture2D PathNormalMap;
    public Texture2D CliffNormalMap;

    public DetailPrototype Gras;
    public DetailPrototype Bush;
    public DetailPrototype Rock;

    private VegetationGenerator vGen;
    private PathFinder paths;
    private Terrain terrain;
    private ComposedHeightMap HeightMapCreator;

    void OnEnable()
    {
        Refresh();   
    }

    public void Refresh()
    {
        // Prepare HeightMap Generation: TODO: Create ChunkManager. Make ChunkManager manage Heightmap Settings.
        INoise2DProvider noise = new Fractal2DNoise(Persistance, Lacunarity, Octaves, Seed, NoiseType);
        ComposedPostProcessor postProcessor = new ComposedPostProcessor();
        postProcessor.AddProcessor(new ExponentialPostProcessor(exponent));
        postProcessor.AddProcessor(new HeightRescale(0f, Mathf.Pow(.8f, exponent)));
        postProcessor.AddProcessor(new SmoothTerracingPostProcessor(.15f, 4f, .15f));
        IScannableHeightSource heigthCreator = new HeightMapFromNoise(noise, FeatureFrequency, new Vector2Int(0,0));
        heigthCreator.SetPostProcessor(postProcessor);

        INoise2DProvider noise2 = new Fractal2DNoise(Persistance2, Lacunarity2, Octaves2, Seed2, NoiseType2);
        IScannableHeightSource heigthCreator2 = new HeightMapFromNoise(noise2, FeatureFrequency2, new Vector2Int(0, 0));
        heigthCreator2.SetPostProcessor(new HeightRescale(.2f, .9f));

        HeightMapCreator = new ComposedHeightMap();
        HeightMapCreator.AddSource(heigthCreator, Weight);
        HeightMapCreator.AddSource(heigthCreator2, Weight2);

        vGen = new VegetationGenerator(0);
        terrain = GetComponent<Terrain>();
        terrain.terrainData = CreateTerrainData();

        terrain.terrainData.splatPrototypes = new SplatPrototype[] {
            new SplatPrototype
            {
                texture = GrasTexture,
                normalMap = GrasNormalMap
            },
            new SplatPrototype
            {
                texture = SnowTexture,
                normalMap = SnowNormalMap,
            },
            new SplatPrototype
            {
                texture = RockTexture,
                normalMap = RockNormalMap
            },
            new SplatPrototype
            {
                texture = SandTexture,
                normalMap = SandNormalMap
            },
            new SplatPrototype
            {
                texture = PathTexture,
                normalMap = PathNormalMap
            },
            new SplatPrototype
            {
                texture = CliffTexture,
                normalMap = CliffNormalMap
            }
        };

        terrain.terrainData.RefreshPrototypes();
        paths = new PathFinder(terrain.terrainData, Depth);
        if (EnableExperimentalPaths)
        {
            paths.MakePath(new Vector2Int(0, 0), new Vector2Int(512, 512), 0, 512);
            paths.MakePath(new Vector2Int(0, 0), new Vector2Int(512, 256), 0, 512);
            paths.MakePath(new Vector2Int(0, 0), new Vector2Int(256, 512), 0, 512);
        }

        terrain.terrainData.SetHeights(0, 0, paths.Heights);
        terrain.terrainData = TerrainLabeler.MapTerrain(noise, terrain.terrainData, paths.StreetMap, WaterLevel, VegeationMaxLevel);
        terrain.terrainData = vGen.PaintGras(noise2, terrain.terrainData);
        terrain.Flush();
    }

    private TerrainData CreateTerrainData()
    {
        if (terrain.terrainData == null)
            terrain.terrainData = new TerrainData();
        terrain.terrainData.heightmapResolution = Resolution;
        terrain.terrainData.size = new Vector3(Size, Depth, Size);
        float[,] heights = new float[Resolution, Resolution];
        heights = HeightMapCreator.ManipulateHeight(
            heights,
            Resolution, 
            Size
        );
        terrain.terrainData.SetHeights(0, 0, heights);
        return terrain.terrainData;
    }
}