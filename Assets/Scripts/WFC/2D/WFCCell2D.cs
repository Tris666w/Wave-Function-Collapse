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

    public int GetEntropy()
    {
        return _possibleTiles.Count;
    }

    public void CollapseCell()
    {
        //Get random tile from the remaining ones
        int randomIndex = Random.Range(0, _possibleTiles.Count);
        _collapsedTile = _possibleTiles[randomIndex];

        //Since the cell is collapsed
        _possibleTiles.Clear();
    }

}
