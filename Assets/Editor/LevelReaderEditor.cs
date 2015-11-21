using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(LevelReader))]
public class LevelReaderEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        LevelReader myScript = (LevelReader)target;
        if (GUILayout.Button("Read Level"))
        {
            myScript.ReadLevel();
        }
        if (GUILayout.Button("Create Level Texture"))
        {
            myScript.CreateLevelTexture();
        }
        if (GUILayout.Button("Compute Jump Points"))
        {
            myScript.ComputeJumpPoints();
        }
        if (GUILayout.Button("Compute Goal Bounds"))
        {
            myScript.ComputeGoalBounds();
        }
    }
}