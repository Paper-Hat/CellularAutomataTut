using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
[CustomEditor(typeof(MapGenerator))]
[ExecuteAlways]
public class MapGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        MapGenerator mapGen = (MapGenerator)target;
        if (GUILayout.Button("Generate Map", GUILayout.ExpandWidth(false)))
        {
            mapGen.GenerateMap();
            SceneView.RepaintAll();
        }
    }
}
