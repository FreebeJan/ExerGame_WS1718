using UnityEngine;
using HeightMapInterfaces;
using NoiseInterfaces;

public class HeightMapFromNoise : IScannableHeightSource
{
	public enum HeightMapType
	{
		smooth,
		ridged
	}
	private float FeatureFrequency { get; set; }
	public Vector2Int TerrainOffset;
	public INoise2DProvider Noise;
	private IHeightPostProcessor PostProcessor;
	Vector2 Iterator;
	private HeightMapType Type;


	public HeightMapFromNoise(INoise2DProvider noise, float featureFrequency, Vector2Int TerrainOffset, HeightMapType type = HeightMapType.smooth)
	{
		this.Noise = noise;
		this.FeatureFrequency = featureFrequency;
		this.TerrainOffset = TerrainOffset;
		Iterator = new Vector2();
		Type = type;
	}

	public void ManipulateHeight(ref float[,] heights, int Resolution, int UnitSize)
	{
		float stepSize = FeatureFrequency / (Resolution - 1);
		float val;
		float x_pos, y_pos;
		y_pos = TerrainOffset.y * stepSize;
		if (Type == HeightMapType.ridged)
		{
			for (int y = 0; y < Resolution; y++)
			{
				x_pos = TerrainOffset.x * stepSize;
				for (int x = 0; x < Resolution; x++)
				{
					Iterator.y = y_pos;
					Iterator.x = x_pos;
					val = 1f - Mathf.Abs(Noise.Evaluate(Iterator));
					if (this.PostProcessor == null)
						heights[y, x] = val;
					else
						heights[y, x] = PostProcessor.PostProcess(val);
					x_pos += stepSize;
				}
				y_pos += stepSize;
			}
		}
		else
		{
			for (int y = 0; y < Resolution; y++)
			{
				x_pos = TerrainOffset.x * stepSize;
				for (int x = 0; x < Resolution; x++)
				{
					Iterator.y = y_pos;
					Iterator.x = x_pos;
					val = 0.5f + Noise.Evaluate(Iterator) / 2f;
					if (this.PostProcessor == null)
						heights[y, x] = val;
					else
						heights[y, x] = PostProcessor.PostProcess(val);
					x_pos += stepSize;
				}
				y_pos += stepSize;
			}
		}
	}

	public float ScanHeight(Vector2 point)
	{
		float val = 0.5f + Noise.Evaluate(point * FeatureFrequency) / 2f;
		if (this.PostProcessor == null)
			return val;
		else
			return PostProcessor.PostProcess(val);
	}

	public void SetPostProcessor(IHeightPostProcessor processor)
	{
		this.PostProcessor = processor;
	}
}
