using UnityEngine;
using System.Collections;
using UnityEngine.Assertions;
using System.Collections.Generic;
using static TileData3D;

public class WaveFunctionCollapse3D : MonoBehaviour
{
    [Header("Wave parameters")]
    public Vector3Int MapSize = new(10, 10, 10);

    [SerializeField] private ModuleCollection3D _modules;
    [SerializeField] private float _tileSize = 2f;
    [SerializeField] private string _emptyTileName = "Empty_i";
    [SerializeField] private string _solidTileName = "Solid_i";
    [SerializeField] private string _simpleFloorTileName = "Grass_i";

    [Header("Generator options")]
    [Tooltip("Enabling this makes the outside faces of the map empty tiles. This gives a cleaner end result.")]
    public bool AddEmptyBorder = true;
    public bool UseTileWeights = true;
    public bool UseMaterialAdjacency = true;

    [Header("Visualization properties")]
    [SerializeField] private float _stepTime = 1f;

    public float StepTime
    {
        set => _stepTime = value;
    }

    private WFCCell3D[,,] _cells3D;
    private GameObject _generatedMap = null;
    [HideInInspector] public bool CurrentlyCollapsing { get; private set; }

    //These parameters are used as info, to see how the algorithm is running
    public int AmountOfCollapsedCells { get; private set; }
    public int AmountOfCellsRemaining { get; private set; }
    public string CurrentStep { get; private set; }

    private void OnValidate()
    {
        Assert.AreNotEqual(null, _modules, "3D WFC: Module Collection not assigned!");

        AmountOfCellsRemaining = MapSize.x * MapSize.y * MapSize.z;
    }

    private void Start()
    {
        _modules.CreateModules();
    }

    /// <summary>
    /// Stop any running co routines on this script and delete a generated map (if any).
    /// </summary>
    public void AttemptDestroyResult()
    {
        StopAllCoroutines();
        if (_generatedMap != null)
            Destroy(_generatedMap);
    }

    /// <summary>
    /// Deletes the previously generated level (if any), stops current co routines and start the generation of a new level.
    /// </summary>
    public void GenerateLevel()
    {
        Debug.Log($"Generating map of size: {MapSize}");
        AttemptDestroyResult();

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
        // Add a border of empty tiles around the generated map, if preferred
        // This makes the outside of the generated map better
        if (AddEmptyBorder)
        {
            GenerateEmptyBorder();
            GenerateFlatBorder();
        }

        CurrentlyCollapsing = true;

        //Get the cell with the lowest entropy to start the algorithm
        var cell = GetLowestEntropyCell();
        if (cell.x == -1 || cell.y == -1 || cell.z == -1)
        {
            Debug.LogWarning("No cell with entropy found, before algorithm start? Stopping algorithm.");
            yield break;
        }

        Debug.Log("Starting the collapsing of the wave");
        //Start collapsing the wave
        do
        {
            //Collapse the cell
            CurrentStep = $"Collapsing cell: {cell}";
            _cells3D[cell.x, cell.y, cell.z].CollapseCell(UseTileWeights);

            //DEBUG INFO
            AmountOfCellsRemaining--;
            AmountOfCollapsedCells++;

            //Propagate the changes to the other cells
            CurrentStep = $"Propagating collapsing of cell: {cell}";
            PropagateChanges(cell);

            //Get the next cell with the lowest entropy
            CurrentStep = "Getting lowest entropy cell";
            cell = GetLowestEntropyCell();

            //Wait the co routine
            if (!Mathf.Approximately(_stepTime, 0))
                yield return new WaitForSeconds(_stepTime);

        } while (cell.x != -1 || cell.y != -1 || cell.z != -1);

        Debug.Log("Outside wave Loop");

        CurrentlyCollapsing = false;

        CleanUp();
    }

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

    private void GenerateEmptyBorder()
    {
        for (var x = 0; x < MapSize.x; x++)
            for (var z = 0; z < MapSize.z; z++)
            {
                var cellIdx = new Vector3Int(x, 0, z);
                var cell = _cells3D[cellIdx.x, cellIdx.y, cellIdx.z];

                if (!cell.IsCollapsed)
                    cell.CollapseCell(_solidTileName);
                PropagateChanges(cellIdx);
            }

        for (var x = 0; x < MapSize.x; x++)
            for (var z = 0; z < MapSize.z; z++)
            {
                var cellIdx = new Vector3Int(x, MapSize.y - 1, z);
                var cell = _cells3D[cellIdx.x, cellIdx.y, cellIdx.z];

                if (!cell.IsCollapsed)
                    cell.CollapseCell(_emptyTileName);

                PropagateChanges(cellIdx);
            }


        AmountOfCellsRemaining -= MapSize.x * MapSize.z * 2;
    }

    private void CleanUp()
    {
        for (var col = 0; col < MapSize.x; col++)
            for (var row = 0; row < MapSize.y; row++)
                for (var layer = 0; layer < MapSize.z; layer++)
                {
                    Destroy(_cells3D[col, row, layer]);
                }
    }

    private Vector3Int GetLowestEntropyCell()
    {
        var minEntropy = float.MaxValue;
        var lowestEntropyCell = new Vector3Int(-1, -1, -1);

        for (var x = 0; x < MapSize.x; x++)
            for (var y = 0; y < MapSize.y; y++)
                for (var z = 0; z < MapSize.z; z++)
                {
                    float newEntropy = _cells3D[x, y, z].GetEntropy();

                    //If the entropy is 0, the cell has been collapsed, so we disregard it
                    if (Mathf.Approximately(newEntropy, 0f))
                        continue;

                    //Add some randomness in case there would be multiple cells with the same entropy
                    newEntropy += Random.Range(0f, 0.1f);

                    if (newEntropy < minEntropy)
                    {
                        minEntropy = newEntropy;
                        lowestEntropyCell = new Vector3Int(x, y, z);
                    }
                }

        return lowestEntropyCell;
    }

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

    private bool CompareCells(Vector3Int currentIdx, Vector3Int neighborIdx)
    {
        var changed = false;
        var horizontalCompare = true;
        var compareDirection = CompareDirection.None;

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
        }


        return changed;
    }

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

    List<Vector3Int> GetNeighbors(Vector3Int cellCoords)
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
