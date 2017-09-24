using System;
using spaar.ModLoader;
using System.Collections;
using System.IO;
using UnityEngine;

namespace TerrainMod
{

    // If you need documentation about any of these values or the mod loader
    // in general, take a look at https://spaar.github.io/besiege-modloader.

    public class TerrainMod : Mod
    {
        public override string Name { get; } = "TerrainMod";
        public override string DisplayName { get; } = "Besiege Terrain Mod";
        public override string Author { get; } = "Orz_PX";
        public override Version Version { get; } = new Version(1, 0, 0);

        // You don't need to override this, if you leavie it out it will default
        // to an empty string.
        // public override string VersionExtra { get; } = "";

        // You don't need to override this, if you leave it out it will default
        // to the current version.
        // public override string BesiegeVersion { get; } = "v0.45";

        // You don't need to override this, if you leave it out it will default
        // to false.
        public override bool CanBeUnloaded { get; } = true;

        // You don't need to override this, if you leave it out it will default
        // to false.
        // public override bool Preload { get; } = false;

        public GameObject terrainMod;


        public override void OnLoad()
        {
            // Your initialization code here
            terrainMod = new GameObject
            {
                name = "Besiege Terrain Mod"
            };
            terrainMod.AddComponent<UI>();
            terrainMod.AddComponent<TerrainCluster>();
            terrainMod.AddComponent<TerrainDeformer>();
            UnityEngine.Object.DontDestroyOnLoad(terrainMod);
        }

        public override void OnUnload()
        {
            // Your code here
            // e.g. save configuration, destroy your objects if CanBeUnloaded is true etc
            if (terrainMod.GetComponent<TerrainCluster>().defaultTerrainToggled)
            {
                terrainMod.GetComponent<TerrainCluster>().ToggleDefaultFloor();
                if (terrainMod.GetComponent<TerrainCluster>().terrainCluster != null)
                {
                    foreach (var terrain in terrainMod.GetComponent<TerrainCluster>().terrainCluster)
                    {
                        UnityEngine.Object.Destroy(terrain);
                        Debug.Log("terrain deleted");
                    }
                }
            }
            UnityEngine.Object.Destroy(terrainMod);
        }
    }
}
