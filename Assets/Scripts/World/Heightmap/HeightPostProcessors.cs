using UnityEngine;
using HeightMapInterfaces;
using System.Collections.Generic;

namespace HeightPostProcessors
{
	// runs  several PostProcessor in succession
	public class ComposedPostProcessor : IHeightPostProcessor
	{
		private List<IHeightPostProcessor> Processors;

		public ComposedPostProcessor()
		{
			this.Processors = new List<IHeightPostProcessor>();
		}

		public ComposedPostProcessor(List<IHeightPostProcessor> processors)
		{
			this.Processors = processors;
		}

		public float PostProcess(float noiseValue)
		{
			if (Processors.Count == 0) return 0;
			foreach (IHeightPostProcessor processor in Processors)
				noiseValue = processor.PostProcess(noiseValue);
			return noiseValue;
		}

		public void AddProcessor(IHeightPostProcessor processor)
		{
			this.Processors.Add(processor);
		}
	}

	//Cuts of and rescales to max and min values
	public class HeightRescale : IHeightPostProcessor
	{
		float MinValue, MaxValue;

		public HeightRescale(float MinValue, float MaxValue)
		{
			this.MinValue = MinValue;
			this.MaxValue = MaxValue;
		}

		public float PostProcess(float noiseValue)
		{
			return Mathf.InverseLerp(MinValue, MaxValue, noiseValue);
		}
	}

	// Creates Terraces
	public class TerracingPostProcessor : IHeightPostProcessor
	{
		float TerraceWidth;

		public TerracingPostProcessor(float terraceWidth)
		{
			TerraceWidth = terraceWidth;
		}

		public float PostProcess(float noiseValue)
		{
			float k = Mathf.Floor(noiseValue / TerraceWidth);
			float r = (noiseValue - k * TerraceWidth) / TerraceWidth;
			float s = Mathf.Min(2 * r, 1);
			return (k + s) * TerraceWidth;
		}
	}

	// Creates Terraces and smoothes them by adjusting the slope between terraces.
	// Got the equation from:
	// https://math.stackexchange.com/questions/1671132/equation-for-a-smooth-staircase-function
	public class SmoothTerracingPostProcessor : IHeightPostProcessor
	{
		private readonly float th, s, w;

		public SmoothTerracingPostProcessor(float terracHeight, float smoothness, float terraceWidth)
		{
			w = terraceWidth;
			s = smoothness;
			th = terracHeight;
		}

		private float TanH(float x)
		{
			return (Mathf.Exp(x) - Mathf.Exp(-x)) / (Mathf.Exp(x) + Mathf.Exp(-x));
		}

		public float PostProcess(float x)
		{
			float step = Mathf.Floor(x / w);
			float res = th * (
				TanH(s * (x / w - step - 1 / 2f))
				/ (2f * TanH(s / 2f))
				+ 1 / 2f + step
			);
			return res;
		}
	}

	// Flattens Terrain
	class ExponentialPostProcessor : IHeightPostProcessor
	{
		private float power;

		public ExponentialPostProcessor(float power)
		{
			this.power = power;
		}

		public float PostProcess(float noiseValue)
		{
			return Mathf.Pow(noiseValue, power);
		}
	}

	// Applies Unity AnimationCurve
	class CurvePostProcessor : IHeightPostProcessor
	{
		private AnimationCurve curve;

		public CurvePostProcessor(AnimationCurve curve)
		{
			this.curve = curve;
		}

		public float PostProcess(float noiseValue)
		{
			return Mathf.Clamp(curve.Evaluate(noiseValue), -1f, 1f);
		}
	}
}