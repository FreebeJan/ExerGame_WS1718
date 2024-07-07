using System.IO;
using UnityEngine;

namespace Assets.Utils
{
	class FloatImageExporter
	{
		float Min, Max;
		public FloatImageExporter(float min, float max)
		{
			Min = min;
			Max = max;
		}

		public FloatImageExporter(float[,] data)
		{
			Min = float.MaxValue;
			Max = float.MinValue;
			for (int x = 0; x < data.GetLength(0); x++)
			{
				for (int y = 0; y < data.GetLength(1); y++)
				{
					if (data[x, y] < Min)
					{
						Min = data[x, y];
					}
					if (data[x, y] > Max)
					{
						Max = data[x, y];
					}
				}
			}
		}

		public void Export(string FileName, float[,] data)
		{
			Texture2D bitmap = new Texture2D(data.GetLength(0), data.GetLength(1), TextureFormat.RGBA32, false);
			for (int x = 0; x < data.GetLength(0); x++)
			{
				for (int y = 0; y < data.GetLength(1); y++)
				{
					float v = Mathf.InverseLerp(Min, Max, data[x, y]);
					bitmap.SetPixel(x, y, new Color(v, v, v));
				}
			}
			bitmap.Apply();
			byte[] bytes = bitmap.EncodeToPNG();

			DirectoryInfo target = Directory.GetParent(Application.dataPath).CreateSubdirectory("debug");
			File.WriteAllBytes(target.FullName + "/" + FileName + ".png", bytes);
		}
	}

	class IntImageExporter
	{
		int Min, Max;
		public IntImageExporter(int min, int max)
		{
			Min = min;
			Max = max;
		}

		public IntImageExporter(int[,] data)
		{
			Min = int.MaxValue;
			Max = int.MinValue;
			for (int x = 0; x < data.GetLength(0); x++)
			{
				for (int y = 0; y < data.GetLength(1); y++)
				{
					if (data[x, y] < Min)
					{
						Min = data[x, y];
					}
					if (data[x, y] > Max)
					{
						Max = data[x, y];
					}
				}
			}
		}

		public void Export(string FileName, int[,] data)
		{
			Texture2D bitmap = new Texture2D(data.GetLength(0), data.GetLength(1), TextureFormat.RGBA32, false);
			for (int x = 0; x < data.GetLength(0); x++)
			{
				for (int y = 0; y < data.GetLength(1); y++)
				{
					float v = Mathf.InverseLerp(Min, Max, data[x, y]);
					bitmap.SetPixel(x, y, new Color(v, v, v));
				}
			}
			bitmap.Apply();
			byte[] bytes = bitmap.EncodeToPNG();

			DirectoryInfo target = Directory.GetParent(Application.dataPath).CreateSubdirectory("debug");
			File.WriteAllBytes(target.FullName + "/" + FileName + ".png", bytes);
		}
	}
}
