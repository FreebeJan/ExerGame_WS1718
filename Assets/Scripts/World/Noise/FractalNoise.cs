using UnityEngine;
using NoiseInterfaces;
using System.Collections.Generic;

/* Combines Noise from different providers.
 * Makes sure result lies between -1 and 1 */
public class Fractal2DNoise : INoise2DProvider
{
	public enum NoiseBase
	{
		OpenSimplex,
		Perlin
	}

	public enum FractalNoiseType
	{
		Normal,
		Ridged
	}
	private float Persistance, Lacunarity;
	private int Octaves;
	INoise2DProvider noise;
	private Vector2[] Offset;
	private FractalNoiseType Type;

	public Fractal2DNoise(float Persistance, float Lacunarity, int Octaves, long Seed, NoiseBase noiseType, FractalNoiseType type = FractalNoiseType.Normal)
	{
		System.Random prng = new System.Random((int)Seed);
		this.Octaves = Octaves;
		this.Persistance = Persistance;
		this.Lacunarity = Lacunarity;
		Type = type;
		Offset = new Vector2[Octaves];
		for (int oct = 0; oct < Octaves; oct++)
		{
			Offset[oct] = new Vector2(
					prng.Next(-10000, 10000),
					prng.Next(-10000, 10000)
			);
		}
		if (noiseType == NoiseBase.Perlin)
			noise = new PerlinNoiseProvider();
		else
			noise = new OpenSimplexProvider(Seed);
	}

	// Evaluates over all  given noise providers. Normalizes by given weight values.
	public float Evaluate(Vector2 point)
	{
		if (Type == FractalNoiseType.Normal)
		{
			float layered = noise.Evaluate(point);
			float amplitude = 1f;
			float range = 1f;
			for (int i = 0; i < Octaves; i++)
			{
				amplitude *= Persistance;
				range += amplitude;
				point *= Lacunarity;
				layered += noise.Evaluate(point + Offset[i]) * amplitude;
			}
			return layered / range;
		}
		else
		{
			float layered = 1f - 2f * Mathf.Abs(noise.Evaluate(point));
			float amplitude = 1f;
			float range = 1f;
			float sum = layered;
			for (int i = 0; i < Octaves; i++)
			{
				amplitude *= Persistance;
				range += amplitude;
				point *= Lacunarity;
				layered *= (1f - 2f * Mathf.Abs(noise.Evaluate(point + Offset[i]))) * amplitude * sum;
				sum += layered;
			}
			return sum / range;
		}
	}
}