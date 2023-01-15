using UnityEngine;

public class PerformanceTestingGUI : MonoBehaviour
{
    [SerializeField] private PerformanceTest _performanceTest = null;
    [SerializeField] private float _windowWidth = 200f;
    [SerializeField] private Vector2Int _windowOffset = new(10, 10);
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
            wordWrap = true,
            margin = new RectOffset(10, 0, 0, 0)
        };

        var titleStyle = new GUIStyle
        {
            alignment = TextAnchor.MiddleCenter,
            fontStyle = FontStyle.Bold,
            normal =
            {
                textColor = Color.white
            }
        };

        var targetRect = new Rect(Screen.width - _windowOffset.x - _windowWidth, _windowOffset.y,
            _windowWidth, Screen.height * _windowHeightPercentage - 2 * _windowOffset.y);

        GUI.Box(targetRect, "");

        GUILayout.BeginArea(targetRect);
        GUILayout.Space(10);
        GUILayout.Label("Performance testing", titleStyle);

        if (GUILayout.Button("Start performanceTest") == true)
        {
            _performanceTest.StartTesting();
        }

        GUILayout.EndArea();

    }
}
