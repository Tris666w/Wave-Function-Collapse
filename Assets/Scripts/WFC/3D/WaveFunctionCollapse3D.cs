using System;
using UnityEngine;
using System.Collections;
using UnityEngine.Assertions;
using System.Collections.Generic;
using UnityEngine.Events;
using static TileData3D;
using Random = UnityEngine.Random;

/// <summary>
/// Generates 3D maps 
/// </summary>
public class WaveFunctionCollapse3D : MonoBehaviour
{
    [Header("Wave parameters")]
    public Vector3Int MapSize = new(10, 10, 10);

    [SerializeField] public ModuleCollection3D _modules;
    [SerializeField] private float _tileSize = 2f;
    [SerializeField] private string _solidTileName = "Solid_i";
    [SerializeField] private string _emptyTileName = "Empty_i";
    [SerializeField] private string _simpleFloorTileName = "Grass_i";

    [Header("Generator options")]
    [Tooltip("Enabling this makes the outside faces of the map empty tiles. This gives a cleaner end result.")]
    public bool GenerateSolidFloor = true;
    public bool UseTileWeights = true;
    public bool UseMaterialAdjacency = true;
    public bool UseExcludedNeighborsAdjacency = true;
    [Header("Visualization properties")]
    [SerializeField] private float _stepTime = 1f;
    public bool UseDebugCube = false;
    [SerializeField] private GameObject _debugCube = null;

    public float StepTime
    {
        set => _stepTime = value;
    }

    private WFCCell3D[,,] _cells3D;
    private GameObject _generatedMap = null;

    //These parameters are used as info, to see how the algorithm is running
    public int AmountOfCollapsedCells { get; private set; }
    public int AmountOfCellsRemaining { get; private set; }
    public string CurrentStep { get; private set; }

    public bool IsRunning { get; private set; }

    private Vector3Int _currentCell = new();


    [Header("Generation events")]

    //These events can be used to add behaviour when generating a map;
    public UnityEvent OnGeneratingStart;
    public UnityEvent OnGeneratingEnd;
    public UnityEvent OnGenerateFailed;



    private void OnValidate()
    {
        Assert.AreNotEqual(null, _modules, "3D WFC: Module Collection not assigned!");
        AmountOfCellsRemaining = MapSize.x * MapSize.y * MapSize.z;
    }

    private void Start()
    {
        _debugCube.SetActive(false);
        _modules.CreateModules();
    }

    /// <summary>
    /// Stop any running co routines on this script and delete a generated map (if any).
    /// </summary>
    public void AttemptDestroyResult()
    {
        IsRunning = false;
        StopAllCoroutines();
        if (_generatedMap != null)
            Destroy(_generatedMap);
    }

    /// <summary>
    /// Deletes the previously generated level (if any), stops current co routines and start the generation of a new level.
    /// </summary>
    public void GenerateLevel()
    {
        AttemptDestroyResult();

        IsRunning = true;

        OnGeneratingStart.Invoke();

        InitializeWave();
        StartCoroutine(CollapseWave());
    }

    /// <summary>
    /// Initialize the wave object with WFCCell3D components and make the result game object.
    /// </summary>
    private void InitializeWave()
    {
        //Create the wave
        _cells3D = new WFCCell3D[MapSize.x, MapSize.y, MapSize.z];

        //Make a game object to hold the generated result and make it a child object of the wave
        var resultObject = new GameObject("Result")
        {
            transform =
            {
                parent = gameObject.transform
            }
        };

        //Save the game object in the generated map field
        _generatedMap = resultObject;

        for (var col = 0; col < MapSize.x; col++)
            for (var row = 0; row < MapSize.y; row++)
                for (var layer = 0; layer < MapSize.z; layer++)
                {
                    //Create the game object that will hold this cell and parent it's transform to the result
                    var go = new GameObject($"x = {col}, y = {row}, z = {layer}")
                    {
                        transform =
                        {
                            parent = resultObject.transform
                        }
                    };

                    //Translate the cell to the correct position
                    var targetPos = transform.position;
                    targetPos.x += col * _tileSize;
                    targetPos.y += row * _tileSize;
                    targetPos.z += layer * _tileSize;
                    go.transform.position = targetPos;


                    //Add the cell class, save the component in the wave and add all the possible modules
                    _cells3D[col, row, layer] = go.AddComponent<WFCCell3D>();
                    _cells3D[col, row, layer].Modules = new List<Module>(_modules.Modules);
                    _cells3D[col, row, layer].RecalculateEntropy();
                    _cells3D[col, row, layer].UseTileWeights = UseTileWeights;
                }


        //DEBUG INFO
        AmountOfCellsRemaining = MapSize.x * MapSize.y * MapSize.z;
    }

