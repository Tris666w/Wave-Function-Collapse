using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using static TileData;

public class WFC_3D : MonoBehaviour
{
    [SerializeField] private Vector3Int _mapSize = new(10, 10, 10);
    [SerializeField] private float _tileSize = 2f;
    [SerializeField] private List<TileData> _tiles = new();

    [SerializeField] private float _stepTime = 1f;

    private WFCCell3D[,,] _cells3D;
    private GameObject _generatedMap = null;


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.J))
        {
            ClearLog();
            Debug.Log($"Generating map of size: {_mapSize.ToString()}");
            AttemptDestroyResult();
            StopAllCoroutines();
            GenerateLevel();
        }
        if (Input.GetKeyDown(KeyCode.K))
        {
            Debug.Log("Destroying generated level");
            AttemptDestroyResult();
            StopAllCoroutines();
        }
    }

    public void ClearLog()
    {
        var assembly = Assembly.GetAssembly(typeof(UnityEditor.Editor));
        var type = assembly.GetType("UnityEditor.LogEntries");
        var method = type.GetMethod("Clear");
        method.Invoke(new object(), null);
    }

    private void AttemptDestroyResult()
    {
        if (_generatedMap != null)
            Destroy(_generatedMap);
    }

    [ContextMenu("Click me to generate a map!")]
    public void GenerateLevel()
    {
        InitializeWave();
        StartCoroutine(CollapseWave());
    }

    private void InitializeWave()
    {
        _cells3D = new WFCCell3D[_mapSize.x, _mapSize.y, _mapSize.z];
        var originalPos = transform.position;
        var resultObject = new GameObject("Result");
        resultObject.transform.parent = gameObject.transform;
        _generatedMap = resultObject;

        for (int col = 0; col < _mapSize.x; col++)
            for (int row = 0; row < _mapSize.y; row++)
                for (int layer = 0; layer < _mapSize.z; layer++)
                {
                    var go = new GameObject($"x = {col}, y = {row}, z = {layer}");
                    go.transform.parent = resultObject.transform;

                    var targetPos = originalPos;
                    targetPos.x += col * _tileSize;
                    targetPos.y += row * _tileSize;
                    targetPos.z += layer * _tileSize;
                    go.transform.position = targetPos;

                    _cells3D[col, row, layer] = go.AddComponent<WFCCell3D>();
                    go.AddComponent<MeshRenderer>();
                    go.AddComponent<MeshFilter>();

                    _cells3D[col, row, layer]._possibleTiles = new List<TileData>(_tiles);
                }
    }

    public IEnumerator CollapseWave()
    {
        Vector3Int cell = new(-1, -1, -1);

        //Get the cell with the lowest entropy to start the algorithm
        cell = GetLowestEntropyCell();
        if (cell.x == -1 || cell.y == -1 || cell.z == -1)
        {
            Debug.LogError("No cell with entropy found, before algorithm start?");
            yield break;
        }

        //Start collapsing the wave
        do
        {
            //Collapse the cell
            _cells3D[cell.x, cell.y, cell.z].CollapseCell();

            //Propogate the changes to the other cells
            PropogateChanges(cell);

            //Get the next cell with the lowest entropy
            cell = GetLowestEntropyCell();

            //Wait the coroutine
            yield return new WaitForSeconds(_stepTime);

            //If there is no cell with a low entropy that is not equal to 0,
            // The wave has collapsed, break the loop
        } while (cell.x != -1 || cell.y != -1);

        //CleanUp();
    }

    private void CleanUp()
    {
        for (int col = 0; col < _mapSize.x; col++)
            for (int row = 0; row < _mapSize.y; row++)
                for (int layer = 0; layer < _mapSize.z; layer++)
                {
                    DestroyImmediate(_cells3D[col, row, layer]);
                }
    }

    private Vector3Int GetLowestEntropyCell()
    {
        float _minEntropy = float.MaxValue;
        var lowestEntropyCell = new Vector3Int(-1, -1, -1);

        for (int x = 0; x < _mapSize.x; x++)
            for (int y = 0; y < _mapSize.y; y++)
                for (int z = 0; z < _mapSize.z; z++)
                {
                    float newEntropy = _cells3D[x, y, z].GetEntropy();

                    //If the entropy is 0, the cell has been collapsed, so we disregard it
                    if (Mathf.Approximately(newEntropy, 0f))
                        continue;

                    //Add some randomness in case there would be multiple cells with the same entropy
                    newEntropy += Random.Range(-0.1f, 0.1f);

                    if (newEntropy < _minEntropy)
                    {
                        _minEntropy = newEntropy;
                        lowestEntropyCell = new Vector3Int(x, y, z);
                    }
                }

        return lowestEntropyCell;
    }

    private void PropogateChanges(Vector3Int originalCell)
    {
        Stack<Vector3Int> changedCells = new();
        changedCells.Push(originalCell);

        while (changedCells.Count > 0)
        {
            //Get the current cell
            var currentCell = changedCells.Pop();

            //Get it's neighbours
            var neighbourList = GetNeighbours(currentCell);

            foreach (var neighbour in neighbourList)
            {
                //Disregard cells that have already been collapsed
                if (_cells3D[neighbour.x, neighbour.y, neighbour.z].IsCollapsed)
                    continue;

                //Detect the compatibilities and checked if something changed
                var somethingChanged = CompareCells(currentCell, neighbour);

                if (somethingChanged)
                {
                    changedCells.Push(neighbour);
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

    bool CompareCells(Vector3Int currentIdx, Vector3Int neighbourIdx)
    {
        bool changed = false;
        bool horizontalCompare = true;
        CompareDirection compareDirection = CompareDirection.None;

        if (neighbourIdx.x - currentIdx.x == 1)
        {
            //Pos X 
            compareDirection = CompareDirection.PosX;
        }
        else if (neighbourIdx.x - currentIdx.x == -1)
        {
            //Neg X
            compareDirection = CompareDirection.NegX;
        }
        else if (neighbourIdx.z - currentIdx.z == 1)
        {
            //Pos Z
            compareDirection = CompareDirection.PosZ;
        }
        else if (neighbourIdx.z - currentIdx.z == -1)
        {
            //Neg Z
            compareDirection = CompareDirection.NegZ;
        }
        else if (neighbourIdx.y - currentIdx.y == 1)
        {
            //Pos Y
            compareDirection = CompareDirection.PosY;
            horizontalCompare = false;
        }
        else if (neighbourIdx.y - currentIdx.y == -1)
        {
            //Neg Y
            compareDirection = CompareDirection.NegY;
            horizontalCompare = false;
        }

        if (compareDirection == CompareDirection.None)
            return false;

        if (horizontalCompare)
        {
            WFCCell3D currentCell = _cells3D[currentIdx.x, currentIdx.y, currentIdx.z];

            WFCCell3D neighbourCell = _cells3D[neighbourIdx.x, neighbourIdx.y, neighbourIdx.z];
            var neighbourTilesCopy = new List<TileData>(neighbourCell._possibleTiles);

            if (!currentCell.IsCollapsed)
                neighbourTilesCopy = GetDeprecatedNeighbourTilesHorizontal(currentCell._possibleTiles, neighbourTilesCopy, compareDirection);
            else
            {
                var currentCellList = new List<TileData>
                {
                    currentCell.CollapsedData
                };
                neighbourTilesCopy = GetDeprecatedNeighbourTilesHorizontal(currentCellList, neighbourTilesCopy, compareDirection);
            }

            //If there are tiles remaining in tilecopy
            //then the propogation changed something in this neighbour
            if (neighbourTilesCopy.Count > 0)
                changed = true;

            //All the tiles that remain in the copy are not correct anymore
            //Delete them from the neighbour 
            foreach (var neighbourTile in neighbourTilesCopy)
            {
                _cells3D[neighbourIdx.x, neighbourIdx.y, neighbourIdx.z]._possibleTiles.Remove(neighbourTile);
            }

        }
        else
        {
            WFCCell3D currentCell = _cells3D[currentIdx.x, currentIdx.y, currentIdx.z];

            WFCCell3D neighbourCell = _cells3D[neighbourIdx.x, neighbourIdx.y, neighbourIdx.z];
            var neighbourTilesCopy = new List<TileData>(neighbourCell._possibleTiles);


            foreach (var tile in currentCell._possibleTiles)
            {
                List<TileData> compatibleTiles = new();

                VerticalFaceData currentCellFace;
                switch (compareDirection)
                {
                    case CompareDirection.PosY:
                        currentCellFace = tile._posY;
                        break;
                    case CompareDirection.NegY:
                        currentCellFace = tile._negY;
                        break;

                    default:
                        continue;
                }

                foreach (var neighbourTile in neighbourTilesCopy)
                {
                    VerticalFaceData neighbourCellFace;
                    switch (compareDirection)
                    {
                        case CompareDirection.PosY:
                            neighbourCellFace = neighbourTile._negY;
                            break;
                        case CompareDirection.NegY:
                            neighbourCellFace = neighbourTile._posY;
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

                    if (currentCellFace._socketID != neighbourCellFace._socketID)
                        continue;
                    if ((currentCellFace._isInvariant && neighbourCellFace._isInvariant) ||
                        (currentCellFace._rotationIndex == neighbourCellFace._rotationIndex))
                    {
                        compatibleTiles.Add(neighbourTile);
                    }
                }

                //Remove all compatible tiles as to not double check them
                foreach (var compatibleTile in compatibleTiles)
                {
                    neighbourTilesCopy.Remove(compatibleTile);
                }

            }

            //If there are tiles remaining in tilecopy
            //then the propogation changed something in this neighbour
            if (neighbourTilesCopy.Count > 0)
                changed = true;

            //All the tiles that remain in the copy are not correct anymore
            //Delete them from the neighbour 
            foreach (var neighbourTile in neighbourTilesCopy)
            {
                _cells3D[neighbourIdx.x, neighbourIdx.y, neighbourIdx.z]._possibleTiles.Remove(neighbourTile);
            }
        }


        return changed;
    }

    private List<TileData> GetDeprecatedNeighbourTilesHorizontal(List<TileData> currentCellTiles, List<TileData> neighbourTilesCopy, CompareDirection compareDirection)
    {
        foreach (var tile in currentCellTiles)
        {
            List<TileData> compatibleTiles = new();

            HorizontalFaceData currentCellFace;
            switch (compareDirection)
            {
                case CompareDirection.PosX:
                    currentCellFace = tile._posX;
                    break;
                case CompareDirection.NegX:
                    currentCellFace = tile._negX;
                    break;
                case CompareDirection.PosZ:
                    currentCellFace = tile._posZ;
                    break;
                case CompareDirection.NegZ:
                    currentCellFace = tile._negZ;
                    break;
                default:
                    continue;
            }

            foreach (var neighbourTile in neighbourTilesCopy)
            {
                HorizontalFaceData neighbourCellFace;
                switch (compareDirection)
                {
                    case CompareDirection.PosX:
                        neighbourCellFace = neighbourTile._negX;
                        break;
                    case CompareDirection.NegX:
                        neighbourCellFace = neighbourTile._posX;
                        break;
                    case CompareDirection.PosZ:
                        neighbourCellFace = neighbourTile._negZ;
                        break;
                    case CompareDirection.NegZ:
                        neighbourCellFace = neighbourTile._posZ;
                        break;
                    default:
                        continue;
                }

                //Compare the tiles
                //Horizontal tiles match if:
                //  -> The socket id's match
                //
                //  -> Both are symmetrical
                //   OR
                //  -> The sockets are one is flipped and one is normal

                if (currentCellFace._socketID != neighbourCellFace._socketID)
                    continue;
                if ((currentCellFace._isSymmetric && neighbourCellFace._isSymmetric) ||
                    (currentCellFace._isFlipped != neighbourCellFace._isFlipped))
                {
                    compatibleTiles.Add(neighbourTile);
                }
            }

            //Remove all compatible tiles as to not double check them
            foreach (var compatibleTile in compatibleTiles)
            {
                neighbourTilesCopy.Remove(compatibleTile);
            }

        }

        return neighbourTilesCopy;
    }

    List<Vector3Int> GetNeighbours(Vector3Int cellCoords)
    {
        List<Vector3Int> neighbours = new();


        if (cellCoords.x - 1 >= 0)
            neighbours.Add(new Vector3Int(cellCoords.x - 1, cellCoords.y, cellCoords.z));
        if (cellCoords.x + 1 < _mapSize.x)
            neighbours.Add(new Vector3Int(cellCoords.x + 1, cellCoords.y, cellCoords.z));
        if (cellCoords.y - 1 >= 0)
            neighbours.Add(new Vector3Int(cellCoords.x, cellCoords.y - 1, cellCoords.z));
        if (cellCoords.y + 1 < _mapSize.y)
            neighbours.Add(new Vector3Int(cellCoords.x, cellCoords.y + 1, cellCoords.z));
        if (cellCoords.z - 1 >= 0)
            neighbours.Add(new Vector3Int(cellCoords.x, cellCoords.y, cellCoords.z - 1));
        if (cellCoords.z + 1 < _mapSize.z)
            neighbours.Add(new Vector3Int(cellCoords.x, cellCoords.y, cellCoords.z + 1));
        return neighbours;
    }
}
