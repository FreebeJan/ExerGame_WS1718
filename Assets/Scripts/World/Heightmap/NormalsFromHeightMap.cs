using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.World.Heightmap
{
	static class NormalsFromHeightMap
	{
		public static void GenerateNormals(float[,] heightmap, Vector3[,] NormalMap, float Depth, float StepSize)
		{
			float dx, dy;
			int ysize = heightmap.GetLength(0);
			int xsize = heightmap.GetLength(1);
			for (int y = 0; y < ysize; y++)
			{
				for (int x = 0; x < xsize; x++)
				{
					dx = heightmap[y, x < xsize - 1 ? x + 1 : x] - heightmap[y, x > 0 ? x - 1 : x];
					if (x == 0 || x == xsize - 1)
					{
						dx *= 2;
					}
					dy = heightmap[y < ysize - 1 ? y + 1 : y, x] - heightmap[y > 0 ? y - 1 : y, x];
					if (y == 0 || y == xsize - 1)
					{
						dy *= 2;
					}
					NormalMap[y, x].Set(-dx * Depth, 2 * StepSize, dy * Depth);
					NormalMap[y, x].Normalize();
				}
			}
		}

		public static void InterpolateNormal(float x, float y, Vector3 VecOut, Vector3[,] NormalMap)
		{
			float x1, x2, y1, y2;
			float d1, d2, t;
			x1 = Mathf.FloorToInt(x);
			x2 = Mathf.CeilToInt(x);
			y1 = Mathf.FloorToInt(y);
			y2 = Mathf.CeilToInt(y);

			if ((x == x1 && y == y1) || (x == x2 && y == y2)) VecOut = NormalMap[(int)y, (int)x];

			d1 = Mathf.Sqrt((x - x1) * (x - x1) + (y - y1) * (y - y1));
			d2 = Mathf.Sqrt((x - x2) * (x - x2) + (y - y2) * (y - y2));

			t = d1 / (d1 + d2);

			VecOut = NormalMap[(int)y1, (int)x1] * t + (1 - t) * NormalMap[(int)y2, (int)x2];
		}
	}
}
