using UnityEngine;

namespace HeightMapInterfaces
{
	/// Creates the initial HeightMap or simulates natur forces on heightmap.
	public interface IHeightSource
	{
		void ManipulateHeight(ref float[,] heights, int Resolution, int UnitSize);
		void SetPostProcessor(IHeightPostProcessor processor);
	}

	public interface IScannableHeightSource : IHeightSource
	{
		// Performs the Height generation for a certain position. Vector2 should be scaled according to target resolution.
		float ScanHeight(Vector2 point);
	}

	public interface IHeightPostProcessor
	{
		// Returns processed noiseValue between -1 and 1
		float PostProcess(float noiseValue);
	}
}