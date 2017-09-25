using System;
using spaar.ModLoader;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TerrainMod
{
    class TerrainCluster : MonoBehaviour
    {
        private int terrainSize = 0;
        public bool defaultTerrainToggled = false;
        public bool DefaultTerrainToggled { get { return defaultTerrainToggled; } set { defaultTerrainToggled = value; } }
        private Vector3 defaultFloorSize = Vector3.zero;
        private Vector3 defaultFloorGridSize = Vector3.zero;
        private float[,] heights;
        private int tileSize = 32;
        private int heightMapResolution = 65;
        private float terrainHeight = 100f;
        private TerrainData terrainData;
        public GameObject[,] terrainCluster;
        

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
                    Destroy(terrain);
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
                thickness = 10f
            };
            //terrainData.SetDetailResolution(512,2);
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
                    terrainCluster[i, j].GetComponent<Terrain>().materialTemplate = Instantiate(GameObject.Find("FloorBig").GetComponent<MeshRenderer>().material);
                    terrainCluster[i, j].GetComponent<Terrain>().materialType = Terrain.MaterialType.Custom;
                    terrainCluster[i, j].GetComponent<Terrain>().detailObjectDistance = 40;
                    terrainCluster[i, j].AddComponent<TerrainDeformer>();
                    // name all terrains
                    terrainCluster[i, j].name = String.Concat("Terrain", "-", i, "-", j);
                    
                    // Shift tiles to the center
                    if (terrainSize % 2 == 0)
                    {
                        terrainCluster[i, j].transform.Translate(new Vector3(-tileSize * (i - (terrainSize - 1) / 2), -terrainHeight / 2 - 0.05f, -tileSize * (j - (terrainSize - 1) / 2)));
                    }
                    else
                    {
                        terrainCluster[i, j].transform.Translate(new Vector3(-tileSize * (i - (terrainSize - 1) / 2 + 0.5f), -terrainHeight / 2 - 0.05f, -tileSize * (j - (terrainSize - 1) / 2 + 0.5f)));
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
                        Terrain bottomTerrain = new Terrain();
                        Terrain topTerrain = new Terrain(); 
                        Terrain leftTerrain = new Terrain(); 
                        Terrain rightTerrain = new Terrain();
                        try
                        {
                            bottomTerrain = terrainCluster[i, j + 1].GetComponent<Terrain>();
                        }
                        catch (Exception)
                        {
                        }
                        try
                        {
                            topTerrain = terrainCluster[i, j - 1].GetComponent<Terrain>();
                        }
                        catch (Exception)
                        {
                        }
                        try
                        {
                            leftTerrain = terrainCluster[i + 1, j].GetComponent<Terrain>();
                        }
                        catch (Exception)
                        {
                        }
                        try
                        {
                            rightTerrain = terrainCluster[i - 1, j].GetComponent<Terrain>();
                        }
                        catch (Exception)
                        {
                        }
                        terrainCluster[i, j].GetComponent<Terrain>().SetNeighbors(leftTerrain, topTerrain, rightTerrain, bottomTerrain);
                        terrainCluster[i, j].GetComponent<Terrain>().Flush();
                    }
                }
            }

        }
    }
}