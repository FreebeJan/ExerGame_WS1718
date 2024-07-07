using UnityEngine;

namespace NoiseInterfaces
{
	public interface INoise2DProvider
	{
		// Return Noise value between -1 and 1
		float Evaluate(Vector2 point);
	}

	public interface INoise3DProvider
	{
		// Return Noise value between -1 and 1
		float Evaluate(Vector3 point);
	}
}