    /// <summary>
    /// Collapses the wave and generates the map. This is a co routine!
    /// </summary>
    /// <returns></returns>
    public IEnumerator CollapseWave()
    {
        // Add a border of chosen tiles around the generated map, if preferred
        // This makes the outside of the generated map better
        if (GenerateSolidFloor)
        {
            GenerateAllSolidFloor();
            //GenerateEmptyRoof();
            GenerateFlatBorder();
        }

        //Get the cell with the lowest entropy to start the algorithm
        _currentCell = GetLowestEntropyCell();
        if (_currentCell.x == -1 || _currentCell.y == -1 || _currentCell.z == -1)
        {
            Debug.LogWarning("No cell with entropy found, before algorithm start? Stopping algorithm.");
            yield break;
        }

        if (UseDebugCube)
        {
            _debugCube.SetActive(true);
        }

        //Start collapsing the wave
        do
        {
            //Visualize the current cell with a cube
            if (UseDebugCube)
            {
                var pos = new Vector3(_currentCell.x * _tileSize, _currentCell.y * _tileSize, _currentCell.z * _tileSize);
                _debugCube.transform.position = pos;
            }

            //Collapse the cell
            var result = _cells3D[_currentCell.x, _currentCell.y, _currentCell.z].CollapseCell();
            switch (result.returnState)
            {
                case WfcReturn.WfcReturnState.Warning:
                    Debug.LogWarning(result.returnContext);
                    break;

                case WfcReturn.WfcReturnState.Error:
                    Debug.Log(result.returnContext);
                    IsRunning = false;
                    AttemptDestroyResult();
                    OnGenerateFailed.Invoke();
                    yield break;
            }

            //DEBUG INFO
            AmountOfCellsRemaining--;
            AmountOfCollapsedCells++;

            //Propagate the changes to the other cells
            PropagateChanges(_currentCell);

            //Get the next cell with the lowest entropy
            CurrentStep = $"Current cell {_currentCell}";
            _currentCell = GetLowestEntropyCell();

            //Wait the co routine
            if (!Mathf.Approximately(_stepTime, 0))
                yield return new WaitForSeconds(_stepTime);

        } while (_currentCell.x != -1 || _currentCell.y != -1 || _currentCell.z != -1);

        IsRunning = false;

        _debugCube.SetActive(false);

        CleanUp();

        OnGeneratingEnd.Invoke();
    }

