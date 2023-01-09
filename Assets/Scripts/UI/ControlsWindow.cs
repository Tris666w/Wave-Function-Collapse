using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlsWindow : MonoBehaviour
{
    [SerializeField] private float _windowWidth = 100f;
    [SerializeField] private Vector2Int _windowOffset = new Vector2Int(10, 10);

    private void OnGUI()
    {
        var targetRect = new Rect(Screen.width - _windowWidth - _windowOffset.x, _windowOffset.y, _windowWidth, Screen.height / 2 - 2 * _windowOffset.y);
        GUI.Box(targetRect, "");

        GUILayout.BeginArea(targetRect);
        GUILayout.BeginVertical();
        GUILayout.Label("Move left: a");
        GUILayout.Label("Move right: d");
        GUILayout.Label("Move forwards: w");
        GUILayout.Label("Move backwards: s");
        GUILayout.Label("Move up: q");
        GUILayout.Label("Move down: e");
        GUILayout.Label("Move faster: shift");
        GUILayout.Label("Hold RMB to rotate the camera");

        GUILayout.EndVertical();
        GUILayout.EndArea();
    }
}
