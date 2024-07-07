using UnityEngine;
using HeightMapInterfaces;
using HeightPostProcessors;
using System;
using System.Text;

[System.Serializable]
public class HeightMapPostProcessor
{
	public enum PostProcessorType
	{
		Exponent, Terrace, Rescale
	}
	public PostProcessorType type;
	public float value1, value2, value3; // Configuration values for the PostProcessors.

	public IHeightPostProcessor ToIHeightPostProcessor()
	{
		if (type == PostProcessorType.Exponent)
		{
			return new ExponentialPostProcessor(value1);
		}
		if (type == PostProcessorType.Rescale)
		{
			return new HeightRescale(value1, value2);
		}
		return new SmoothTerracingPostProcessor(value1, value2, value3);
	}
}

[System.Serializable]
public class HeightmapSetting
{
	public int Octaves;
	public float Weight, Lacunarity, Persistance, FeatureFrequency;
	public Fractal2DNoise.NoiseBase NoiseType;
	public Fractal2DNoise.FractalNoiseType FractalType = Fractal2DNoise.FractalNoiseType.Normal;
	public IHeightPostProcessor PostProcessor;
	private long Seed;
	public HeightMapPostProcessor[] postprocessors;
	public HeightMapFromNoise.HeightMapType HeightmapType = HeightMapFromNoise.HeightMapType.smooth;

	public IScannableHeightSource GetHeightSource(Vector2Int Offset, long WorldSeed, int index)
	{
		int hash = index;
		unchecked
		{
			hash = hash + 3277 * (int)WorldSeed;
		}
		System.Random prng = new System.Random(hash);
		byte[] buf = new byte[8];
		prng.NextBytes(buf);
		Seed = BitConverter.ToInt64(buf, 0);

		IScannableHeightSource source = new HeightMapFromNoise(
			new Fractal2DNoise(Persistance, Lacunarity, Octaves, Seed, NoiseType, FractalType),
			FeatureFrequency,
			Offset,
			HeightmapType
		);

		if (postprocessors.Length == 1)
		{
			source.SetPostProcessor(postprocessors[0].ToIHeightPostProcessor());

		}
		else if (postprocessors.Length > 1)
		{
			ComposedPostProcessor composer = new ComposedPostProcessor();
			for (int i = 0; i < postprocessors.Length; i++)
			{
				composer.AddProcessor(postprocessors[i].ToIHeightPostProcessor());
			}
			source.SetPostProcessor(composer);
		}
		return source;
	}
}

[System.Serializable]
public class MixedHeightMap
{
	public HeightmapSetting BaseMap;
	public float BaseWeight;
	public HeightmapSetting Mixer;
	public HeightmapSetting Component1;
	public HeightmapSetting Component2;

	public IHeightSource GetHeightMapGenerator(Vector2Int Offset, long WorldSeed)
	{
		return new DynamicallyComposedHeightMap(
			Offset,
			BaseMap.GetHeightSource(Offset, WorldSeed, 0),
			Mixer.GetHeightSource(Offset, WorldSeed, 1),
			Component1.GetHeightSource(Offset, WorldSeed, 2),
			Component2.GetHeightSource(Offset, WorldSeed, 3),
			BaseWeight
		);
	}
}

[System.Serializable]
public class ChunkLOD
{
	public int minDist;
	public int HeightmapResolution;

	public bool hasGras;
	public int MaxGrasCount;

	public bool HasTrees;
	public int TreeLevel;

	public bool HasJumps;
	public bool HasStreets;
	public bool HasPowerups;


	[HideInInspector]
	public float gridElementWidth;

	[HideInInspector]
	public int StreetNeighborOffset;

	[HideInInspector]
	public float CenterStreetNeighborOffset;

	[HideInInspector]
	public Vector3 TileCorrection;

	[HideInInspector]
	public Vector3 TreeCorrection;
}

[System.Serializable]
public class ItemSettings
{
	public UnityEngine.GameObject Appearance;
	public float SomeProperty = 1.0f;
}

[System.Serializable]
public class GameSettings
{
	public GameObject MainObject;
	public GameObject[] AIs;
	public ItemSettings[] ItemTypes;
	public ChunkLOD[] TerrainLOD;

	public int ChunkMapSize = 32;
	public float Depth, WaterLevel, VegetationLevel;
	public int Size;
	public MixedHeightMap Heightmap;
	public HeightmapSetting Moisture;
	public long WorldSeed;
	public Material TerrainMaterial;

	public static TreePrototype[] TreeProtoTypes;
	public static SplatPrototype[] SplatPrototypes;
	public static DetailPrototype[] DetailPrototypes;

	public float TreeBillBoardDistance;
	public float TreeRenderDistance;
	public float DetailRenderDistance;
	public float TreePlantOffset;

	public float MaxVegetaionDensity;

	public int MaxGrasCount;
	public Color HealthyGrasColor;
	public Color DryGrasColor;

	public Material GrasMaterial;

	public float Gravity, MinJumpSpeed, MaxJumpSpeed;
	public float JumpOffsetY, JumpOffsetX, MinJumpDist, MaxJumpDist;
	public float MinGrasDist = .3f;
	public float GrasFieldSize = 32;

	public float StreetRadius = 1f;
	public float LODDDistanceUnit;

	private static string charset = "qwertzuiopü+asdfghjklöä#yxcvbnm,.<1234567890ß!§$%&/()=?`QWERTZUIOPÜASDFGHJKLÖÄ'*>YXCVBNM;:_|²³{[]}^°@€";
	
	public float SplatMixing = .6f;
	public float JumpOffsetDirChange = .5f;

