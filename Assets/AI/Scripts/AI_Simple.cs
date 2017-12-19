﻿using System.Collections;
using System.Collections.Generic;
using Assets.Utils;
using Assets.World.Heightmap;
using Assets.World.Paths;
using UnityEngine;
using UnityEditor;

/*
this class manage the Player's bicycle : gameplay like a train on rails
*/

public class AI_Simple : MonoBehaviour
{

    public SurfaceManager surfaceManager;
    public float speedMultiplier = 1;
    public float maxSpeed = 5;
    public float maxRotation = 5;
    private Vector3 startPosition;
    private Vector3 endPosition;
    private List<Vector3> path = new List<Vector3>();
    private int current_node;
    private TerrainChunk ActiveTerrain;

    // Use this for initialization
    void OnEnable()
    {

        ActiveTerrain = surfaceManager.GetTile(new Vector2Int(2,2));
        GameSettings Settings = surfaceManager.Settings;
        // initial terrainChunk 
        Terrain terrain = ActiveTerrain.UnityTerrain.GetComponent<Terrain>();
        // Initial Position
        List<NavigationPath> listOfPath = ActiveTerrain.GetPathFinder().paths;
        UnityEngine.Debug.Log(string.Format("Path length {0}", listOfPath[0].Waypoints.Count));

        LinkedList<Vector2Int> path2D = listOfPath[0].Waypoints;
        // Le path est dans les coordonnées du TerrainChunk
        // Pour les passer en coordonnées monde -> P_terrain(x, y)*Settings.Size/Settings.HeighMapResolution + Offset_TerrainChunk
        LinkedListNode<Vector2Int> pathNode = path2D.First;
        while (pathNode != null)
        {
            Vector2Int pathPoint = pathNode.Value;
            float x = pathPoint.x * Settings.Size / Settings.HeightmapResolution + ActiveTerrain.GridCoords.x;
            float z = pathPoint.y * Settings.Size / Settings.HeightmapResolution + ActiveTerrain.GridCoords.y;
            float y = terrain.SampleHeight(new Vector3(x, 0, z));

            path.Add(new Vector3(x, y, z));
            pathNode = pathNode.Next;
        }
        startPosition = path[0];
        transform.position = startPosition;
    }
    // Update is called once per frame
    void Update()
    {

        if ((transform.position - path[current_node]).sqrMagnitude > 1)
        {
            // if the position of the player is not at the path point
            // move until it reach it
            Vector3 pos = Vector3.MoveTowards(transform.position, path[current_node], maxSpeed * Time.deltaTime);

            Quaternion rotationQ = Quaternion.LookRotation(path[current_node] - transform.position);
            transform.position = pos;
            transform.rotation = rotationQ;
            transform.rotation *= Quaternion.Euler(0, 90, 0);

        }
        else
        {
            current_node = (current_node + 1) % path.Count;
        }
    }

}
