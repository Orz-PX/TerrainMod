using System;
using spaar.ModLoader;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TerrainMod
{
    class TerrainCluster : MonoBehaviour
    {
        private int terrainSize = 0;
        private bool defaultTerrainToggled = false;
        public bool DefaultTerrainToggled { get { return defaultTerrainToggled; } set { defaultTerrainToggled = value; } }
        private Vector3 defaultFloorSize = Vector3.zero;
        private Vector3 defaultFloorGridSize = Vector3.zero;
        private float[,] heights;
        private int tileSize = 16;
        private int heightMapResolution = 65;
        private float terrainHeight = 100f;
        
        private TerrainData terrainData;
        private GameObject[,] terrainCluster;
        public GameObject[,] GetTerrainCluster()
        {
            return terrainCluster;
        }

        void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            try
            {
                Debug.Log(Game.GetCurrentZone().Island);
                defaultFloorSize = GameObject.Find("FloorBig").transform.localScale;
                defaultFloorGridSize = GameObject.Find("FloorGrid").transform.localScale;
            }
            catch (Exception)
            {
                Debug.Log("You're not in a zone");
            }

        }

        public void ToggleDefaultFloor()
        {
            if (DefaultTerrainToggled)
            {
                GameObject.Find("FloorBig").transform.localScale = defaultFloorSize;
                GameObject.Find("FloorGrid").transform.localScale = defaultFloorGridSize;
            }
            else
            {
                GameObject.Find("FloorBig").transform.localScale = Vector3.zero;
                GameObject.Find("FloorGrid").transform.localScale = Vector3.zero;
            }
            DefaultTerrainToggled = !DefaultTerrainToggled;
        }

        public void CreateTerrains()
        {
            if (terrainCluster != null)
            {
                foreach (var terrain in terrainCluster)
                {
                    Destroy(terrain);
                    Debug.Log("terrain deleted");
                }
            }
            terrainSize = GetComponent<UI>().TerrainSize;
            terrainCluster = new GameObject[terrainSize, terrainSize];
            terrainData = new TerrainData();
            Vector3 floorSize = new Vector3(tileSize, terrainHeight, tileSize);

            terrainData = new TerrainData
            {
                heightmapResolution = heightMapResolution,
                size = floorSize,
                thickness = 10f,
            };
            heights = new float[terrainData.heightmapWidth, terrainData.heightmapHeight];
            for (int i = 0; i < terrainData.heightmapWidth; i++)
            {
                for (int j = 0; j < terrainData.heightmapHeight; j++)
                {
                    heights[i, j] = 0.5f;
                }
            }
            terrainData.SetHeights(0, 0, heights);
            
            // Creating terrain cluster
            for (int i = 0; i < terrainSize; i++)
            {
                for (int j = 0; j < terrainSize; j++)
                {
                    
                    terrainCluster[i, j] = Terrain.CreateTerrainGameObject(Instantiate(terrainData));
                    terrainCluster[i, j].GetComponent<Terrain>().materialTemplate = GameObject.Find("FloorBig").GetComponent<MeshRenderer>().material;
                    //terrainCluster[i, j].GetComponent<Terrain>().materialTemplate.CopyPropertiesFromMaterial(GameObject.Find("FloorBig").GetComponent<MeshRenderer>().material);
                    //terrainCluster[i, j].GetComponent<Terrain>().materialTemplate.color = Color.white;
                    terrainCluster[i, j].GetComponent<Terrain>().materialType = Terrain.MaterialType.Custom;
                    // name all terrains
                    if (i < 10 && j < 10)
                    {
                        terrainCluster[i, j].name = String.Concat("Terrain", "-0", i, "-0", j);
                    }
                    else if (i < 10 && j >= 10)
                    {
                        terrainCluster[i, j].name = String.Concat("Terrain", "-0", i, "-", j);
                    }
                    else if (i >= 10 && j < 10)
                    {
                        terrainCluster[i, j].name = String.Concat("Terrain", "-", i, "-0", j);
                    }
                    else
                    {
                        terrainCluster[i, j].name = String.Concat("Terrain", "-", i, "-", j);
                    }

                    terrainCluster[i, j].AddComponent<TerrainCollisionDetector>();
                    if (terrainSize % 2 == 0)
                    {
                        terrainCluster[i, j].transform.Translate(new Vector3(-tileSize * (i - (terrainSize - 1) / 2), -terrainHeight / 2, -tileSize * (j - (terrainSize - 1) / 2)));
                    }
                    else
                    {
                        terrainCluster[i, j].transform.Translate(new Vector3(-tileSize * (i - (terrainSize - 1) / 2 + 0.5f), -terrainHeight / 2, -tileSize * (j - (terrainSize - 1) / 2 + 0.5f)));
                    }
                }
            }
            // Setting up neighbour for each terrain object when terrainsize > 1
            if (terrainSize > 1)
            {
                for (int i = 0; i < terrainSize; i++)
                {
                    for (int j = 0; j < terrainSize; j++)
                    {
                        if (i == 0 || j == 0 || i == terrainSize - 1 || j == terrainSize - 1)
                        {
                            if (i == 0)
                            {
                                if (j == 0)
                                {
                                    terrainCluster[i, j].GetComponent<Terrain>().SetNeighbors(new Terrain(), terrainCluster[i, j + 1].GetComponent<Terrain>(),
                                        terrainCluster[i + 1, j].GetComponent<Terrain>(), new Terrain());
                                }
                                else if (j == terrainSize - 1)
                                {
                                    terrainCluster[i, j].GetComponent<Terrain>().SetNeighbors(new Terrain(), new Terrain(), terrainCluster[i + 1, j].GetComponent<Terrain>(),
                                        terrainCluster[i, j - 1].GetComponent<Terrain>());
                                }
                                else
                                {
                                    terrainCluster[i, j].GetComponent<Terrain>().SetNeighbors(new Terrain(), terrainCluster[i, j + 1].GetComponent<Terrain>(), terrainCluster[i + 1, j].GetComponent<Terrain>(), terrainCluster[i, j - 1].GetComponent<Terrain>());
                                }
                            }
                            else
                            {
                                if (j == 0)
                                {
                                    terrainCluster[i, j].GetComponent<Terrain>().SetNeighbors(terrainCluster[i - 1, j].GetComponent<Terrain>(), terrainCluster[i, j + 1].GetComponent<Terrain>(),
                                        new Terrain(), new Terrain());
                                }
                                else if (j == terrainSize - 1)
                                {
                                    terrainCluster[i, j].GetComponent<Terrain>().SetNeighbors(terrainCluster[i - 1, j].GetComponent<Terrain>(), new Terrain(),
                                        new Terrain(), terrainCluster[i, j - 1].GetComponent<Terrain>());
                                }
                                else
                                {
                                    terrainCluster[i, j].GetComponent<Terrain>().SetNeighbors(terrainCluster[i - 1, j].GetComponent<Terrain>(), terrainCluster[i, j + 1].GetComponent<Terrain>(),
                                        new Terrain(), terrainCluster[i, j - 1].GetComponent<Terrain>());
                                }
                            }
                        }
                        else
                        {
                            terrainCluster[i, j].GetComponent<Terrain>().SetNeighbors(terrainCluster[i - 1, j].GetComponent<Terrain>(), terrainCluster[i, j + 1].GetComponent<Terrain>(),
                                terrainCluster[i + 1, j].GetComponent<Terrain>(), terrainCluster[i, j - 1].GetComponent<Terrain>());
                        }
                    }
                }
            }

        }
    }

    class TerrainCollisionDetector : MonoBehaviour
    {
        private Queue<Collision> currentCollisions = new Queue<Collision>();

        private int updateCount = 0;
        private Char delimiter = '-';
        private GameObject parentTerrain;
        private String[] terrainNameSplit;
        private int indexI;
        private int indexJ;

        private void OnCollisionEnter(Collision c)
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
                terrainNameSplit = gameObject.name.Split(delimiter);
                indexI = int.Parse(terrainNameSplit[1]);
                indexJ = int.Parse(terrainNameSplit[2]);
                while (currentCollisions.Count > 0)
                {
                    Collision collidedObject = currentCollisions.Dequeue();
                    int xCoord = Mathf.FloorToInt((collidedObject.transform.position.x - parentTerrain.transform.position.x) / parentTerrain.GetComponent<Terrain>().terrainData.size.x * parentTerrain.GetComponent<Terrain>().terrainData.heightmapWidth);
                    int zCoord = Mathf.FloorToInt((collidedObject.transform.position.z - parentTerrain.transform.position.z) / parentTerrain.GetComponent<Terrain>().terrainData.size.z * parentTerrain.GetComponent<Terrain>().terrainData.heightmapWidth);
                    try
                    {
                        float[,] newLOD = new float[1, 1];
                        newLOD[0, 0] = parentTerrain.GetComponent<Terrain>().terrainData.GetHeights(xCoord, zCoord, 1, 1)[0, 0] - 0.001f;
                        if (newLOD[0, 0] > 0.495)
                        {
                            Vector3 xzVelocityNormalised = new Vector3(collidedObject.relativeVelocity.x, 0, collidedObject.relativeVelocity.z).normalized;
                            int deformForecast = 10;
                            for (int i = 0; i < deformForecast; i++)
                            {
                                for (int j = 0; j < deformForecast; j++)
                                {
                                    int xPosition = xCoord + Mathf.RoundToInt((i - deformForecast / 2) * xzVelocityNormalised.normalized.x);
                                    int zPosition = zCoord + Mathf.RoundToInt((j - deformForecast / 2) * xzVelocityNormalised.normalized.z);
                                    parentTerrain.GetComponent<Terrain>().terrainData.SetHeightsDelayLOD(xPosition, zPosition, newLOD);

                                    if (xPosition == 0)
                                    {
                                        //Bottom
                                    }
                                    if (xPosition == parentTerrain.GetComponent<Terrain>().terrainData.heightmapWidth)
                                    {
                                        //Top

                                    }
                                    if (zPosition == 0)
                                    {
                                        //Left

                                    }
                                    if (zPosition == parentTerrain.GetComponent<Terrain>().terrainData.heightmapHeight)
                                    {
                                        //Right

                                    }
                                }
                            }
                            //parentTerrain.GetComponent<Terrain>().terrainData.SetHeightsDelayLOD(xCoord, zCoord, newLOD);
                            //parentTerrain.GetComponent<Terrain>().terrainData.SetHeightsDelayLOD(xCoord + Mathf.RoundToInt(xzVelocityNormalised.normalized.x),
                            //     zCoord, newLOD);
                            //parentTerrain.GetComponent<Terrain>().terrainData.SetHeightsDelayLOD(xCoord, zCoord + Mathf.RoundToInt(xzVelocityNormalised.normalized.z), newLOD);
                            //parentTerrain.GetComponent<Terrain>().terrainData.SetHeightsDelayLOD(xCoord + Mathf.RoundToInt(xzVelocityNormalised.normalized.x),
                            //    zCoord + Mathf.RoundToInt(xzVelocityNormalised.normalized.z), newLOD);
                        }
                        if (updateCount != 25)
                        {
                            updateCount++;
                        }
                        else
                        {
                            updateCount = 0;
                            parentTerrain.GetComponent<Terrain>().ApplyDelayedHeightmapModification();
                        }
                    }
                    catch (Exception)
                    {
                    }
                    
                }
            }
        }
    }
}