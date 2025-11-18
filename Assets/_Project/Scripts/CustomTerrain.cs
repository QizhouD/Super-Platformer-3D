// referenced code from the course procedural terrain generation : https://www.udemy.com/course/procedural-terrain-generation-with-unity
using UnityEditor;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;


[ExecuteInEditMode]
public class CustomTerrain : MonoBehaviour
{
    public bool resetTerrain = true;


    //initial variable declaration / initlization can be changed in the inspector
    public float xScale = 0.1f;
    public float yScale = 0.1f;
    public int xOffset = 0;
    public int yOffset = 0;
    public int octaves = 3;
    public float persistance = 8;
    public float heightScale = 0.09f;
  
    public Terrain terrain;
    public TerrainData terrainData;


    void OnEnable()
    {
        //Debug.Log("Initialising Terrain Data");
        terrain = this.GetComponent<Terrain>();
        terrainData = Terrain.activeTerrain.terrainData;
    }
    //returning hights 
   float[,] GetHeightMap()
    {
        if (!resetTerrain)
        {
            return terrainData.GetHeights(0, 0, terrainData.heightmapResolution,
                                                terrainData.heightmapResolution);
        }
        else
            return new float[terrainData.heightmapResolution,
                             terrainData.heightmapResolution];
            
    }

    // function to reset terrain back initial state i.e flat
     public void ResetTerrain()
    {
        float[,] heightMap;
        heightMap = new float[terrainData.heightmapResolution,terrainData.heightmapResolution];
        for (int x = 0; x < terrainData.heightmapResolution; x++)
        {
            for (int z = 0; z < terrainData.heightmapResolution; z++)
            {
                heightMap[x, z] = 0;
            }
        }
        terrainData.SetHeights(0, 0, heightMap);

    }

    //applying perlin noise ho terrain  
    public void PerlinNoise()
    {
        float[,] heightMap = GetHeightMap();
        for (int y = 0; y< terrainData.heightmapResolution; y++)
        {
            for (int x=0 ; x < terrainData.heightmapResolution; x++)
            {
                heightMap[x,y]+= fBM((x + xOffset)* xScale , (y + yOffset)*yScale, octaves ,persistance)* heightScale;
            }
        }
        terrainData.SetHeights(0,0, heightMap);
    }

   

    //fBM function calculation
    public static float fBM (float x, float y, int oct, float persistance)
   {
    float total = 0;
        float frequency = 1;
        float amplitude = 1;
        float maxValue = 0;
        for (int i = 0; i < oct; i++)
        {
            total += Mathf.PerlinNoise(x * frequency, y * frequency) * amplitude;
            maxValue += amplitude;
            amplitude *= persistance;
            frequency *= 2;
        }

        return total / maxValue;
   }

void Update() 
{
    //PerlinNoise();
}
    
   

}


