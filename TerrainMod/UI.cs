using spaar.ModLoader.UI;
using spaar.ModLoader;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;


namespace TerrainMod
{
    class UI : MonoBehaviour
    {
        private Rect rect = new Rect(20f, 20f, 250f, 400f);
        private int windowID = spaar.ModLoader.Util.GetWindowID();
        private int terrainSize = 20;
        public int TerrainSize { get { return terrainSize; } set { terrainSize = value; } }


        private void DoMyWindow(int windowID)
        {
            GUILayout.BeginHorizontal(new GUILayoutOption[0]);
            GUILayout.Label("Terrain Size:", new GUILayoutOption[0]);
            TerrainSize = int.Parse(GUILayout.TextField(TerrainSize.ToString(), new GUILayoutOption[0]));
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal(new GUILayoutOption[0]);
            if (GUILayout.Button("Toggle Original Terrain", new GUILayoutOption[0]))
            {
                GetComponent<TerrainCluster>().ToggleDefaultFloor();
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
            GUI.skin = ModGUI.Skin;
            GUI.skin.textField.fontSize = 10;
            GUI.skin.label.fontSize = 10;
            GUI.skin.window.fontSize = 10;
            GUI.skin.button.fontSize = 10;
            rect = GUI.Window(windowID, rect, new GUI.WindowFunction(DoMyWindow), "Terrain Mod");
        }          
 
       
    }
}