using System;
using System.Collections.Generic;
using UnityEngine;

public class WaveFunctionCollapse : MonoBehaviour
{
    [SerializeField] private Vector2Int _mapSize = new(10, 10);
    [SerializeField] private float _tileSize = 256f;
    [SerializeField] private List<Sprite> _tiles = new();

    private WFCCell2D[,] _cells;

    [ContextMenu("Click me to generate a map!")]
    public void GenerateLevel()
    {
        InitializeWave();
        CollapseWave();
    }
    private void InitializeWave()
    {
        _cells = new WFCCell2D[_mapSize.x, _mapSize.y];
        for (int col = 0; col < _mapSize.x; col++)
            for (int row = 0; row < _mapSize.y; row++)
            {
                _cells[col, row] = gameObject.AddComponent<WFCCell2D>();
                _cells[col, row]._possibleTiles = new List<Sprite>(_tiles);
            }
    }

    public void CollapseWave()
    {
        Vector2Int cell = new(-1, -1);

        //Get the cell with the lowest entropy to start the algorithm
        cell = GetLowestEntropyCell();
        if (cell.x == -1 || cell.y == -1)
        {
            Debug.LogError("No cell with entropy found, before algorithm start?");
            return;
        }

        //Start collapsing the wave
        do
        {
            //Collapse the cell
            _cells[cell.x, cell.y].CollapseCell();

            //Propogate the changes to the other cells
            PropogateChanges(cell);

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
                DestroyImmediate(_cells[col, row]);
            }
    }

    private Vector2Int GetLowestEntropyCell()
    {
        float _minEntropy = float.MaxValue;
        var lowestEntropyCell = new Vector2Int(-1, -1);

        for (int x = 0; x < _mapSize.x; x++)
            for (int y = 0; y < _mapSize.y; y++)
            {
                float newEntropy = _cells[x, y].GetEntropy();

                //If the entropy is 1, the cell has been collapsed, so we disregard it
                if (Mathf.Approximately(newEntropy, 1f))
                    continue;

                if (newEntropy < _minEntropy)
                {
                    _minEntropy = newEntropy;
                    lowestEntropyCell = new Vector2Int(x, y);
                }
            }

        return lowestEntropyCell;
    }

