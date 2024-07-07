using UnityEngine;
using NoiseInterfaces;

public class PerlinNoiseProvider : INoise2DProvider
{
	public float Evaluate(Vector2 point)
	{
		return 2f * Mathf.PerlinNoise(point.x, point.y) - 1f;
	}
}