    /// <summary>
    /// Assures that all cells along the x and z border with y == 1 are collapsed to _simpleFloorTileName 
    /// </summary>
    private void GenerateFlatBorder()
    {
        //Along z-faces
        for (var z = 0; z < MapSize.z; z++)
        {
            var cellIdx = new Vector3Int(0, 1, z);
            var cell = _cells3D[cellIdx.x, cellIdx.y, cellIdx.z];

            if (cell.IsCollapsed)
                continue;

            cell.CollapseCell(_simpleFloorTileName);
            PropagateChanges(cellIdx);
        }

        for (var z = 0; z < MapSize.z; z++)
        {
            var cellIdx = new Vector3Int(MapSize.x - 1, 1, z);
            var cell = _cells3D[cellIdx.x, cellIdx.y, cellIdx.z];

            if (cell.IsCollapsed)
                continue;

            cell.CollapseCell(_simpleFloorTileName);
            PropagateChanges(cellIdx);
        }

        //Along x-faces
        for (var x = 0; x < MapSize.z; x++)
        {
            var cellIdx = new Vector3Int(x, 1, 0);
            var cell = _cells3D[cellIdx.x, cellIdx.y, cellIdx.z];

            if (cell.IsCollapsed)
                continue;

            cell.CollapseCell(_simpleFloorTileName);
            PropagateChanges(cellIdx);
        }

        for (var x = 0; x < MapSize.z; x++)
        {
            var cellIdx = new Vector3Int(x, 1, MapSize.z - 1);
            var cell = _cells3D[cellIdx.x, cellIdx.y, cellIdx.z];

            if (cell.IsCollapsed)
                continue;

            cell.CollapseCell(_simpleFloorTileName);
            PropagateChanges(cellIdx);
        }

        //---------
        //DEBUG
        //---------
        AmountOfCellsRemaining -= 2 * MapSize.x + 2 * MapSize.z - 4;
        AmountOfCollapsedCells += 2 * MapSize.x + 2 * MapSize.z;

    }


    /// <summary>
    /// Assures that all cells with y == 0 are solid
    /// </summary>
    private void GenerateAllSolidFloor()
    {
        //Add solid tiles to the floor
        for (var x = 0; x < MapSize.x; x++)
            for (var z = 0; z < MapSize.z; z++)
            {
                var cellIdx = new Vector3Int(x, 0, z);
                var cell = _cells3D[cellIdx.x, cellIdx.y, cellIdx.z];

                if (!cell.IsCollapsed)
                    cell.CollapseCell(_solidTileName);
                PropagateChanges(cellIdx);
            }

        AmountOfCellsRemaining -= MapSize.x * MapSize.z;
    }

    /// <summary>
    /// Assures that all cells with y == MapSize.y - 1 are empty
    /// </summary>
    private void GenerateEmptyRoof()
    {
        //Add empty tiles to the roof
        for (var x = 0; x < MapSize.x; x++)
            for (var z = 0; z < MapSize.z; z++)
            {
                var cellIdx = new Vector3Int(x, MapSize.y - 1, z);
                var cell = _cells3D[cellIdx.x, cellIdx.y, cellIdx.z];

                if (!cell.IsCollapsed)
                    cell.CollapseCell(_emptyTileName);
                PropagateChanges(cellIdx);
            }

        AmountOfCellsRemaining -= MapSize.x * MapSize.z;
    }

    /// <summary>
    /// Remove all used WFCCell3D components
    /// </summary>
    private void CleanUp()
    {
        for (var col = 0; col < MapSize.x; col++)
            for (var row = 0; row < MapSize.y; row++)
                for (var layer = 0; layer < MapSize.z; layer++)
                {
                    Destroy(_cells3D[col, row, layer]);
                }
    }

    /// <summary>
    /// Return the lowest entropy cell from the entire wave. Uses some small randomness in case of multiple cells with the same entropy.
    /// </summary>
    /// <returns>Lowest entropy cell</returns>
    private Vector3Int GetLowestEntropyCell()
    {
        var minEntropy = float.MaxValue;
        var lowestEntropyCell = new Vector3Int(-1, -1, -1);

        for (var x = 0; x < MapSize.x; x++)
            for (var y = 0; y < MapSize.y; y++)
                for (var z = 0; z < MapSize.z; z++)
                {
                    //Disregard collapsed cells
                    if (_cells3D[x, y, z].IsCollapsed)
                        continue;

                    var newEntropy = (float)_cells3D[x, y, z].Entropy;


                    //Add some randomness in case there would be multiple cells with the same entropy
                    newEntropy += Random.Range(0f, 1f);

                    if (newEntropy < minEntropy)
                    {
                        minEntropy = newEntropy;
                        lowestEntropyCell = new Vector3Int(x, y, z);
                    }
                }

        return lowestEntropyCell;
    }

