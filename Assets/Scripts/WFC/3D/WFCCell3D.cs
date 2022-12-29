using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WFCCell3D : MonoBehaviour
{
    public List<TileData> _possibleTiles = new();
    private TileData _collapsedData = null;
    public TileData CollapsedData
    {
        get
        {
            return _collapsedData;
        }
    }
    private bool _isCollapsed = false;
    public bool IsCollapsed
    {
        get
        {
            return _isCollapsed;
        }
    }

    public int GetEntropy()
    {
        return _possibleTiles.Count;
    }

    public void CollapseCell()
    {
        if (_possibleTiles.Count <= 0)
            throw new UnityException($"{nameof(WFCCell3D)}, cell should get collapsed, but no tiles are possible!");

        //Get random tile from the remaining ones
        int randomIndex = Random.Range(0, _possibleTiles.Count);
        var chosenTile = _possibleTiles[randomIndex];


        GetComponent<MeshFilter>().mesh = chosenTile.GetMesh();
        var materials = chosenTile.GetMaterials();
        if (materials != null)
            GetComponent<MeshRenderer>().materials = materials;
        Debug.Log(chosenTile.transform.name);
        _isCollapsed = true;

        _possibleTiles.Clear();
        _possibleTiles.Add(chosenTile);
    }
}
