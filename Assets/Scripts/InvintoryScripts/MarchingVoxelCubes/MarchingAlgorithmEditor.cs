using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

[CustomEditor(typeof(MarchingAlgorithm))]
public class MarchingAlgorithmEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        MarchingAlgorithm script = (MarchingAlgorithm)target;

        if(GUILayout.Button("Generate Island"))
        {
            script.GenerateIsland();
        }
    }
}
