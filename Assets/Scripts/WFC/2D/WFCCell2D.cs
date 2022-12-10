using System.Collections.Generic;
using UnityEngine;

public class WFCCell2D : MonoBehaviour
{
    public List<Sprite> _possibleTiles = new();
    private Sprite _collapsedTile = null;
    public Sprite CollapsedTile
    {
        get => _collapsedTile;
    }

    private bool _isCollapsed = false;
    public bool IsCollapsed
    {
        get => _isCollapsed;
    }


    public int GetEntropy()
    {
        return _possibleTiles.Count;
    }

    public void CollapseCell()
    {
        //Get random tile from the remaining ones
        int randomIndex = Random.Range(0, _possibleTiles.Count);
        _collapsedTile = _possibleTiles[randomIndex];

        _isCollapsed = true;

        //Since the cell is collapsed
        _possibleTiles.Clear();
        _possibleTiles.Add(CollapsedTile);
    }

}
