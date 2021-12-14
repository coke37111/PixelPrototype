using Assets.Scripts.Feature.GenSample;
using UnityEditor;
using UnityEngine;

namespace Assets.Scripts.Managers
{
    [CustomEditor(typeof(GenSampleManager))]
    public class GenSampleManagerEditor : Editor
    {
        void OnSceneGUI()
        {
            GenSampleManager connectedObjects = target as GenSampleManager;
            if (connectedObjects.srBg == null)
                return;

            Vector3 spawnArea = connectedObjects.GetSpawnArea();
            Handles.color = Color.red;
            
            Handles.DrawLines(new Vector3[]{
                new Vector3(-spawnArea.x, -spawnArea.y),
                new Vector3(-spawnArea.x, spawnArea.y),
                new Vector3(-spawnArea.x, spawnArea.y),
                new Vector3(spawnArea.x, spawnArea.y),
                new Vector3(spawnArea.x, spawnArea.y),
                new Vector3(spawnArea.x, -spawnArea.y),
                new Vector3(spawnArea.x, -spawnArea.y),
                new Vector3(-spawnArea.x, -spawnArea.y),
            });

            Handles.color = Color.cyan;
            foreach (Unit unit in connectedObjects.GetUnitList())
            {
                Handles.DrawLine(unit.GetSpawnPos(), unit.transform.position);
            }
        }
    }
}