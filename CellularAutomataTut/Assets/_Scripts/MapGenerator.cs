using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using CRandom = System.Random;
using Random = UnityEngine.Random;

public class MapGenerator : MonoBehaviour
{
    //Map Dimensions
    public int width;
    public int height;

    //Random seed generated each time
    public string seed;
    public bool useRandomSeed;
    
    [Range(0, 100)]
    public int randomFillPercent;
    
    public int[,] map;

    public int smoothingIterations = 0;

    public float marchingSquareSize = 1f;
    /*Ep2 "Marching Squares" Note: Corners are "control nodes", intermediary points (between ctrl nodes) are
    Simply "Nodes". These "Control Nodes" own the nodes directly above and to the right*/
    public void GenerateMap()
    {
        map = new int[width, height];
        RandomFillMap();
        
        for (int i = 0; i < smoothingIterations; i++)
            SmoothMap();
        int borderSize = 5;
        int[,] borderedMap = new int[width + borderSize * 2, height + borderSize * 2];
        
        MeshGenerator meshGen = GetComponent<MeshGenerator>();
        meshGen.GenerateMesh(map, marchingSquareSize);
    }

    void RandomFillMap()
    {
        if (useRandomSeed)
        {
            seed = Random.value.ToString();
        }
        
        //Pseudo-random number generator
        CRandom pRand = new CRandom(seed.GetHashCode()); //gethashcode returns random unique int for seed
        for (int x = 0; x < width; x++){
            for (int y = 0; y < height; y++)
            {
                //tiles around border of map are always walls (no holes in edge of map)
                if (x == 0 || x == width - 1 || y == 0 || y == height - 1) map[x, y] = 1;
                else
                    map[x, y] = (pRand.Next(0, 100) < randomFillPercent) ? 1 : 0; //If less than full %, add wall. if greater, leave blank
            }
        }
    }

    void SmoothMap()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                int neighbourWallTiles = GetSurroundingWallCount(x, y);
                //these following rules can determine what kind of map shapes you're going to get
                if (neighbourWallTiles > 4) map[x, y] = 1;
                else if (neighbourWallTiles < 4) map[x, y] = 0;

            }
        }
    }

    int GetSurroundingWallCount(int gridX, int gridY)
    {
        int wallCount = 0;
        //check 8 tiles surrounding current one, however this could be changed depending on desired effect
        for (int neighbourX = gridX - 1; neighbourX <= gridX + 1; neighbourX++){ //horiz
            for (int neighbourY = gridY - 1; neighbourY <= gridY + 1; neighbourY++){ //vert
                if (neighbourX >= 0 && neighbourX < width && neighbourY >= 0 && neighbourY < height){ //stay within map bounds
                    if (neighbourX != gridX || neighbourY != gridY){ //don't consider tile we're looking at
                        wallCount += map[neighbourX, neighbourY]; //if it's a wall, increase
                    }
                }
                else
                    wallCount++; //encourages growth of walls around edges of map
            }
        }
        return wallCount;
    }
    /*void OnDrawGizmos()
    {
        if (map != null)
        {
            for (int x = 0; x < width; x++){
                for (int y = 0; y < height; y++)
                {
                    Gizmos.color = (map[x, y] == 1) ? Color.black : Color.white;
                    Vector3 pos = new Vector3(-width/2 + x + .5f, 0, -height/2 + y + 0.5f);
                    Gizmos.DrawCube(pos, Vector3.one);
                }
            }
        }
    }*/
}
