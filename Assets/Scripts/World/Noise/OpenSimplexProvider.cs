using UnityEngine;
using NoiseInterfaces;
using NoiseTest;

public class OpenSimplexProvider : INoise2DProvider, INoise3DProvider
{
	OpenSimplexNoise noise;

	public OpenSimplexProvider(long seed)
	{
		noise = new OpenSimplexNoise(seed);
	}

	public float Evaluate(Vector2 point)
	{
		return (float)noise.Evaluate(point.x, point.y);
	}

	public float Evaluate(Vector3 point)
	{
		return (float)noise.Evaluate(point.x, point.y, point.z);
	}
}