    /// <summary>
    /// Conveys all changes of the collapsed cell to all neighbors and updates the possible states of all neighbors.
    /// </summary>
    /// <param name="originalCell"> The cell that was just collapsed.</param>
    private void PropagateChanges(Vector3Int originalCell)
    {
        Stack<Vector3Int> changedCells = new();
        changedCells.Push(originalCell);

        while (changedCells.Count > 0)
        {
            //Get the current cell
            var currentCell = changedCells.Pop();

            //Get it's neighbors
            var neighborList = GetNeighbors(currentCell);

            foreach (var neighbor in neighborList)
            {
                //Disregard cells that have already been collapsed
                if (_cells3D[neighbor.x, neighbor.y, neighbor.z].IsCollapsed)
                    continue;

                //Detect the compatibilities and checked if something changed
                var somethingChanged = CompareCells(currentCell, neighbor);

                if (somethingChanged)
                {
                    changedCells.Push(neighbor);
                }
            }
        }
    }

    private enum CompareDirection
    {
        PosX,
        NegX,
        PosY,
        NegY,
        PosZ,
        NegZ,

        None
    }

    /// <summary>
    /// This function compares both cells and updates their possible modules.
    /// </summary>
    /// <param name="currentIdx">The current cells</param>
    /// <param name="neighborIdx">The current neighbor</param>
    /// <returns>A boolean representing if the possible modules of the neighbor has changed.</returns>
    private bool CompareCells(Vector3Int currentIdx, Vector3Int neighborIdx)
    {
        var changed = false;
        var horizontalCompare = true;
        var compareDirection = CompareDirection.None;

        //Find the orientation of the cells
        if (neighborIdx.x - currentIdx.x == 1)
        {
            //Pos X 
            compareDirection = CompareDirection.PosX;
        }
        else if (neighborIdx.x - currentIdx.x == -1)
        {
            //Neg X
            compareDirection = CompareDirection.NegX;
        }
        else if (neighborIdx.z - currentIdx.z == 1)
        {
            //Pos Z
            compareDirection = CompareDirection.PosZ;
        }
        else if (neighborIdx.z - currentIdx.z == -1)
        {
            //Neg Z
            compareDirection = CompareDirection.NegZ;
        }
        else if (neighborIdx.y - currentIdx.y == 1)
        {
            //Pos Y
            compareDirection = CompareDirection.PosY;
            horizontalCompare = false;
        }
        else if (neighborIdx.y - currentIdx.y == -1)
        {
            //Neg Y
            compareDirection = CompareDirection.NegY;
            horizontalCompare = false;
        }

        if (compareDirection == CompareDirection.None)
            return false;

        if (horizontalCompare)
        {
            var currentCell = _cells3D[currentIdx.x, currentIdx.y, currentIdx.z];

            var neighborCell = _cells3D[neighborIdx.x, neighborIdx.y, neighborIdx.z];
            var neighborTilesCopy = new List<Module>(neighborCell.Modules);

            if (!currentCell.IsCollapsed)
                neighborTilesCopy = GetDeprecatedNeighborTilesHorizontal(currentCell.Modules, neighborTilesCopy, compareDirection);
            else
            {
                var currentCellList = new List<Module>
                {
                    currentCell.CollapsedData
                };
                neighborTilesCopy = GetDeprecatedNeighborTilesHorizontal(currentCellList, neighborTilesCopy, compareDirection);
            }

            //If there are tiles remaining in tile copy
            //then the propagation changed something in this neighbor
            if (neighborTilesCopy.Count > 0)
                changed = true;

            //All the tiles that remain in the copy are not correct anymore
            //Delete them from the neighbor 
            foreach (var neighborTile in neighborTilesCopy)
            {
                _cells3D[neighborIdx.x, neighborIdx.y, neighborIdx.z].Modules.Remove(neighborTile);
            }

        }
        else
        {
            var currentCell = _cells3D[currentIdx.x, currentIdx.y, currentIdx.z];

            var neighborCell = _cells3D[neighborIdx.x, neighborIdx.y, neighborIdx.z];
            var neighborTilesCopy = new List<Module>(neighborCell.Modules);


            if (!currentCell.IsCollapsed)
                neighborTilesCopy = GetDeprecatedNeighborTilesVertical(currentCell.Modules, neighborTilesCopy, compareDirection);
            else
            {
                var currentCellList = new List<Module>
                {
                    currentCell.CollapsedData
                };
                neighborTilesCopy = GetDeprecatedNeighborTilesVertical(currentCellList, neighborTilesCopy, compareDirection);
            }

            //If there are tiles remaining in tile copy
            //then the propagation changed something in this neighbor
            if (neighborTilesCopy.Count > 0)
                changed = true;

            //All the tiles that remain in the copy are not correct anymore
            //Delete them from the neighbor 
            foreach (var neighborTile in neighborTilesCopy)
            {
                _cells3D[neighborIdx.x, neighborIdx.y, neighborIdx.z].Modules.Remove(neighborTile);
            }

            if (changed)
                _cells3D[neighborIdx.x, neighborIdx.y, neighborIdx.z].RecalculateEntropy();
        }


        return changed;
    }

