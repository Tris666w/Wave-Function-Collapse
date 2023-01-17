using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using Debug = UnityEngine.Debug;

public class PerformanceTest : MonoBehaviour
{
    [SerializeField] private WaveFunctionCollapse3D _wfc = null;


    [HideInInspector] public bool CurrentlyTesting = false;
    [HideInInspector] public bool TestRunning = false;
    private bool _waitForNextCase = false;
    private Stopwatch _testStopwatch = new();

    public int AmountOfTests = 10;
    [HideInInspector] public int CurrentTestIteration = 0;

    private List<long> _testTimes = new();

    public float TimeBetweenTestIterations = 5.0f;
    private float _testTimer = 0f;

    public float TimeBetweenTestCases = 5.0f;
    private float _caseTimer = 0f;

    [SerializeField] private string _fileName = "WFC_Test";
    [SerializeField] private string _targetPath = "PerformanceTests/";

    [SerializeField] private List<WFC_3D_Test_Settings> _testCases = new();
    [HideInInspector] public WFC_3D_Test_Settings RunningCase;
    [HideInInspector] public int CurrentTestCase = 0;
    public int AmountOfIterations
    {
        get => AmountOfTests;
    }

    public int AmountOfCases
    {
        get => _testCases.Count;
    }


    private StreamWriter _writer;


    private void Start()
    {
#if UNITY_EDITOR
        Assert.AreNotEqual(_wfc, null, "PerformanceTest: WaveFunctionCollapse3D not assigned!");
#endif
    }

    private void Update()
    {
        if (CurrentlyTesting == false)
            return;

        if (_waitForNextCase)
        {
            _caseTimer += Time.deltaTime;
            if (_caseTimer >= TimeBetweenTestCases)
            {
                _caseTimer = 0;
                StartNextTestCase();
            }

            return;
        }

        if (TestRunning)
            return;

        _testTimer += Time.deltaTime;
        if (_testTimer >= TimeBetweenTestIterations)
        {
            _testTimer = 0;
            StartNextTest();
        }

    }

    public void StartTesting()
    {
        if (_testCases.Count <= 0)
            return;

        _wfc.OnGeneratingEnd.AddListener(EndCurrentTest);


        //Write the data to a .csv file
        var path = $"{Application.persistentDataPath}";
        path += _targetPath + _fileName;
        var time = "_" + System.DateTime.Now.Hour + "_" +
                   System.DateTime.Now.Minute + "_" +
                   System.DateTime.Now.Second + "_";

        var fi = new FileInfo(path);
        if (!fi.Directory.Exists)
        {
            System.IO.Directory.CreateDirectory(fi.DirectoryName);
        }
        _writer = new StreamWriter(path + time + ".csv");
        _writer.WriteLine("sep=;");
        _writer.WriteLine("Wave function collapse performance test");
        _writer.WriteLine();
        _writer.WriteLine();

        CurrentlyTesting = true;
        _testTimes.Clear();

        Debug.Log("Start performance testing");

        StartNextTestCase();
    }

    private void StartNextTestCase()
    {
        RunningCase = _testCases[CurrentTestCase];
        _waitForNextCase = false;
        CurrentTestIteration = 0;

        //Set correct test case settings
        _wfc.StepTime = 0;
        _wfc.MapSize = _testCases[CurrentTestCase].TestMapSize;
        _wfc.UseMaterialAdjacency = _testCases[CurrentTestCase].UseMaterialAdjacency;
        _wfc.UseTileWeights = _testCases[CurrentTestCase].UseTileWeights;
        _wfc.UseExcludedNeighborsAdjacency = _testCases[CurrentTestCase].UseExcludedNeighborsAdjacency;
        _wfc.GenerateSolidFloor = _testCases[CurrentTestCase].GenerateSolidFloor;

        //Start the algorithm and stopwatch
        _testStopwatch.Restart();
        _wfc.GenerateLevel();
    }

    private void StartNextTest()
    {
        TestRunning = true;

        // Destroy any previously generated result
        // otherwise extra time might be added
        _wfc.AttemptDestroyResult();

        _testStopwatch.Restart();
        _wfc.StepTime = 0;
        _wfc.GenerateLevel();
    }

    public void EndCurrentTest()
    {
        _testStopwatch.Stop();
        _testTimes.Add(_testStopwatch.ElapsedMilliseconds);

        CurrentTestIteration++;
        //Have all the test for the current case been done?
        if (CurrentTestIteration < AmountOfTests)
        {
            //No, go to next test
            TestRunning = false;
            return;
        }

        WriteCurrentCase();
        CurrentTestCase++;
        _testTimes.Clear();

        //Is this the final test case?
        if (CurrentTestCase < _testCases.Count)
        {
            //No, go to the next case
            _waitForNextCase = true;
            CurrentTestIteration = 0;
        }
        else
        {
            EndTests();
        }
    }

    private void EndTests()
    {
        //End the test
        CurrentlyTesting = false;
        _writer.Close();
        _wfc.AttemptDestroyResult();

    }

    private void WriteCurrentCase()
    {
        _writer.WriteLine($"Case {CurrentTestCase + 1}:; {_testCases[CurrentTestCase].CaseName}");

        WriteAlgorithmContext(_writer);
        WriteTimes(_writer);

        _writer.WriteLine();
        _writer.WriteLine();

    }

    private void WriteAlgorithmContext(StreamWriter writer)
    {
        writer.WriteLine("Map size (# cells); Solid Floor Assurance; Tile Weights; Material Adjacency; Neighbor exclusion");
        writer.WriteLine($"{_wfc.MapSize.x * _wfc.MapSize.y * _wfc.MapSize.z}; {_wfc.GenerateSolidFloor}; {_wfc.UseTileWeights}; {_wfc.UseMaterialAdjacency}; {_wfc.UseExcludedNeighborsAdjacency}");
    }

    private void WriteTimes(StreamWriter writer)
    {
        //Disregard the lowest and the highest value!!
        var min = _testTimes.Min();
        var max = _testTimes.Max();
        _testTimes.Remove(min);
        _testTimes.Remove(max);


        writer.WriteLine("Test times (ms):");

        for (var i = 0; i < _testTimes.Count; i++)
        {
            _writer.Write($"Test {i + 1};");
        }

        writer.WriteLine();

        foreach (var testTime in _testTimes)
        {
            var line = testTime + ";";
            writer.Write(line);
        }
    }

}
