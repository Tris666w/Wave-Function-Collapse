using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class WaveFunctionCollapse : MonoBehaviour
{
    [SerializeField] private Vector2Int _mapSize = new(10, 10);
    [SerializeField] private List<Sprite> _tiles = new();
    private WFCCell2D[] _cells;

    private bool _isCollapsed = false;

    private void Start()
    {
        _cells = new WFCCell2D[_mapSize.x * _mapSize.y];
        for (int index = 0; index < _cells.Length; index++)
        {
            _cells[index] = gameObject.AddComponent<WFCCell2D>();
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
                _isCollapsed = true;
                break;
            }

            //Collapse the cell
            cell.CollapseCell();

            //Propogate the changes to the other cells
            PropogateChanges(cell);
        }
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

}
