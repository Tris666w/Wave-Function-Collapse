using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class WaveFunctionCollapse2D : MonoBehaviour
{
    [SerializeField] private Vector2Int _mapSize = new(10, 10);

    public Vector2Int MapSize
    {
        set => _mapSize = value;
    }

    [SerializeField] private float _tileSize = 256f;
    [SerializeField] private List<Sprite> _tiles = new();
    [SerializeField] private float _stepTime = 1f;
    public float StepTime
    {
        set => _stepTime = value;
    }


    private GameObject _generatedMap;
    private WFCCell2D[,] _cells2D;

    public void GenerateLevel()
    {
        Debug.Log($"Generating 2D map of size: {_mapSize}");
        StopAllCoroutines();
        AttemptDestroyResult();

        InitializeWave();
        StartCoroutine(CollapseWave());
    }

    public void AttemptDestroyResult()
    {
        Debug.Log("Destroying generated 2D level");
        StopAllCoroutines();
        if (_generatedMap != null)
            Destroy(_generatedMap);
    }

    private void InitializeWave()
    {
        //Create the wave
        _cells2D = new WFCCell2D[_mapSize.x, _mapSize.y];

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

        for (var col = 0; col < _mapSize.x; col++)
            for (var row = 0; row < _mapSize.y; row++)
            {
                //Create the game object that will hold this cell and parent it's transform to the result
                var go = new GameObject($"x = {col}, y = {row}")
                {
                    transform =
                    {
                        parent = resultObject.transform
                    }
                };

                //Translate the cell to the correct position
                var targetPos = transform.position;
                var scale = 100;
                targetPos.x += col * _tileSize / scale;
                targetPos.y += row * _tileSize / scale;
                go.transform.position = targetPos;

                //Add the cell class, save the component in the wave and add all the possible sprites
                _cells2D[col, row] = go.AddComponent<WFCCell2D>();
                _cells2D[col, row]._possibleTiles = new List<Sprite>(_tiles);
            }
    }

    public IEnumerator CollapseWave()
    {
        //Get the cell with the lowest entropy to start the algorithm
        var cell = GetLowestEntropyCell();
        if (cell.x == -1 || cell.y == -1)
        {
            Debug.LogWarning("No cell with entropy found, before algorithm start? Stopping algorithm.");
            yield break;
        }

        //Start collapsing the wave
        do
        {
            //Collapse the cell
            _cells2D[cell.x, cell.y].CollapseCell();

            //Propagate the changes to the other cells
            PropagateChanges(cell);

            //Get the next cell with the lowest entropy
            cell = GetLowestEntropyCell();

            //Wait the co routine
            if (!Mathf.Approximately(_stepTime, 0))
                yield return new WaitForSeconds(_stepTime);

            //If there is no cell with a low entropy that is not equal to 0,
            // The wave has collapsed, break the loop
        } while (cell.x != -1 || cell.y != -1);

        CleanUp();
    }

    private void CleanUp()
    {
        for (int col = 0; col < _mapSize.x; col++)
            for (int row = 0; row < _mapSize.y; row++)
            {
                DestroyImmediate(_cells2D[col, row]);
            }
    }

    private Vector2Int GetLowestEntropyCell()
    {
        var minEntropy = float.MaxValue;
        var lowestEntropyCell = new Vector2Int(-1, -1);

        for (var x = 0; x < _mapSize.x; x++)
            for (var y = 0; y < _mapSize.y; y++)
            {
                float newEntropy = _cells2D[x, y].GetEntropy();

                //If the entropy is 0, the cell has been collapsed, so we disregard it
                if (Mathf.Approximately(newEntropy, 0f))
                    continue;

                //Add some randomness in case there would be multiple cells with the same entropy
                newEntropy += Random.Range(0f, 0.1f);

                if (newEntropy < minEntropy)
                {
                    minEntropy = newEntropy;
                    lowestEntropyCell = new Vector2Int(x, y);
                }
            }

        return lowestEntropyCell;
    }

    private void PropagateChanges(Vector2Int originalCell)
    {
        Stack<Vector2Int> changedCells = new();
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
                if (_cells2D[neighbor.x, neighbor.y].IsCollapsed)
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

    bool CompareCells(Vector2Int currentCellIdx, Vector2Int neighborIdx)
    {
        var changed = false;

        Vector2Int samplePoint1 = new();
        Vector2Int samplePoint2 = new();
        Vector2Int samplePoint3 = new();

        Vector2Int neighborSamplePoint1 = new();
        Vector2Int neighborSamplePoint2 = new();
        Vector2Int neighborSamplePoint3 = new();

        //Get the direction that needs to be checked
        if (neighborIdx.x - currentCellIdx.x == 1)
        {
            //RIGHT 
            samplePoint1.x = (int)_tileSize - 1;
            samplePoint2.x = (int)_tileSize - 1;
            samplePoint3.x = (int)_tileSize - 1;

            neighborSamplePoint1.x = 1;
            neighborSamplePoint2.x = 1;
            neighborSamplePoint3.x = 1;

            samplePoint1.y = 1 * (int)_tileSize / 4;
            samplePoint2.y = 2 * (int)_tileSize / 4;
            samplePoint3.y = 3 * (int)_tileSize / 4;

            neighborSamplePoint1.y = 1 * (int)_tileSize / 4;
            neighborSamplePoint2.y = 2 * (int)_tileSize / 4;
            neighborSamplePoint3.y = 3 * (int)_tileSize / 4;

        }
        else if (neighborIdx.x - currentCellIdx.x == -1)
        {
            //LEFT 

            samplePoint1.x = 1;
            samplePoint2.x = 1;
            samplePoint3.x = 1;

            neighborSamplePoint1.x = (int)_tileSize - 1;
            neighborSamplePoint2.x = (int)_tileSize - 1;
            neighborSamplePoint3.x = (int)_tileSize - 1;

            samplePoint1.y = 1 * (int)_tileSize / 4;
            samplePoint2.y = 2 * (int)_tileSize / 4;
            samplePoint3.y = 3 * (int)_tileSize / 4;

            neighborSamplePoint1.y = 1 * (int)_tileSize / 4;
            neighborSamplePoint2.y = 2 * (int)_tileSize / 4;
            neighborSamplePoint3.y = 3 * (int)_tileSize / 4;
        }

        if (neighborIdx.y - currentCellIdx.y == 1)
        {
            //UP 
            samplePoint1.y = (int)_tileSize - 1;
            samplePoint2.y = (int)_tileSize - 1;
            samplePoint3.y = (int)_tileSize - 1;

            neighborSamplePoint1.y = 1;
            neighborSamplePoint2.y = 1;
            neighborSamplePoint3.y = 1;

            samplePoint1.x = 1 * (int)_tileSize / 4;
            samplePoint2.x = 2 * (int)_tileSize / 4;
            samplePoint3.x = 3 * (int)_tileSize / 4;

            neighborSamplePoint1.x = 1 * (int)_tileSize / 4;
            neighborSamplePoint2.x = 2 * (int)_tileSize / 4;
            neighborSamplePoint3.x = 3 * (int)_tileSize / 4;
        }
        else if (neighborIdx.y - currentCellIdx.y == -1)
        {
            //DOWN
            samplePoint1.y = 1;
            samplePoint2.y = 1;
            samplePoint3.y = 1;

            neighborSamplePoint1.y = (int)_tileSize - 1;
            neighborSamplePoint2.y = (int)_tileSize - 1;
            neighborSamplePoint3.y = (int)_tileSize - 1;

            samplePoint1.x = 1 * (int)_tileSize / 4;
            samplePoint2.x = 2 * (int)_tileSize / 4;
            samplePoint3.x = 3 * (int)_tileSize / 4;

            neighborSamplePoint1.x = 1 * (int)_tileSize / 4;
            neighborSamplePoint2.x = 2 * (int)_tileSize / 4;
            neighborSamplePoint3.x = 3 * (int)_tileSize / 4;
        }


        //Make a copy of the neighbors possible tiles
        var tilesCopy = new List<Sprite>(_cells2D[neighborIdx.x, neighborIdx.y]._possibleTiles);

        var currentCell = _cells2D[currentCellIdx.x, currentCellIdx.y];
        var currentCellTiles = new List<Sprite>();

        if (!currentCell.IsCollapsed)
            currentCellTiles = currentCell._possibleTiles;
        else
            currentCellTiles.Add(currentCell.CollapsedTile);

        //Loop over the possible tiles of the current tile
        foreach (var tile in currentCellTiles)
        {
            //Sample the colors
            var color1 = tile.texture.GetPixel(samplePoint1.x, samplePoint1.y);
            var color2 = tile.texture.GetPixel(samplePoint2.x, samplePoint2.y);
            var color3 = tile.texture.GetPixel(samplePoint3.x, samplePoint3.y);

            List<Sprite> compatibleSprite = new();

            //Loop over the remaining tiles in the copy of neighborIdx
            foreach (var neighborTile in tilesCopy)
            {
                //Sample the colors
                var neighborColor1 = neighborTile.texture.GetPixel(neighborSamplePoint1.x, neighborSamplePoint1.y);
                var neighborColor2 = neighborTile.texture.GetPixel(neighborSamplePoint2.x, neighborSamplePoint2.y);
                var neighborColor3 = neighborTile.texture.GetPixel(neighborSamplePoint3.x, neighborSamplePoint3.y);

                //If the colors match, delete the tile from the copy
                if (color1 == neighborColor1 && color2 == neighborColor2 && color3 == neighborColor3)
                    compatibleSprite.Add(neighborTile);

            }

            foreach (var sprite in compatibleSprite)
            {
                tilesCopy.Remove(sprite);
            }

        }
        //If there are tiles remaining in tile copy
        //then the propagation changed something in this neighborIdx
        if (tilesCopy.Count > 0)
            changed = true;

        //All the tiles that remain in the copy are not correct anymore
        //Delete them from the neighborIdx 
        foreach (var neighborTile in tilesCopy)
        {
            _cells2D[neighborIdx.x, neighborIdx.y]._possibleTiles.Remove(neighborTile);
        }


        return changed;
    }

    private List<Vector2Int> GetNeighbors(Vector2Int cellCoords)
    {
        List<Vector2Int> neighbors = new();


        if (cellCoords.x - 1 >= 0)
            neighbors.Add(new Vector2Int(cellCoords.x - 1, cellCoords.y));
        if (cellCoords.x + 1 < _mapSize.x)
            neighbors.Add(new Vector2Int(cellCoords.x + 1, cellCoords.y));
        if (cellCoords.y - 1 >= 0)
            neighbors.Add(new Vector2Int(cellCoords.x, cellCoords.y - 1));
        if (cellCoords.y + 1 < _mapSize.y)
            neighbors.Add(new Vector2Int(cellCoords.x, cellCoords.y + 1));

        return neighbors;
    }
}
