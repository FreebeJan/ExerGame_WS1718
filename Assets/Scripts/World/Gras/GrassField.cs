using UnityEngine;
using Assets.Utils;
using System.Collections.Generic;

public class GrasField
{
	private GameObject GrasFieldObject;
	private TerrainChunk Terrain;
	private List<Vector3> positions;
	public List<Vector2> DetailLayer;
	private List<Color> colors;

	public GrasField(TerrainChunk Terrain, int seed)
	{
		this.Terrain = Terrain;

		float[,] heights = Terrain.Heights;
		float[,] moisture = Terrain.Moisture;
		Vector3[,] TerrainNormals = Terrain.Normals;

		int Resolution = Terrain.Heights.GetLength(0);
		int grasCount = Terrain.Settings.MaxGrasCount;

		positions = new List<Vector3>(grasCount);
		DetailLayer = new List<Vector2>(grasCount);
		colors = new List<Color>(grasCount);

		Color HealtyColor = Terrain.Settings.HealthyGrasColor;
		Color DryColor = Terrain.Settings.DryGrasColor;
		float WaterLevel = Terrain.Settings.WaterLevel;
		float MaxVegLevel = Terrain.Settings.VegetationLevel;

		System.Random prng = new System.Random(seed);

		int HeightmapResolution = Terrain.Settings.TerrainLOD[Terrain.LOD].HeightmapResolution;
		float gridElementWidth = Terrain.Settings.TerrainLOD[Terrain.LOD].gridElementWidth;
		float CenterStreetNeighborOffset;
		if (Terrain.LOD == 0)
			CenterStreetNeighborOffset = Terrain.Settings.TerrainLOD[Terrain.LOD].CenterStreetNeighborOffset - .5f * gridElementWidth;
		else
			CenterStreetNeighborOffset = Terrain.Settings.TerrainLOD[Terrain.LOD].CenterStreetNeighborOffset;

		CircleBound SmallCircle = new CircleBound(
			new Vector2(),
			CenterStreetNeighborOffset
		);
		CircleBound LargeCircle = new CircleBound(new Vector2(), 3f);
		Vector2 flatCoord = new Vector2();
		int i = 0;
		int n = 0;
		float ToTerrainOffset = (float)1f / HeightmapResolution;
		while (i < grasCount && n < grasCount * 5)
		{
			n++;
			float fx = (float)prng.NextDouble();
			float fy = (float)prng.NextDouble();
			int x = Mathf.RoundToInt(fx * (Resolution - 1));
			int z = Mathf.RoundToInt(fy * (Resolution - 1));
			x = x < 0 ? 0 : (x > (Resolution - 1) ? (Resolution - 1) : x);
			z = z < 0 ? 0 : (z > (Resolution - 1) ? (Resolution - 1) : z);

			Terrain.ToWorldCoordinate(fx, fy, ref flatCoord);
			SmallCircle.Center = flatCoord;
			if (Terrain.Objects.Collides(SmallCircle)) continue;
			float height = heights[z, x];
			if (height < WaterLevel || height > MaxVegLevel) continue;
			if (TerrainNormals[z, x].y < .85) continue;
			if (moisture[z, x] < .3f) continue;
			float health = (Mathf.InverseLerp(.3f, 1f, moisture[z, x]) + Mathf.InverseLerp(0.85f, 1f, TerrainNormals[z, x].y)) / 2f;
			LargeCircle.Center = flatCoord;
			if (Terrain.Objects.Collides(LargeCircle, QuadDataType.vegetation)) health *= .8f;
			if (health < .3f) continue;
			Color healthColor = Color.Lerp(DryColor, HealtyColor, health * health);
			positions.Add(new Vector3(
				flatCoord.x,
				height * Terrain.Settings.Depth +1,
				flatCoord.y)
			);
			DetailLayer.Add(new Vector2(Mathf.Round((float)prng.NextDouble()), .5f * (health + (float)prng.NextDouble())));
			colors.Add(healthColor);
			Vector3 GrasNormal = new Vector3(0, 1f, 0);
			i++;
		}

	}

	public void Flush()
	{
		Terrain UnityTerrain = Terrain.UnityTerrain.GetComponent<Terrain>();
		TerrainData UnityTerrainData = UnityTerrain.terrainData;
		GrasFieldObject = new GameObject(
				"GrasField"
			);
		GrasFieldObject.transform.SetParent(Terrain.UnityTerrain.transform);
		GrasFieldObject.AddComponent<MeshFilter>();
		GrasFieldObject.AddComponent<MeshRenderer>();
		MeshFilter Filter = GrasFieldObject.GetComponent<MeshFilter>();
		int[] indices = new int[positions.Count];
		List<Vector3> normals = new List<Vector3>(positions.Count);

		RaycastHit hit;
		for (int i = 0; i < positions.Count; i++)
		{
			indices[i] = i;
			Vector3 raystart = positions[i];
			raystart.y = Terrain.Settings.Depth + 1;
			if (Physics.Raycast(raystart, -Vector3.up, out hit, Terrain.Settings.Depth + 1f, 1 << 8))
			{
				positions[i] = hit.point;
				normals.Add((hit.normal + Vector3.up).normalized);
			}
			else
			{
				normals.Add(Vector3.up);
			}
		}

		Mesh mesh = new Mesh();
		mesh.SetVertices(positions);
		mesh.SetUVs(2, DetailLayer);
		mesh.SetColors(colors);
		mesh.SetNormals(normals);
		mesh.SetIndices(indices, MeshTopology.Points, 0);
		Filter.mesh = mesh;
		MeshRenderer renderer = GrasFieldObject.GetComponent<MeshRenderer>();
		renderer.material = Terrain.Settings.GrasMaterial;
	}
}
