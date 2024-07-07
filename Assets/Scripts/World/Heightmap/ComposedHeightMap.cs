using HeightMapInterfaces;
using System.Collections.Generic;
using UnityEngine;

public class ComposedHeightMap : IHeightSource
{
	public List<IScannableHeightSource> Sources;
	public List<float> Weights;

	public Vector2Int TerrainOffset;
	private Vector2 Iterator;
	private IHeightPostProcessor Postprocessor;


	public ComposedHeightMap(Vector2Int TerrainOffset)
	{
		Sources = new List<IScannableHeightSource>();
		Weights = new List<float>();
		this.TerrainOffset = TerrainOffset;
		Iterator = new Vector2();
	}

	public void AddSource(IScannableHeightSource source, float weight)
	{
		this.Sources.Add(source);
		this.Weights.Add(weight);
	}

	virtual protected float CombineValues(Vector2 Iterator)
	{
		float val = 0;
		for (int i = 0; i < Sources.Count; i++)
		{
			val += Weights[i] * Sources[i].ScanHeight(Iterator);
		}
		return val;
	}

	public void ManipulateHeight(ref float[,] heights, int Resolution, int UnitSize)
	{
		float stepSize = 1f / (Resolution - 1);
		float x_pos, y_pos;
		y_pos = TerrainOffset.y * stepSize;
		for (int y = 0; y < Resolution; y++)
		{
			x_pos = TerrainOffset.x * stepSize;
			for (int x = 0; x < Resolution; x++)
			{
				Iterator.x = x_pos;
				Iterator.y = y_pos;
				float val = CombineValues(Iterator);
				if (Postprocessor == null)
					heights[y, x] = val;
				else
					heights[y, x] = Postprocessor.PostProcess(val);
				x_pos += stepSize;
			}
			y_pos += stepSize;
		}
	}

	public void SetPostProcessor(IHeightPostProcessor processor)
	{
		this.Postprocessor = processor;
	}
}

public class DynamicallyComposedHeightMap : IHeightSource
{
	public IScannableHeightSource HeightBase;
	public IScannableHeightSource Mix;
	public IScannableHeightSource MixerComp1;
	public IScannableHeightSource MixerComp2;

	public Vector2Int TerrainOffset;
	private Vector2 Iterator;
	private IHeightPostProcessor Postprocessor;
	private float BaseWeight;


	public DynamicallyComposedHeightMap(Vector2Int TerrainOffset, IScannableHeightSource HBase, IScannableHeightSource Mix, IScannableHeightSource Comp1, IScannableHeightSource Comp2, float baseWeight)
	{
		this.TerrainOffset = TerrainOffset;
		HeightBase = HBase;
		this.Mix = Mix;
		MixerComp1 = Comp1;
		MixerComp2 = Comp2;
		Iterator = new Vector2();
		BaseWeight = baseWeight;
	}

	virtual protected float CombineValues(Vector2 Iterator)
	{
		float val = HeightBase.ScanHeight(Iterator);
		float mixVal = Mix.ScanHeight(Iterator);
		float mixed = mixVal * MixerComp1.ScanHeight(Iterator) + (1f - mixVal) * MixerComp2.ScanHeight(Iterator);
		return val * BaseWeight + mixed * (1f - BaseWeight);
	}

	public void ManipulateHeight(ref float[,] heights, int Resolution, int UnitSize)
	{
		float stepSize = 1f / (Resolution - 1);
		float x_pos, y_pos;
		y_pos = TerrainOffset.y * stepSize;
		for (int y = 0; y < Resolution; y++)
		{
			x_pos = TerrainOffset.x * stepSize;
			for (int x = 0; x < Resolution; x++)
			{
				Iterator.x = x_pos;
				Iterator.y = y_pos;
				float val = CombineValues(Iterator);
				if (Postprocessor == null)
					heights[y, x] = val;
				else
					heights[y, x] = Postprocessor.PostProcess(val);

				x_pos += stepSize;
			}
			y_pos += stepSize;
		}
	}

	public void SetPostProcessor(IHeightPostProcessor processor)
	{
		this.Postprocessor = processor;
	}
}