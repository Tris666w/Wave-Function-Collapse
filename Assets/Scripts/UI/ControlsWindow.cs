using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlsWindow : MonoBehaviour
{
    [SerializeField] private float _windowWidth = 200f;
    [SerializeField] private Vector2Int _windowOffset = new Vector2Int(10, 10);
    [SerializeField][Range(0f, 1f)] private float _windowHeightPercentage = 0.45f;

    private void OnGUI()
    {
        var textSyle = new GUIStyle()
        {
            alignment = TextAnchor.MiddleCenter,
            normal =
            {
                textColor = Color.white
            },
            fontSize = 18,
            wordWrap = true
        };

        var targetRect = new Rect(_windowOffset.x, _windowOffset.y + Screen.height * (1 - _windowHeightPercentage), _windowWidth, Screen.height * _windowHeightPercentage - 2 * _windowOffset.y);
        GUI.Box(targetRect, "");

        GUILayout.BeginArea(targetRect);
        GUILayout.BeginVertical();
        GUILayout.Space(15);
        GUILayout.Label("Move left: a", textSyle);
        GUILayout.Label("Move right: d", textSyle);
        GUILayout.Space(10);
        GUILayout.Label("Move forwards: w", textSyle);
        GUILayout.Label("Move backwards: s", textSyle);
        GUILayout.Space(10);
        GUILayout.Label("Move up: q", textSyle);
        GUILayout.Label("Move down: e", textSyle);
        GUILayout.Space(10);
        GUILayout.Label("Move faster: shift", textSyle);
        GUILayout.Space(10);
        GUILayout.Label("Hold RMB to rotate the camera", textSyle);

        GUILayout.EndVertical();
        GUILayout.EndArea();
    }
}