    /// <summary>
    /// Returns all neighborStates that cannot fit anymore for horizontal faces.
    /// </summary>
    /// <param name="currentCellTiles">The current cells possible modules</param>
    /// <param name="neighborTilesCopy">The current neighbors current modules</param>
    /// <param name="compareDirection">The orientation of both cells</param>
    /// <returns></returns>
    private List<Module> GetDeprecatedNeighborTilesHorizontal(List<Module> currentCellTiles, List<Module> neighborTilesCopy, CompareDirection compareDirection)
    {
        foreach (var module in currentCellTiles)
        {
            List<Module> compatibleTiles = new();

            HorizontalFaceData currentCellFace;
            switch (compareDirection)
            {
                case CompareDirection.PosX:
                    currentCellFace = module._tile._posX;
                    break;
                case CompareDirection.NegX:
                    currentCellFace = module._tile._negX;
                    break;
                case CompareDirection.PosZ:
                    currentCellFace = module._tile._posZ;
                    break;
                case CompareDirection.NegZ:
                    currentCellFace = module._tile._negZ;
                    break;
                default:
                    continue;
            }

            foreach (var neighborModule in neighborTilesCopy)
            {
                HorizontalFaceData neighborCellFace;
                switch (compareDirection)
                {
                    case CompareDirection.PosX:
                        neighborCellFace = neighborModule._tile._negX;
                        break;
                    case CompareDirection.NegX:
                        neighborCellFace = neighborModule._tile._posX;
                        break;
                    case CompareDirection.PosZ:
                        neighborCellFace = neighborModule._tile._negZ;
                        break;
                    case CompareDirection.NegZ:
                        neighborCellFace = neighborModule._tile._posZ;
                        break;
                    default:
                        continue;
                }

                if ((currentCellFace._socketID == 0 && neighborCellFace._socketID == 0) ||
                    (currentCellFace._socketID == 9 && neighborCellFace._socketID == 9))
                {
                    compatibleTiles.Add(neighborModule);
                    continue;
                }

                if (UseExcludedNeighborsAdjacency)
                {

                    //If the current cell face is in the neighbors excluded list or vise versa, ignore it
                    if (currentCellFace.ExcludedNeighbors.Contains(neighborModule._prefab) ||
                       neighborCellFace.ExcludedNeighbors.Contains(module._prefab))
                        continue;
                }

                //Compare the tiles
                //Horizontal tiles match if:
                //  -> The socket id's match
                //
                //  -> Both are symmetrical
                //   OR
                //  -> The sockets are one is flipped and one is normal
                //   OR
                //  -> Both socket id's are 0 (empty faces)

                if (currentCellFace._socketID != neighborCellFace._socketID)
                    continue;

                if ((currentCellFace._isSymmetric && neighborCellFace._isSymmetric) ||
                    (currentCellFace._isFlipped != neighborCellFace._isFlipped))
                {
                    if (!UseMaterialAdjacency)
                    {
                        compatibleTiles.Add(neighborModule);
                        continue;
                    }
                    if (module._tile.CompatibleMaterials.Contains(neighborModule._tile.OwnMaterial) &&
                        neighborModule._tile.CompatibleMaterials.Contains(module._tile.OwnMaterial))
                    {
                        compatibleTiles.Add(neighborModule);
                    }
                }
            }

            //Remove all compatible tiles as to not double check them
            foreach (var compatibleTile in compatibleTiles)
            {
                neighborTilesCopy.Remove(compatibleTile);
            }

        }

        return neighborTilesCopy;
    }