	public int MaxTreeLevel = 2;
	public int MaxTreeCount = 128;
	public float MinTreeHealth = .5f;
	public float MinTreeDist = 1f;
	public float MaxTreeDist = 10f;

	/// <summary>
	/// Calculates for each tree level how many trees and how to place them.
	/// </summary>
	/// <param name="level">input the tree level</param>
	/// <param name="levelCount">output number of trees to generate</param>
	/// <param name="minHealth">output minimum health level to place a tree</param>
	/// <param name="minDist">ouput minimum distance between two trees</param>
	/// <param name="maxDist">output roughly at this distance there should be the next tree</param>
	public void TreeLevelParams(int level, ref int levelCount, ref float minHealth, ref float minDist, ref float maxDist)
	{
		level = level > MaxTreeLevel ? MaxTreeLevel : level < 0 ? 0 : level;
		int levelFactor = 1 << (level + 1);
		levelCount = MaxTreeCount / levelFactor;
		minHealth = MinTreeHealth + (1f -  MinTreeHealth) / (float)levelFactor;
		float levelDist = (MaxTreeDist - MinTreeDist) / (float)(levelFactor << 1);
		minDist = MinTreeDist + levelDist;
		maxDist = MaxTreeDist - levelDist;
	}

	public static long HashString(string input)
	{
		byte[] hash_out = new byte[8];
		using (var sha = new System.Security.Cryptography.SHA256Managed())
		{
			byte[] textData = System.Text.Encoding.UTF8.GetBytes(input);
			byte[] hash = sha.ComputeHash(textData);
			int i = 0;
			foreach (byte chunk in hash)
			{
				hash_out[i] ^= chunk;
				i = (i + 1) % 8;
			}
			return (long)BitConverter.ToUInt64(hash_out, 0);
		}
	}
	public static string RandomString(int length)
	{
		StringBuilder builder = new StringBuilder();
		for (int i = 0; i < length; i++)
		{

			System.Random prng = new System.Random();
			builder.Append(charset[prng.Next(0, charset.Length)]);
		}
		return builder.ToString();
	}

	public void Prepare()
	{
		string WorldSeedString = PlayerPrefs.GetString("GameSeed", "");
		if (WorldSeedString.Length == 0)
		{
			WorldSeedString = RandomString(64);
			PlayerPrefs.SetString("GameSeed", WorldSeedString);
		}
		WorldSeed = HashString(WorldSeedString);
		// Adjust World to Player Settings.
		string terrainType = PlayerPrefs.GetString("TerrainType", "Mix");
		switch (terrainType)
		{
			case ("Valley"):
				Heightmap.BaseWeight = .8f;
				break;
			case ("Mountains"):
				Heightmap.BaseWeight = .3f;
				break;
			default:
				Heightmap.BaseWeight = .55f;
				break;
		}
		string spikiness = PlayerPrefs.GetString("Spikiness", "Normal");
		switch (spikiness)
		{
			case ("Rigid"):
				Heightmap.Component1.Persistance = .45f;
				Heightmap.Component2.Persistance = .45f;
				break;
			case ("Very Rigid"):
				Heightmap.Component1.Persistance = .5f;
				Heightmap.Component2.Persistance = .5f;
				break;
			case ("Extreme"):
				Heightmap.Component1.Persistance = .55f;
				Heightmap.Component2.Persistance = .55f;
				break;
			case ("Smooth"):
				Heightmap.Component1.Persistance = .35f;
				Heightmap.Component2.Persistance = .35f;
				break;
			default:
				Heightmap.Component1.Persistance = .45f;
				Heightmap.Component2.Persistance = .45f;
				break;
		}
		// Calculate some units that are influenced by settings.
		for (int i = 0; i < TerrainLOD.Length; i++)
		{
			int HeightmapResolution = TerrainLOD[i].HeightmapResolution;
			TerrainLOD[i].gridElementWidth = (float)Size / (float)HeightmapResolution;
			TerrainLOD[i].StreetNeighborOffset = Mathf.CeilToInt(StreetRadius / TerrainLOD[i].gridElementWidth);
			TerrainLOD[i].CenterStreetNeighborOffset = TerrainLOD[i].gridElementWidth * TerrainLOD[i].StreetNeighborOffset;

			float correctionalOffset = .5f / (float)HeightmapResolution; // Put coordinates in the middle of a tile.
			TerrainLOD[i].TileCorrection = new Vector3(correctionalOffset * (float)Size, 0, correctionalOffset * (float)Size);
			TerrainLOD[i].TreeCorrection = new Vector3(correctionalOffset, -TreePlantOffset, correctionalOffset);
		}
		LODDDistanceUnit = (float)Math.Sqrt(1.5f * Size * 1.5f * Size * 2f);
	}

	public IHeightSource GetHeightMapGenerator(Vector2Int Offset)
	{
		return Heightmap.GetHeightMapGenerator(Offset, WorldSeed);
	}

	public int RetrieveLOD(Vector2 GridCoords, Vector3 PlayerPos)
	{
		Vector2 FlatCoord = Size * GridCoords;
		float Dist = (new Vector2(PlayerPos.x - FlatCoord.x, PlayerPos.z - FlatCoord.y)).magnitude;
		int units = Mathf.RoundToInt(Dist / LODDDistanceUnit);
		for (int i = 0; i < TerrainLOD.Length; i++)
		{
			if (TerrainLOD[i].minDist > units)
			{
				int id = i == 0 ? 0 : i - 1;
				return id;
			}
		}
		return TerrainLOD.Length - 1;
	}
}
