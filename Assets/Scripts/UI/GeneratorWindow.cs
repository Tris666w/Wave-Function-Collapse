using UnityEngine;
using UnityEngine.Assertions;

public class GeneratorWindow : MonoBehaviour
{
    [Header("Generator")]
    public WaveFunctionCollapse2D _2DGenerator = null;
    [Space(2)]
    [SerializeField] private WaveFunctionCollapse3D _3DGenerator = null;
    [Space(10)]
    [Header("Window parameters")]
    [SerializeField] private Vector2Int _windowOffset = new(10, 10);

    private float _stepTime = 0.15f;

    private void OnValidate()
    {
        Assert.AreNotEqual(null, _2DGenerator, "GeneratorWindow: no 2D generator linked!");
        Assert.AreNotEqual(null, _2DGenerator, "GeneratorWindow: no 3D generator linked!");
    }

    private void OnGUI()
    {
        var windowDimensionPercentage = new Vector2(0.25f, 1f);

        var targetRect = new Rect(_windowOffset.x, _windowOffset.y, windowDimensionPercentage.x * Screen.width, windowDimensionPercentage.y * Screen.height - 2 * _windowOffset.y);
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

        _2DGenerator.UseTileWeight = GUILayout.Toggle(_2DGenerator.UseTileWeight, "Use tile weights");

        GUILayout.BeginHorizontal();

        var sizeX2D = _2DGenerator.MapSize.x.ToString();
        var sizeY2D = _2DGenerator.MapSize.y.ToString();
        sizeX2D = GUILayout.TextField(sizeX2D);
        sizeY2D = GUILayout.TextField(sizeY2D);

        if (sizeX2D.Length > 0 && sizeY2D.Length > 0)
            _2DGenerator.MapSize = new Vector2Int(int.Parse(sizeX2D), int.Parse(sizeY2D));

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
        _3DGenerator.UseTileWeights = GUILayout.Toggle(_3DGenerator.UseTileWeights, "Use tile weights");

        if (_3DGenerator.IsRunning)
        {
            //-------------
            //DEBUG AREA
            //-------------
            GUILayout.Label($"Currently on: {_3DGenerator.CurrentStep}");
            GUILayout.Label($"Amount of cells collapsed: {_3DGenerator.AmountOfCollapsedCells}");
            GUILayout.Label($"Amount of open cells remaining: {_3DGenerator.AmountOfCellsRemaining}");

        }

        GUILayout.BeginHorizontal();

        var sizeX3D = _3DGenerator.MapSize.x.ToString();
        var sizeY3D = _3DGenerator.MapSize.y.ToString();
        var sizeZ3D = _3DGenerator.MapSize.z.ToString();

        sizeX3D = GUILayout.TextField(sizeX3D);
        sizeY3D = GUILayout.TextField(sizeY3D);
        sizeZ3D = GUILayout.TextField(sizeZ3D);

        if (sizeX3D.Length > 0 && sizeY3D.Length > 0 && sizeZ3D.Length > 0)
            _3DGenerator.MapSize = new Vector3Int(int.Parse(sizeX3D), int.Parse(sizeY3D), int.Parse(sizeZ3D));

        GUILayout.EndHorizontal();

        GUILayout.Space(10);
        GUILayout.Label("Shared options", titleStyle);
        GUILayout.Label("StepTime");
        _stepTime = GUILayout.HorizontalSlider(_stepTime, 0f, 1f);
        _3DGenerator.StepTime = _stepTime;
        _2DGenerator.StepTime = _stepTime;





        GUILayout.EndVertical();
        GUILayout.EndArea();

    }

}
