using System.Collections.Generic;
using UnityEngine;

public class WaveFunctionCollapse2D : MonoBehaviour
{
    [SerializeField] private Vector2Int _mapSize = new(10, 10);
    [SerializeField] private float _tileSize = 256f;
    [SerializeField] private List<Sprite> _tiles = new();

    private WFCCell2D[,] _cells2D;

    [ContextMenu("Click me to generate a map!")]
    public void GenerateLevel()
    {
        InitializeWave();
        CollapseWave();
    }

    private void InitializeWave()
    {
        _cells2D = new WFCCell2D[_mapSize.x, _mapSize.y];
        for (int col = 0; col < _mapSize.x; col++)
            for (int row = 0; row < _mapSize.y; row++)
            {
                _cells2D[col, row] = gameObject.AddComponent<WFCCell2D>();
                _cells2D[col, row]._possibleTiles = new List<Sprite>(_tiles);
            }
    }

    public void CollapseWave()
    {
        //Get the cell with the lowest entropy to start the algorithm
        var cell = GetLowestEntropyCell();
        if (cell.x == -1 || cell.y == -1)
        {
            Debug.LogWarning("No cell with entropy found, before algorithm start? Stopping algorithm.");
            return;
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

            //If there is no cell with a low entropy that is not equal to 0,
            // The wave has collapsed, break the loop
        } while (cell.x != -1 || cell.y != -1);

        GenerateSpritesInWorld();

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
        var _minEntropy = float.MaxValue;
        var lowestEntropyCell = new Vector2Int(-1, -1);

        for (int x = 0; x < _mapSize.x; x++)
            for (int y = 0; y < _mapSize.y; y++)
            {
                float newEntropy = _cells2D[x, y].GetEntropy();

                //If the entropy is 0, the cell has been collapsed, so we disregard it
                if (Mathf.Approximately(newEntropy, 0f))
                    continue;

                if (newEntropy < _minEntropy)
                {
                    _minEntropy = newEntropy;
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


            //Get it's neighbours
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
        bool changed = false;


        int sampleStep = (int)_tileSize / 20;

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
            samplePoint1.x = 19 * sampleStep;
            samplePoint2.x = 19 * sampleStep;
            samplePoint3.x = 19 * sampleStep;

            neighborSamplePoint1.x = sampleStep;
            neighborSamplePoint2.x = sampleStep;
            neighborSamplePoint3.x = sampleStep;

            samplePoint1.y = 5 * sampleStep;
            samplePoint2.y = 10 * sampleStep;
            samplePoint3.y = 15 * sampleStep;

            neighborSamplePoint1.y = 5 * sampleStep;
            neighborSamplePoint2.y = 10 * sampleStep;
            neighborSamplePoint3.y = 15 * sampleStep;

        }
        else if (neighborIdx.x - currentCellIdx.x == -1)
        {
            //LEFT 

            samplePoint1.x = sampleStep;
            samplePoint2.x = sampleStep;
            samplePoint3.x = sampleStep;

            neighborSamplePoint1.x = 3 * sampleStep;
            neighborSamplePoint2.x = 3 * sampleStep;
            neighborSamplePoint3.x = 3 * sampleStep;

            samplePoint1.y = 1 * sampleStep;
            samplePoint2.y = 2 * sampleStep;
            samplePoint3.y = 3 * sampleStep;

            neighborSamplePoint1.y = 1 * sampleStep;
            neighborSamplePoint2.y = 2 * sampleStep;
            neighborSamplePoint3.y = 3 * sampleStep;
        }

        if (neighborIdx.y - currentCellIdx.y == 1)
        {
            //UP 
            samplePoint1.y = 19 * sampleStep;
            samplePoint2.y = 19 * sampleStep;
            samplePoint3.y = 19 * sampleStep;

            neighborSamplePoint1.y = sampleStep;
            neighborSamplePoint2.y = sampleStep;
            neighborSamplePoint3.y = sampleStep;

            samplePoint1.x = 5 * sampleStep;
            samplePoint2.x = 10 * sampleStep;
            samplePoint3.x = 15 * sampleStep;

            neighborSamplePoint1.x = 5 * sampleStep;
            neighborSamplePoint2.x = 10 * sampleStep;
            neighborSamplePoint3.x = 15 * sampleStep;
        }
        else if (neighborIdx.y - currentCellIdx.y == -1)
        {
            //DOWN
            samplePoint1.y = sampleStep;
            samplePoint2.y = sampleStep;
            samplePoint3.y = sampleStep;

            neighborSamplePoint1.y = 19 * sampleStep;
            neighborSamplePoint2.y = 19 * sampleStep;
            neighborSamplePoint3.y = 19 * sampleStep;

            samplePoint1.x = 5 * sampleStep;
            samplePoint2.x = 10 * sampleStep;
            samplePoint3.x = 15 * sampleStep;

            neighborSamplePoint1.x = 5 * sampleStep;
            neighborSamplePoint2.x = 10 * sampleStep;
            neighborSamplePoint3.x = 15 * sampleStep;
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

    private void GenerateSpritesInWorld()
    {
        var parent = new GameObject
        {
            name = "Result"
        };

        for (int x = 0; x < _mapSize.x; x++)
            for (int y = 0; y < _mapSize.y; y++)
            {
                var go = new GameObject();
                var spriteRenderer = go.AddComponent<SpriteRenderer>();
                spriteRenderer.sprite = _cells2D[x, y].CollapsedTile;

                _cells2D[x, y].transform.parent = go.transform;

                var pos = new Vector3(x * _tileSize / 100.0f, y * _tileSize / 100.0f, 0);
                go.transform.position = pos;
                go.transform.name = $"Tile x:{x} y:{y}";
                go.transform.parent = parent.transform;
            }

        transform.parent = null;
    }

    private List<Vector2Int> GetNeighbors(Vector2Int cellCoords)
    {
        List<Vector2Int> neighbors = new();


        if (cellCoords.x - 1 > 0)
            neighbors.Add(new Vector2Int(cellCoords.x - 1, cellCoords.y));
        if (cellCoords.x + 1 < _mapSize.x)
            neighbors.Add(new Vector2Int(cellCoords.x + 1, cellCoords.y));
        if (cellCoords.y - 1 > 0)
            neighbors.Add(new Vector2Int(cellCoords.x, cellCoords.y - 1));
        if (cellCoords.y + 1 < _mapSize.y)
            neighbors.Add(new Vector2Int(cellCoords.x, cellCoords.y + 1));

        return neighbors;
    }
}
