using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.TerrainTools;

[CustomEditor(typeof(ModuleCollection3D))]
public class ModuleCollectionCustomEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        ModuleCollection3D script = (ModuleCollection3D)target;

        EditorGUILayout.HelpBox("Create a list of modules from the given tilePrefab.\n Add a tile prefab before generating modules.", MessageType.Info);
        if (GUILayout.Button("Generate Modules"))
        {
            script.CreateModules();
        }

        EditorGUILayout.HelpBox("Remove all modules", MessageType.Info);
        if (GUILayout.Button("Clear Modules"))
        {
            script.ResetModules();
        }

    }
}
