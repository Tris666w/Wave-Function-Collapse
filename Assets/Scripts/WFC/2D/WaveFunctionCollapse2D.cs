using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A generator for 2D map using the Wave Function Collapse Algorithm, using 2D sprites an input.
/// </summary>
public class WaveFunctionCollapse2D : MonoBehaviour
{
    [Header("Output Parameters")]
    [Tooltip("Desired output size.")] public Vector2Int MapSize = new(10, 10);

    [Space(10f)]
    [Header("Input parameters")]
    [Tooltip(" The size of a tile sprite in pixels.")][SerializeField] private float _tileSizePx = 1024;
    [Tooltip("The list of tiles to use with WFC.")][SerializeField] private List<Sprite> _tiles = new();

    [Space(10f)]
    [Header("Visualization parameters")]
    [Tooltip("The minimum time it takes to perform 1 loop of WFC.")][SerializeField] private float _stepTime = 1f;
    public float StepTime
    {
        set => _stepTime = value;
    }

    private GameObject _generatedMap;
    private WFCCell2D[,] _cells2D;

    /// <summary>
    /// Deletes the previously generated level (if any), stops current co routines and start the generation of a new level.
    /// </summary>
    public void GenerateLevel()
    {
        Debug.Log($"Generating 2D map of size: {MapSize}");

        //Reset the script
        AttemptDestroyResult();

        //Start the generation of the new map
        InitializeWave();
        StartCoroutine(CollapseWave());
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
    /// Initialize the wave object with WFCCell2D components and make the result game object.
    /// </summary>
    private void InitializeWave()
    {
        //Create the wave
        _cells2D = new WFCCell2D[MapSize.x, MapSize.y];

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
                targetPos.x += col * _tileSizePx / scale;
                targetPos.y += row * _tileSizePx / scale;
                go.transform.position = targetPos;

                //Add the cell class, save the component in the wave and add all the possible sprites
                _cells2D[col, row] = go.AddComponent<WFCCell2D>();
                _cells2D[col, row].PossibleTiles = new List<Sprite>(_tiles);
            }
    }

    /// <summary>
    /// Collapses the wave and generates the map. This is a co routine!
    /// </summary>
    /// <returns></returns>
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

    /// <summary>
    /// This function destroys all WFCCell2D components. 
    /// </summary>
    private void CleanUp()
    {
        for (var col = 0; col < MapSize.x; col++)
            for (var row = 0; row < MapSize.y; row++)
            {
                DestroyImmediate(_cells2D[col, row]);
            }
    }

    /// <summary>
    /// This function loops over all cells in the wave and looks for the cell with the lowest, non 0 entropy.
    /// </summary>
    /// <returns>Cell indices for the cell with the lowest entropy.</returns>
    private Vector2Int GetLowestEntropyCell()
    {
        //These objects will hold the cell with the lowest entropy
        var minEntropy = float.MaxValue;
        var lowestEntropyCell = new Vector2Int(-1, -1);

        //Loop over all the cells in the wave
        for (var x = 0; x < MapSize.x; x++)
            for (var y = 0; y < MapSize.y; y++)
            {
                //Ignore collapsed cells
                if (_cells2D[x, y].IsCollapsed)
                    continue;

                //Get the entropy of this cell
                float newEntropy = _cells2D[x, y].GetEntropy();

                //Add some randomness in case there would be multiple cells with the same entropy
                newEntropy += Random.Range(0f, 0.1f);

                //Replace the lowest entropy cell if the new cell has a lower one
                if (newEntropy < minEntropy)
                {
                    minEntropy = newEntropy;
                    lowestEntropyCell = new Vector2Int(x, y);
                }
            }
        return lowestEntropyCell;
    }

    /// <summary>
    /// This function propagates the collapsed cell to it's neighbors and removes impossible states from them.
    /// This loop repeats until all necessary cells have been notified of the collapsing of the original cell.
    /// </summary>
    /// <param name="originalCell">The array indices of the cell that has been collapsed.</param>
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

                //Detect the compatibilities and check if something changed
                var somethingChanged = CompareCells(currentCell, neighbor);

