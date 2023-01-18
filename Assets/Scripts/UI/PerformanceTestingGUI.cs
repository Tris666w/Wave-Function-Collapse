using UnityEngine;

public class PerformanceTestingGUI : MonoBehaviour
{
    [SerializeField] private PerformanceTest _performanceTest = null;
    [SerializeField] private float _windowWidth = 200f;
    [SerializeField] private Vector2Int _windowOffset = new(10, 10);
    [SerializeField][Range(0f, 1f)] private float _windowHeightPercentage = 0.45f;


    private void OnGUI()
    {
        var titleStyle = new GUIStyle
        {
            alignment = TextAnchor.MiddleCenter,
            fontStyle = FontStyle.Bold,
            normal =
            {
                textColor = Color.white
            }
        };

        Rect targetRect;
        if (_performanceTest.CurrentlyTesting)
        {
            targetRect = new Rect(_windowOffset.x, _windowOffset.y,
                _windowWidth, Screen.height * _windowHeightPercentage - 2 * _windowOffset.y);
        }
        else
        {
            targetRect = new Rect(_windowOffset.x, _windowOffset.y, _windowWidth, Screen.height / 15);
        }



        GUI.Box(targetRect, "");

        GUILayout.BeginArea(targetRect);
        GUILayout.Space(10);
        GUILayout.Label("Performance testing", titleStyle);

        if (GUILayout.Button("Start performanceTest") == true)
        {
            _performanceTest.StartTesting();
        }

        GUILayout.Space(10);
        GUILayout.Label("Test info:", titleStyle);
        GUILayout.Label($"Currently testing: {_performanceTest.CurrentlyTesting}");
        GUILayout.Label($"Current test case: {_performanceTest.CurrentTestCase + 1} / {_performanceTest.AmountOfCases}");
        GUILayout.Label($"Current test iteration: {_performanceTest.CurrentTestIteration + 1} / {_performanceTest.AmountOfIterations}");

        GUILayout.Space(10);
        GUILayout.Label("Current case info", titleStyle);
        GUILayout.Label($"Case name: {_performanceTest.RunningCase.CaseName}");
        GUILayout.Label($"Amount of failed iterations for this case: {_performanceTest.CurrentCaseErrorCount}");
        GUILayout.Label($"Case size: {_performanceTest.RunningCase.TestMapSize}");
        GUILayout.Label($"Assures solid floor: {_performanceTest.RunningCase.GenerateSolidFloor}");
        GUILayout.Label($"Uses tile weights: {_performanceTest.RunningCase.UseTileWeights}");
        GUILayout.Label($"Uses material adjacency: {_performanceTest.RunningCase.UseMaterialAdjacency}");
        GUILayout.Label($"Uses neighbor exclusion: {_performanceTest.RunningCase.UseExcludedNeighborsAdjacency}");


        GUILayout.EndArea();

    }
}
