using UnityEngine;
using UnityEngine.Assertions;

public class GeneratorWindow : MonoBehaviour
{
    [SerializeField] private WaveFunctionCollapse2D _2DGenerator = null;
    [SerializeField] private WaveFunctionCollapse3D _3DGenerator = null;
    [SerializeField] private Vector2Int _windowOffset = new Vector2Int(10, 10);
    [SerializeField][Range(0f, 1f)] private float _windowHeightPercentage = 0.55f;

    private float _stepTime = 0.25f;
    private string _2DSizeX = "10";
    private string _2DSizeY = "10";

    private string _3DSizeX = "10";
    private string _3DSizeY = "10";
    private string _3DSizeZ = "10";

    private void OnValidate()
    {
        Assert.AreNotEqual(null, _2DGenerator, "GeneratorWindow: no 2D generator linked!");
        Assert.AreNotEqual(null, _2DGenerator, "GeneratorWindow: no 3D generator linked!");
    }

    private void OnGUI()
    {
        var targetRect = new Rect(_windowOffset.x, _windowOffset.y, 200, _windowHeightPercentage * Screen.height - 2 * _windowOffset.y);
        var titleStyle = new GUIStyle
        {
            alignment = TextAnchor.MiddleCenter,
            fontStyle = FontStyle.Bold,
            normal =
            {
                textColor = Color.white
            }
        };

        GUI.Box(targetRect, "");

        GUILayout.BeginArea(targetRect);
        GUILayout.BeginVertical();
        GUILayout.Space(15);

        GUILayout.Label("2D Wave Function Collapse", titleStyle);

        if (GUILayout.Button("Generate 2D level"))
        {
            _2DGenerator.GenerateLevel();
        }
        if (GUILayout.Button("Destroy 2D level"))
        {
            _2DGenerator.AttemptDestroyResult();
        }
        GUILayout.BeginHorizontal();

        _2DSizeX = GUILayout.TextField(_2DSizeX);
        _2DSizeY = GUILayout.TextField(_2DSizeY);

        if (_2DSizeX.Length > 0 && _2DSizeY.Length > 0)
            _2DGenerator.MapSize = new Vector2Int(int.Parse(_2DSizeX), int.Parse(_2DSizeY));

        GUILayout.EndHorizontal();


        GUILayout.Space(10);

        GUILayout.Label("3D Wave Function Collapse", titleStyle);
        if (GUILayout.Button("Generate 3D level"))
        {
            _3DGenerator.GenerateLevel();
        }
        if (GUILayout.Button("Destroy 3D level"))
        {
            _3DGenerator.AttemptDestroyResult();
        }
        _3DGenerator.AddEmptyBorder = GUILayout.Toggle(_3DGenerator.AddEmptyBorder, "Add empty border");


        GUILayout.BeginHorizontal();

        _3DSizeX = GUILayout.TextField(_3DSizeX);
        _3DSizeY = GUILayout.TextField(_3DSizeY);
        _3DSizeZ = GUILayout.TextField(_3DSizeZ);

        if (_3DSizeX.Length > 0 && _3DSizeY.Length > 0 && _3DSizeZ.Length > 0)
            _3DGenerator.MapSize = new Vector3Int(int.Parse(_3DSizeX), int.Parse(_3DSizeY), int.Parse(_3DSizeZ));

        GUILayout.EndHorizontal();

        GUILayout.Space(10);
        GUILayout.Label("StepTime", titleStyle);
        _stepTime = GUILayout.HorizontalSlider(_stepTime, 0f, 1f);
        _3DGenerator.StepTime = _stepTime;
        _2DGenerator.StepTime = _stepTime;
        GUILayout.Label($"Current:{_stepTime}");


        GUILayout.EndVertical();
        GUILayout.EndArea();

    }

}
