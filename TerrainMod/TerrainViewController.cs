using System;
using spaar.ModLoader;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TerrainMod
{
    class TerrainViewController : MonoBehaviour
    {
        public Shader originalTerrainShader;
        private bool coroutineStarted = false;

        void Update()
        {
            if (StatMaster.isSimulating && !coroutineStarted)
            {
                StartCoroutine(CheckInTheView());
                coroutineStarted = true;
            }
            else if (!StatMaster.isSimulating)
            {
                StopAllCoroutines();
            }


        }

        IEnumerator CheckInTheView()
        {
            Plane[] currentPlane = GeometryUtility.CalculateFrustumPlanes(Camera.main);
            bool isInCameraView = GeometryUtility.TestPlanesAABB(currentPlane, gameObject.GetComponent<TerrainCollider>().bounds);
            float terrain2CameraDistance = currentPlane[4].GetDistanceToPoint(gameObject.GetComponent<Terrain>().GetPosition());
            
            if (isInCameraView && terrain2CameraDistance <= 100)
            {
                //gameObject.GetComponent<Terrain>().enabled = true;
                gameObject.GetComponent<Terrain>().materialTemplate.shader.maximumLOD = 10;
            }
            else
            {
                //gameObject.GetComponent<Terrain>().enabled = false;
                gameObject.GetComponent<Terrain>().materialTemplate.shader.maximumLOD = 600;
            }
            yield return null;
        }
    }
}