    /// <summary>
    /// Returns all neighborStates that cannot fit anymore for vertical faces.
    /// </summary>
    /// <param name="currentCellTiles">The current cells possible modules</param>
    /// <param name="neighborTilesCopy">The current neighbors current modules</param>
    /// <param name="compareDirection">The orientation of both cells</param>
    /// <returns></returns>
    private List<Module> GetDeprecatedNeighborTilesVertical(List<Module> currentCellTiles, List<Module> neighborTilesCopy, CompareDirection compareDirection)
    {
        foreach (var module in currentCellTiles)
        {
            List<Module> compatibleTiles = new();

            VerticalFaceData currentCellFace;
            switch (compareDirection)
            {
                case CompareDirection.PosY:
                    currentCellFace = module._tile._posY;
                    break;
                case CompareDirection.NegY:
                    currentCellFace = module._tile._negY;
                    break;

                default:
                    continue;
            }

            foreach (var neighborTile in neighborTilesCopy)
            {
                VerticalFaceData neighborCellFace;
                switch (compareDirection)
                {
                    case CompareDirection.PosY:
                        neighborCellFace = neighborTile._tile._negY;
                        break;
                    case CompareDirection.NegY:
                        neighborCellFace = neighborTile._tile._posY;
                        break;
                    default:
                        continue;
                }

                //Compare the tiles
                //Vertical tiles match if:
                //  -> The socket id's match
                //
                //  -> Both are rotationally invariant
                //   OR
                //  -> Both have the same rotation index

                if (currentCellFace._socketID != neighborCellFace._socketID)
                    continue;
                if ((currentCellFace._isInvariant && neighborCellFace._isInvariant) ||
                    (currentCellFace._rotationIndex == neighborCellFace._rotationIndex))
                {
                    compatibleTiles.Add(neighborTile);
                }
            }

            //Remove all compatible tiles as to not double check them
            foreach (var compatibleTile in compatibleTiles)
            {
                neighborTilesCopy.Remove(compatibleTile);
            }
        }

        return neighborTilesCopy;
    }

    /// <summary>
    /// This functions get's all the cell coordinates of all neighbors to the given cell.
    /// </summary>
    /// <param name="cellCoords"> The cell of which te neighbors are wanted</param>
    /// <returns>A list of the cell coordinates of the neighbors </returns>
    private List<Vector3Int> GetNeighbors(Vector3Int cellCoords)
    {
        List<Vector3Int> neighbors = new();


        if (cellCoords.x - 1 >= 0)
            neighbors.Add(new Vector3Int(cellCoords.x - 1, cellCoords.y, cellCoords.z));
        if (cellCoords.x + 1 < MapSize.x)
            neighbors.Add(new Vector3Int(cellCoords.x + 1, cellCoords.y, cellCoords.z));
        if (cellCoords.y - 1 >= 0)
            neighbors.Add(new Vector3Int(cellCoords.x, cellCoords.y - 1, cellCoords.z));
        if (cellCoords.y + 1 < MapSize.y)
            neighbors.Add(new Vector3Int(cellCoords.x, cellCoords.y + 1, cellCoords.z));
        if (cellCoords.z - 1 >= 0)
            neighbors.Add(new Vector3Int(cellCoords.x, cellCoords.y, cellCoords.z - 1));
        if (cellCoords.z + 1 < MapSize.z)
            neighbors.Add(new Vector3Int(cellCoords.x, cellCoords.y, cellCoords.z + 1));
        return neighbors;
    }

}