    private void PropogateChanges(Vector2Int originalCell)
    {
        Stack<Vector2Int> changedCells = new();
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
                if (_cells[neighbour.x, neighbour.y].IsCollapsed)
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

    bool CompareCells(Vector2Int currentCell, Vector2Int neighbour)
    {
        bool changed = false;


        int sampleStep = (int)_tileSize / 20;

        Vector2Int samplePoint1 = new();
        Vector2Int samplePoint2 = new();
        Vector2Int samplePoint3 = new();

        Vector2Int neighbourSamplePoint1 = new();
        Vector2Int neighbourSamplePoint2 = new();
        Vector2Int neighbourSamplePoint3 = new();

        //Get the direction that needs to be checked
        if (neighbour.x - currentCell.x == 1)
        {
            //RIGHT 
            samplePoint1.x = 19 * sampleStep;
            samplePoint2.x = 19 * sampleStep;
            samplePoint3.x = 19 * sampleStep;

            neighbourSamplePoint1.x = sampleStep;
            neighbourSamplePoint2.x = sampleStep;
            neighbourSamplePoint3.x = sampleStep;

            samplePoint1.y = 5 * sampleStep;
            samplePoint2.y = 10 * sampleStep;
            samplePoint3.y = 15 * sampleStep;

            neighbourSamplePoint1.y = 5 * sampleStep;
            neighbourSamplePoint2.y = 10 * sampleStep;
            neighbourSamplePoint3.y = 15 * sampleStep;

        }
        else if (neighbour.x - currentCell.x == -1)
        {
            //LEFT 

            samplePoint1.x = sampleStep;
            samplePoint2.x = sampleStep;
            samplePoint3.x = sampleStep;

            neighbourSamplePoint1.x = 3 * sampleStep;
            neighbourSamplePoint2.x = 3 * sampleStep;
            neighbourSamplePoint3.x = 3 * sampleStep;

            samplePoint1.y = 1 * sampleStep;
            samplePoint2.y = 2 * sampleStep;
            samplePoint3.y = 3 * sampleStep;

            neighbourSamplePoint1.y = 1 * sampleStep;
            neighbourSamplePoint2.y = 2 * sampleStep;
            neighbourSamplePoint3.y = 3 * sampleStep;
        }

        if (neighbour.y - currentCell.y == 1)
        {
            //UP 
            samplePoint1.y = 19 * sampleStep;
            samplePoint2.y = 19 * sampleStep;
            samplePoint3.y = 19 * sampleStep;

            neighbourSamplePoint1.y = sampleStep;
            neighbourSamplePoint2.y = sampleStep;
            neighbourSamplePoint3.y = sampleStep;

            samplePoint1.x = 5 * sampleStep;
            samplePoint2.x = 10 * sampleStep;
            samplePoint3.x = 15 * sampleStep;

            neighbourSamplePoint1.x = 5 * sampleStep;
            neighbourSamplePoint2.x = 10 * sampleStep;
            neighbourSamplePoint3.x = 15 * sampleStep;
        }
        else if (neighbour.y - currentCell.y == -1)
        {
            //DOWN
            samplePoint1.y = sampleStep;
            samplePoint2.y = sampleStep;
            samplePoint3.y = sampleStep;

            neighbourSamplePoint1.y = 19 * sampleStep;
            neighbourSamplePoint2.y = 19 * sampleStep;
            neighbourSamplePoint3.y = 19 * sampleStep;

            samplePoint1.x = 5 * sampleStep;
            samplePoint2.x = 10 * sampleStep;
            samplePoint3.x = 15 * sampleStep;

            neighbourSamplePoint1.x = 5 * sampleStep;
            neighbourSamplePoint2.x = 10 * sampleStep;
            neighbourSamplePoint3.x = 15 * sampleStep;
        }


        //Make a copy of the neighbours possible tiles
        var tilesCopy = new List<Sprite>(_cells[neighbour.x, neighbour.y]._possibleTiles);

        //Loop over the possible tiles of the current tile
        foreach (var tile in _cells[currentCell.x, currentCell.y]._possibleTiles)
        {
            //Sample the colors
            var color1 = tile.texture.GetPixel(samplePoint1.x, samplePoint1.y);
            var color2 = tile.texture.GetPixel(samplePoint2.x, samplePoint2.y);
            var color3 = tile.texture.GetPixel(samplePoint3.x, samplePoint3.y);

            List<Sprite> compatibleSprite = new();

            //Loop over the remaining tiles in the copy of neighbour
            foreach (var neighbourTile in tilesCopy)
            {
                //Sample the colors
                var neighbourColor1 = neighbourTile.texture.GetPixel(neighbourSamplePoint1.x, neighbourSamplePoint1.y);
                var neighbourColor2 = neighbourTile.texture.GetPixel(neighbourSamplePoint2.x, neighbourSamplePoint2.y);
                var neighbourColor3 = neighbourTile.texture.GetPixel(neighbourSamplePoint3.x, neighbourSamplePoint3.y);

                //If the colours match, delete the tile from the copy
                if (color1 == neighbourColor1 && color2 == neighbourColor2 && color3 == neighbourColor3)
                    compatibleSprite.Add(neighbourTile);

            }

            foreach (var sprite in compatibleSprite)
            {
                tilesCopy.Remove(sprite);
            }

        }
        //If there are tiles remaining in tilecopy
        //then the propogation changed something in this neighbour
        if (tilesCopy.Count > 0)
            changed = true;

        //All the tiles that remain in the copy are not correct anymore
        //Delete them from the neighbour 
        foreach (var neighbourTile in tilesCopy)
        {
            _cells[neighbour.x, neighbour.y]._possibleTiles.Remove(neighbourTile);
        }


        return changed;
    }

    void GenerateSpritesInWorld()
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
                spriteRenderer.sprite = _cells[x, y].CollapsedTile;

                _cells[x, y].transform.parent = go.transform;

                var pos = new Vector3(x * _tileSize / 100.0f, y * _tileSize / 100.0f, 0);
                go.transform.position = pos;
                go.transform.name = $"Tile x:{x} y:{y}";
                go.transform.parent = parent.transform;
            }

        transform.parent = null;
    }

    List<Vector2Int> GetNeighbours(Vector2Int cellCoords)
    {
        List<Vector2Int> neighbours = new();


        if (cellCoords.x - 1 > 0)
            neighbours.Add(new Vector2Int(cellCoords.x - 1, cellCoords.y));
        if (cellCoords.x + 1 < _mapSize.x)
            neighbours.Add(new Vector2Int(cellCoords.x + 1, cellCoords.y));
        if (cellCoords.y - 1 > 0)
            neighbours.Add(new Vector2Int(cellCoords.x, cellCoords.y - 1));
        if (cellCoords.y + 1 < _mapSize.y)
            neighbours.Add(new Vector2Int(cellCoords.x, cellCoords.y + 1));

        return neighbours;
    }
}