                if (somethingChanged)
                {
                    changedCells.Push(neighbor);
                }
            }
        }
    }

    /// <summary>
    /// This function compares all possible states of both cells and removes impossible states from the neighboring cell.
    /// </summary>
    /// <param name="currentCellIdx">Vector2Int representing the array indices of the original cell.</param>
    /// <param name="neighborIdx">Vector2Int representing the array indices of the neighboring cell.</param>
    /// <returns>A bool representing whether or not the neighbors possible states have changed.</returns>
    private bool CompareCells(Vector2Int currentCellIdx, Vector2Int neighborIdx)
    {
        var changed = false;

        //Sample points are the points on sprites which get color checked and compared.
        Vector2Int samplePoint1 = new();
        Vector2Int samplePoint2 = new();
        Vector2Int samplePoint3 = new();

        Vector2Int neighborSamplePoint1 = new();
        Vector2Int neighborSamplePoint2 = new();
        Vector2Int neighborSamplePoint3 = new();

        //Get the direction that needs to be checked
        //Set the sample points
        if (neighborIdx.x - currentCellIdx.x == 1)
        {
            //RIGHT 
            samplePoint1.x = (int)_tileSizePx - 1;
            samplePoint2.x = (int)_tileSizePx - 1;
            samplePoint3.x = (int)_tileSizePx - 1;

            neighborSamplePoint1.x = 1;
            neighborSamplePoint2.x = 1;
            neighborSamplePoint3.x = 1;

            samplePoint1.y = 1 * (int)_tileSizePx / 4;
            samplePoint2.y = 2 * (int)_tileSizePx / 4;
            samplePoint3.y = 3 * (int)_tileSizePx / 4;

            neighborSamplePoint1.y = 1 * (int)_tileSizePx / 4;
            neighborSamplePoint2.y = 2 * (int)_tileSizePx / 4;
            neighborSamplePoint3.y = 3 * (int)_tileSizePx / 4;

        }
        else if (neighborIdx.x - currentCellIdx.x == -1)
        {
            //LEFT 

            samplePoint1.x = 1;
            samplePoint2.x = 1;
            samplePoint3.x = 1;

            neighborSamplePoint1.x = (int)_tileSizePx - 1;
            neighborSamplePoint2.x = (int)_tileSizePx - 1;
            neighborSamplePoint3.x = (int)_tileSizePx - 1;

            samplePoint1.y = 1 * (int)_tileSizePx / 4;
            samplePoint2.y = 2 * (int)_tileSizePx / 4;
            samplePoint3.y = 3 * (int)_tileSizePx / 4;

            neighborSamplePoint1.y = 1 * (int)_tileSizePx / 4;
            neighborSamplePoint2.y = 2 * (int)_tileSizePx / 4;
            neighborSamplePoint3.y = 3 * (int)_tileSizePx / 4;
        }
        else if (neighborIdx.y - currentCellIdx.y == 1)
        {
            //UP 
            samplePoint1.y = (int)_tileSizePx - 1;
            samplePoint2.y = (int)_tileSizePx - 1;
            samplePoint3.y = (int)_tileSizePx - 1;

            neighborSamplePoint1.y = 1;
            neighborSamplePoint2.y = 1;
            neighborSamplePoint3.y = 1;

            samplePoint1.x = 1 * (int)_tileSizePx / 4;
            samplePoint2.x = 2 * (int)_tileSizePx / 4;
            samplePoint3.x = 3 * (int)_tileSizePx / 4;

            neighborSamplePoint1.x = 1 * (int)_tileSizePx / 4;
            neighborSamplePoint2.x = 2 * (int)_tileSizePx / 4;
            neighborSamplePoint3.x = 3 * (int)_tileSizePx / 4;
        }
        else if (neighborIdx.y - currentCellIdx.y == -1)
        {
            //DOWN
            samplePoint1.y = 1;
            samplePoint2.y = 1;
            samplePoint3.y = 1;

            neighborSamplePoint1.y = (int)_tileSizePx - 1;
            neighborSamplePoint2.y = (int)_tileSizePx - 1;
            neighborSamplePoint3.y = (int)_tileSizePx - 1;

            samplePoint1.x = 1 * (int)_tileSizePx / 4;
            samplePoint2.x = 2 * (int)_tileSizePx / 4;
            samplePoint3.x = 3 * (int)_tileSizePx / 4;

            neighborSamplePoint1.x = 1 * (int)_tileSizePx / 4;
            neighborSamplePoint2.x = 2 * (int)_tileSizePx / 4;
            neighborSamplePoint3.x = 3 * (int)_tileSizePx / 4;
        }

        //Make a copy of the neighbors possible tiles
        var tilesCopy = new List<Sprite>(_cells2D[neighborIdx.x, neighborIdx.y].PossibleTiles);

        var currentCell = _cells2D[currentCellIdx.x, currentCellIdx.y];
        var currentCellTiles = new List<Sprite>();

        if (!currentCell.IsCollapsed)
            currentCellTiles = currentCell.PossibleTiles;
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
            _cells2D[neighborIdx.x, neighborIdx.y].PossibleTiles.Remove(neighborTile);
        }


        return changed;
    }

    /// <summary>
    /// Get all neighbors of the given cell.
    /// </summary>
    /// <param name="cellCoords">Vector2Int representing the array indices of the original cell.</param>
    /// <returns>A list containing the array indices of the neighboring cells.</returns>
    private List<Vector2Int> GetNeighbors(Vector2Int cellCoords)
    {
        List<Vector2Int> neighbors = new();


        if (cellCoords.x - 1 >= 0)
            neighbors.Add(new Vector2Int(cellCoords.x - 1, cellCoords.y));
        if (cellCoords.x + 1 < MapSize.x)
            neighbors.Add(new Vector2Int(cellCoords.x + 1, cellCoords.y));
        if (cellCoords.y - 1 >= 0)
            neighbors.Add(new Vector2Int(cellCoords.x, cellCoords.y - 1));
        if (cellCoords.y + 1 < MapSize.y)
            neighbors.Add(new Vector2Int(cellCoords.x, cellCoords.y + 1));

        return neighbors;
    }
}
