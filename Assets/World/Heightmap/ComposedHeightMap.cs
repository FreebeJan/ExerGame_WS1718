﻿using HeightMapInterfaces;
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

    public ComposedHeightMap(List<IScannableHeightSource> sources, List<float> weights)
    {
        Sources = sources;
        Weights = weights;
    }

    public void AddSource(ref IScannableHeightSource source, float weight)
    {
        this.Sources.Add(source);
        this.Weights.Add(weight);
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
                float val = 0;
                for (int i = 0; i < Sources.Count; i++)
                {
                    val += Weights[i] * Sources[i].ScanHeight(Iterator);
                }
                if (Postprocessor == null)
                    heights[x, y] = val;
                else
                    heights[x, y] = Postprocessor.PostProcess(val);

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