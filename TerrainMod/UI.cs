using UnityEngine;


namespace TerrainMod
{
    class UI : MonoBehaviour
    {
        private Rect rect = new Rect(20f, 20f, 250f, 400f);
        private int windowID = spaar.ModLoader.Util.GetWindowID();
        private int terrainSize = 10;
        public int TerrainSize { get { return terrainSize; } set { terrainSize = value; } }


        private void DoMyWindow(int windowID)
        {
            GUILayout.BeginHorizontal(new GUILayoutOption[0]);
            GUILayout.Label("Tiles per side:", new GUILayoutOption[0]);
            TerrainSize = int.Parse(GUILayout.TextField(TerrainSize.ToString(), new GUILayoutOption[0]));
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal(new GUILayoutOption[0]);
            if (GUILayout.Button("Revert to default terrain", new GUILayoutOption[0]))
            {
                if (GetComponent<TerrainCluster>().defaultTerrainToggled)
                {
                    GetComponent<TerrainCluster>().ToggleDefaultFloor();
                    if (GetComponent<TerrainCluster>().terrainCluster != null)
                    {
                        foreach (var terrain in GetComponent<TerrainCluster>().terrainCluster)
                        {
                            Destroy(terrain);
                            Debug.Log("terrain deleted");
                        }
                    }
                }
            }
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal(new GUILayoutOption[0]);
            if (GUILayout.Button("Create Terrain", new GUILayoutOption[0]))
            {
                GetComponent<TerrainCluster>().CreateTerrains();
            }
            GUILayout.EndHorizontal();
            GUI.DragWindow();
        }

        private void OnGUI()
        {
            rect = GUI.Window(windowID, rect, new GUI.WindowFunction(DoMyWindow), "Terrain Mod");
        }          
 
       
    }
}