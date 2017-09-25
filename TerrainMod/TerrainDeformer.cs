using System;
using spaar.ModLoader;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TerrainMod
{
    class TerrainDeformer : MonoBehaviour
    {        
        private Queue<Collision> currentCollisions = new Queue<Collision>();

        private int updateCount = 0;
        private Char delimiter = '-';
        private int indexI;
        private int indexJ;
        private String[] terrainNameSplit;

        private GameObject parentTerrain;
        private GameObject[,] terrainCluster;
        Terrain bottomTerrain;
        Terrain topTerrain;
        Terrain leftTerrain;
        Terrain rightTerrain;
        private float[][,] currentTileEdgeLOD;
        private float[][,] bottomTileEdgeLOD;
        private float[][,] topTileEdgeLOD;
        private float[][,] leftTileEdgeLOD;
        private float[][,] rightTileEdgeLOD;
        //private int secBottom = 0;
        //private int secTop = 1;
        //private int secLeft = 2;
        //private int secRight = 3;
        private int bottom = 4;
        private int top = 5;
        private int left = 6;
        private int right = 7;

        private bool terrainLODUpdateStarted = false;

        //void OnEnable()
        //{
        //    SceneManager.sceneLoaded += OnSceneLoaded;
        //    print("script was enabled");
        //    //if (GameObject.Find("Besiege Terrain Mod").GetComponent<TerrainCluster>().terrainCluster.Length > 0)
        //    //{
        //    //    StartCoroutine(TryCoroutineEdgeCorrection());
        //    //}
        //}

        //void OnDisable()
        //{
        //    SceneManager.sceneLoaded -= OnSceneLoaded;
        //}

        //private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        //{           
        //}

        private void OnCollisionExit(Collision c)
        {
            try
            {
                if (Enum.IsDefined(typeof(BlockType), c.gameObject.GetComponent<MachineTrackerMyId>().myId))
                {
                    currentCollisions.Enqueue(c);
                }
            }
            catch (Exception)
            {
            }
        }

        void Update()
        {
            if (StatMaster.isSimulating)
            {
                parentTerrain = gameObject;
                int collisionCount = currentCollisions.Count;
                if (!terrainLODUpdateStarted && collisionCount > 0)
                {
                    StartCoroutine(UpdateTerrainLOD());
                    terrainLODUpdateStarted = !terrainLODUpdateStarted;
                }
                else if (collisionCount == 0)
                {
                    StopCoroutine(UpdateTerrainLOD());
                    terrainLODUpdateStarted = !terrainLODUpdateStarted;
                }
                while (currentCollisions.Count > 0)
                {
                    Collision collidedObject = currentCollisions.Dequeue();
                    int xCoord = Mathf.RoundToInt((collidedObject.transform.position.x - parentTerrain.transform.position.x) / parentTerrain.GetComponent<Terrain>().terrainData.size.x * parentTerrain.GetComponent<Terrain>().terrainData.heightmapWidth);
                    int zCoord = Mathf.RoundToInt((collidedObject.transform.position.z - parentTerrain.transform.position.z) / parentTerrain.GetComponent<Terrain>().terrainData.size.z * parentTerrain.GetComponent<Terrain>().terrainData.heightmapWidth);
                    try
                    {
                        float[,] newLOD = new float[1, 1];
                        newLOD[0, 0] = parentTerrain.GetComponent<Terrain>().terrainData.GetHeights(xCoord, zCoord, 1, 1)[0, 0] - 0.0001f;
                        if (newLOD[0, 0] > 0.495)
                        {
                            Vector3 xzVelocityNormalised = new Vector3(collidedObject.relativeVelocity.x, 0, collidedObject.relativeVelocity.z).normalized;
                            int deformForecast = 3;
                            for (int i = 0; i < deformForecast; i++)
                            {
                                for (int j = 0; j < deformForecast; j++)
                                {
                                    //int xPosition = xCoord + Mathf.RoundToInt((i - deformForecast / 2) * xzVelocityNormalised.normalized.x);
                                    //int zPosition = zCoord + Mathf.RoundToInt((j - deformForecast / 2) * xzVelocityNormalised.normalized.z);
                                    int xPosition = xCoord + Mathf.RoundToInt(i * xzVelocityNormalised.normalized.x);
                                    int zPosition = zCoord + Mathf.RoundToInt(j * xzVelocityNormalised.normalized.z);
                                    parentTerrain.GetComponent<Terrain>().terrainData.SetHeightsDelayLOD(xPosition, zPosition, newLOD);
                                }
                            }
                        }
                    }
                    catch (Exception)
                    {
                    }
                }
                if (collisionCount > 0)
                {
                    //Only correct terrain tiles' edges when there are collision on them
                    CorrectTerrainEdge();
                }
                //if (updateCount < 25)
                //{
                //    updateCount++;
                //}
                //else
                //{
                //    updateCount = 0;
                //    parentTerrain.GetComponent<Terrain>().ApplyDelayedHeightmapModification();
                //}

            }
            else
            {
                currentCollisions.Clear();
            }
        }

        void CorrectTerrainEdge()
        {
            Terrain terrain = gameObject.GetComponent<Terrain>();
            
            int terrainGridCount = terrain.terrainData.heightmapWidth;
            terrainCluster = GameObject.Find("Besiege Terrain Mod").GetComponent<TerrainCluster>().terrainCluster;

            //float[,] BottomHeights = terrain.terrainData.GetHeights(0, 0, terrainGridCount, 1);
            //float[,] TopHeights = terrain.terrainData.GetHeights(0, terrainGridCount - 1, terrainGridCount, 1);
            //float[,] LeftHeights = terrain.terrainData.GetHeights(0, 0, 1, terrainGridCount);
            //float[,] RightHeights = terrain.terrainData.GetHeights(terrainGridCount - 1, 0, 1, terrainGridCount);

            terrainNameSplit = gameObject.name.Split(delimiter);
            indexI = int.Parse(terrainNameSplit[1]);
            indexJ = int.Parse(terrainNameSplit[2]);

            try
            {
                bottomTerrain = terrainCluster[indexI, indexJ + 1].GetComponent<Terrain>();
            }
            catch (Exception)
            {
            }
            try
            {
                topTerrain = terrainCluster[indexI, indexJ - 1].GetComponent<Terrain>();
            }
            catch (Exception)
            {
            }
            try
            {
                leftTerrain = terrainCluster[indexI + 1, indexJ].GetComponent<Terrain>();
            }
            catch (Exception)
            {
            }
            try
            {
                rightTerrain = terrainCluster[indexI - 1, indexJ].GetComponent<Terrain>();
            }
            catch (Exception)
            {
            }

            currentTileEdgeLOD = GetEdgeLOD(terrain);
            float[,] bottomHeights = currentTileEdgeLOD[4];
            float[,] topHeights = currentTileEdgeLOD[5];
            float[,] leftHeights = currentTileEdgeLOD[6];
            float[,] rightHeights = currentTileEdgeLOD[7];
            

            if (bottomTerrain != null)
            {
                bottomTileEdgeLOD = GetEdgeLOD(bottomTerrain);
                float[,] aveTopBottomEdgeLOD = AveTwo2DArray(bottomHeights, bottomTileEdgeLOD[top]);
                terrain.terrainData.SetHeightsDelayLOD(0, 0, aveTopBottomEdgeLOD);
                bottomTerrain.terrainData.SetHeightsDelayLOD(0, terrainGridCount - 1, aveTopBottomEdgeLOD);
            }
            if (topTerrain != null)
            {
                topTileEdgeLOD = GetEdgeLOD(topTerrain);
                float[,] aveTopBottomEdgeLOD = AveTwo2DArray(topHeights, topTileEdgeLOD[bottom]);
                terrain.terrainData.SetHeightsDelayLOD(0, terrainGridCount - 1, aveTopBottomEdgeLOD);
                topTerrain.terrainData.SetHeightsDelayLOD(0, 0, aveTopBottomEdgeLOD);
            }
            if (leftTerrain != null)
            {
                leftTileEdgeLOD = GetEdgeLOD(leftTerrain);
                float[,] aveLeftRightEdgeLOD = AveTwo2DArray(leftHeights, leftTileEdgeLOD[right]);
                terrain.terrainData.SetHeightsDelayLOD(0, 0, aveLeftRightEdgeLOD);
                leftTerrain.terrainData.SetHeightsDelayLOD(terrainGridCount - 1, 0, aveLeftRightEdgeLOD);
            }
            if (rightTerrain != null)
            {
                rightTileEdgeLOD = GetEdgeLOD(rightTerrain);
                float[,] aveLeftRightEdgeLOD = AveTwo2DArray(rightHeights, rightTileEdgeLOD[left]);
                terrain.terrainData.SetHeightsDelayLOD(terrainGridCount - 1, 0, aveLeftRightEdgeLOD);
                rightTerrain.terrainData.SetHeightsDelayLOD(0, 0, aveLeftRightEdgeLOD);
            }

        }
        IEnumerator UpdateTerrainLOD()
        {
            //Debug.Log("in coroutine");
            gameObject.GetComponent<Terrain>().ApplyDelayedHeightmapModification();
            yield return new WaitForSeconds(5f);
            //yield return null;
        }

        private float[][,] GetEdgeLOD(Terrain terrain)
        {
            float[][,] edgeLOD = new float[8][,];
            int terrainGridCount = terrain.terrainData.heightmapWidth;

            float[,] bottomHeights = terrain.terrainData.GetHeights(0, 0, terrainGridCount, 1);
            float[,] topHeights = terrain.terrainData.GetHeights(0, terrainGridCount - 1, terrainGridCount, 1);
            float[,] leftHeights = terrain.terrainData.GetHeights(0, 0, 1, terrainGridCount);
            float[,] rightHeights = terrain.terrainData.GetHeights(terrainGridCount - 1, 0, 1, terrainGridCount);
            //Debug.Log(bottomHeights.GetLength(0) + "," + bottomHeights.GetLength(1));
            //Debug.Log(leftHeights.GetLength(0) + "," + leftHeights.GetLength(1));
            edgeLOD[4] = bottomHeights;
            edgeLOD[5] = topHeights;
            edgeLOD[6] = leftHeights;
            edgeLOD[7] = rightHeights;

            return edgeLOD;
        }
        private float[,] AveTwo2DArray(float[,] array1, float[,] array2)
        {
            float[,] min2DArray = new float[array1.GetLength(0), array1.GetLength(1)];
            for (int i = 0; i < array1.GetLength(0); i++)
            {
                for (int j = 0; j < array1.GetLength(1); j++)
                {
                    //ave2Darray[i, j] = (array1[i, j] + array2[i, j]) / 2;
                    min2DArray[i, j] = Math.Min(array1[i, j], array2[i, j]);
                }
            }
            return min2DArray;
        }
    }
}