using JetBrains.Annotations;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class WaveFunctionCollapse : MonoBehaviour
{
    [SerializeField] private Vector2Int _mapSize = new(10, 10);
    [SerializeField] private float _tileSize = 256f;
    [SerializeField] private List<Sprite> _tiles = new();
    [SerializeField] private bool _generate = false;

    private WFCCell2D[] _cells;



    private void Start()
    {
        _cells = new WFCCell2D[_mapSize.x * _mapSize.y];
        for (int index = 0; index < _cells.Length; index++)
        {
            _cells[index] = gameObject.AddComponent<WFCCell2D>();
            _cells[index]._possibleTiles = new List<Sprite>(_tiles);
        }
    }

    public void Update()
    {
        if (_generate)
        {
            _generate = false;
            CollapseWave();
        }
    }

    public void CollapseWave()
    {
        //While the wave is not yet collapsed
        while (true)
        {
            //Get the cell with the lowest entropy
            var cell = GetLowestEntropyCell();

            //If there is no cell with a low entropy that is not equal to 0,
            // The wave hall collapsed, break the loop
            if (cell == null)
            {
                break;
            }

            //Collapse the cell
            cell.CollapseCell();

            //Propogate the changes to the other cells
            PropogateChanges(cell);

        }

        GenerateSpritesInWorld();
    }

    private WFCCell2D GetLowestEntropyCell()
    {
        float _minEntropy = float.MaxValue;
        WFCCell2D lowestEntropyCell = null;

        foreach (var cell in _cells)
        {
            float newEntropy = cell.GetEntropy();

            //If the entropy is 0, the cell has been collapsed, so we disregard it
            if (Mathf.Approximately(newEntropy, 0f))
                continue;

            if (newEntropy < _minEntropy)
            {
                _minEntropy = newEntropy;
                lowestEntropyCell = cell;
            }
        }

        return lowestEntropyCell;
    }

    private void PropogateChanges(WFCCell2D cell)
    {

    }

    void GenerateSpritesInWorld()
    {
        var parent = new GameObject();
        parent.name = "Result";

        for (int index = 0; index < _cells.Length; index++)
        {
            var rowIdx = index / _mapSize.y;
            var colIdx = index % _mapSize.x;

            var go = new GameObject();
            var spriteRenderer = go.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = _cells[index].CollapsedTile;

            _cells[index].transform.parent = go.transform;

            var pos = new Vector3(rowIdx * _tileSize / 100.0f, colIdx * _tileSize / 100.0f, 0);
            go.transform.position = pos;

            go.transform.parent = parent.transform;
        }
    }
}
