using System;
using spaar.ModLoader;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TerrainMod
{
    class TerrainCluster : MonoBehaviour
    {
        private int tilePerSide = 0;
        public bool defaultTerrainToggled = false;
        public bool DefaultTerrainToggled { get { return defaultTerrainToggled; } set { defaultTerrainToggled = value; } }
        private Vector3 defaultFloorSize = Vector3.zero;
        private Vector3 defaultFloorGridSize = Vector3.zero;
        private float[,] heights;
        private int tileSize = 96;
        private int heightMapResolution = 257;
        private float terrainHeight = 100f;
        private int heightmapPixelError = 5;
        private TerrainData terrainData;
        public Dictionary<int, GameObject> terrainCluster;
        

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
            if (!defaultTerrainToggled)
            {
                ToggleDefaultFloor();
            }
            if (terrainCluster != null)
            {
                foreach (var terrain in terrainCluster)
                {
                    Destroy(terrain.Value);
                }
                terrainCluster.Clear();
            }
            tilePerSide = GameObject.Find("Besiege Terrain Mod").GetComponent<UI>().TilePerSide;
            terrainCluster = new Dictionary<int, GameObject>();
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
            for (int i = 0; i < tilePerSide; i++)
            {
                for (int j = 0; j < tilePerSide; j++)
                {
                    //terrainCluster.Add(i*terrainSize+j, Terrain.CreateTerrainGameObject(Instantiate(terrainData)));
                    GameObject tempTerrainObject = Terrain.CreateTerrainGameObject(Instantiate(terrainData));
                    tempTerrainObject.isStatic = true;
                    tempTerrainObject.GetComponent<Terrain>().materialType = Terrain.MaterialType.Custom;
                    tempTerrainObject.GetComponent<Terrain>().materialTemplate = Instantiate(GameObject.Find("FloorBig").GetComponent<MeshRenderer>().material);
                    tempTerrainObject.GetComponent<Terrain>().materialTemplate.globalIlluminationFlags = MaterialGlobalIlluminationFlags.BakedEmissive;
                    //if (i == 0 && j == 0)
                    //{
                    //    tempTerrainObject.GetComponent<Terrain>().materialTemplate.color = Color.black;
                    //}
                    //if (i == 1 && j == 1)
                    //{
                    //    tempTerrainObject.GetComponent<Terrain>().materialTemplate.color = Color.blue;
                    //}
                    //if (i == 0 & j == 1)
                    //{
                    //    tempTerrainObject.GetComponent<Terrain>().materialTemplate.color = Color.green;
                    //}
                    tempTerrainObject.GetComponent<Terrain>().heightmapPixelError = heightmapPixelError;
                    tempTerrainObject.GetComponent<Terrain>().detailObjectDistance = 25;
                    tempTerrainObject.GetComponent<Terrain>().detailObjectDensity = 0f;
                    tempTerrainObject.GetComponent<Terrain>().castShadows = false;
                    tempTerrainObject.GetComponent<Terrain>().drawTreesAndFoliage = false;
                    tempTerrainObject.GetComponent<Terrain>().editorRenderFlags = TerrainRenderFlags.heightmap;
                    tempTerrainObject.AddComponent<TerrainDeformer>();
                    // name all terrains
                    tempTerrainObject.name = String.Concat("Terrain", "-", i, "-", j);
                    
                    // Shift tiles to the center
                    if (tilePerSide % 2 == 0)
                    {
                        tempTerrainObject.transform.Translate(new Vector3(-tileSize * (i - (tilePerSide - 1) / 2), -terrainHeight / 2 - 0.05f, -tileSize * (j - (tilePerSide - 1) / 2)));
                    }
                    else
                    {
                        tempTerrainObject.transform.Translate(new Vector3(-tileSize * (i - (tilePerSide - 1) / 2 + 0.5f), -terrainHeight / 2 - 0.05f, -tileSize * (j - (tilePerSide - 1) / 2 + 0.5f)));
                    }
                    terrainCluster.Add(i * tilePerSide + j, tempTerrainObject);
                }
            }
            // Setting up neighbour for each terrain object when terrainsize > 1            
            if (tilePerSide > 1)
            {
                for (int i = 0; i < tilePerSide; i++)
                {
                    for (int j = 0; j < tilePerSide; j++)
                    {
                        Terrain bottomTerrain = new Terrain();
                        Terrain topTerrain = new Terrain(); 
                        Terrain leftTerrain = new Terrain(); 
                        Terrain rightTerrain = new Terrain();
                        try
                        {
                            bottomTerrain = terrainCluster[i * tilePerSide + j + 1].GetComponent<Terrain>();
                        }
                        catch (Exception)
                        {
                        }
                        try
                        {
                            topTerrain = terrainCluster[i * tilePerSide + j - 1].GetComponent<Terrain>();
                        }
                        catch (Exception)
                        {
                        }
                        try
                        {
                            leftTerrain = terrainCluster[(i + 1) * tilePerSide + j].GetComponent<Terrain>();
                        }
                        catch (Exception)
                        {
                        }
                        try
                        {
                            rightTerrain = terrainCluster[(i - 1) * tilePerSide + j].GetComponent<Terrain>();
                        }
                        catch (Exception)
                        {
                        }
                        terrainCluster[i * tilePerSide + j].GetComponent<Terrain>().SetNeighbors(leftTerrain, topTerrain, rightTerrain, bottomTerrain);
                        terrainCluster[i * tilePerSide + j].GetComponent<Terrain>().Flush();
                    }
                }
            }

        }

        void Update()
        {
            if (!StatMaster.isSimulating)
            {
                StopAllCoroutines();
            }
        }
    }
}