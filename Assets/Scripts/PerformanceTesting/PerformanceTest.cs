using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Assertions;
using Debug = UnityEngine.Debug;

public class PerformanceTest : MonoBehaviour
{
    private bool CurrentlyTesting = false;
    private bool _testRunning = false;

    private Stopwatch _testStopwatch = new();
    [SerializeField] private WaveFunctionCollapse3D _wfc = null;

    public int AmountOfTests = 10;
    private int _currentTest = 0;

    private List<long> _testTimes = new();

    public float TimeBetweenTestIterations = 5.0f;
    private float _testTimer = 0f;

    private void OnValidate()
    {
        Assert.AreNotEqual(_wfc, null, "PerformanceTest: WaveFunctionCollapse3D not assigned!");
    }

    private void Start()
    {
        _wfc.OnGeneratingEnd.AddListener(EndCurrentTest);
    }

    private void Update()
    {
        if (CurrentlyTesting == false)
            return;

        if (_testRunning)
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
        CurrentlyTesting = true;
        _currentTest = 0;
        _testTimes.Clear();
        Debug.Log("Start performance testing");

        StartNextTest();
    }

    private void StartNextTest()
    {
        Debug.Log($"Running iteration {_currentTest}");
        _testRunning = true;

        _testStopwatch.Restart();
        _wfc.StepTime = 0;
        _wfc.GenerateLevel();
    }

    public void EndCurrentTest()
    {
        _testStopwatch.Stop();
        _testTimes.Add(_testStopwatch.ElapsedMilliseconds);

        _currentTest++;
        if (_currentTest < AmountOfTests)
        {
            _testRunning = false;
        }
        else
        {
            EndTests();
        }
    }

    private void EndTests()
    {
        CurrentlyTesting = false;
        Debug.Log($"End performance testing with times: {_testTimes.ToArray()}");
    }
}
