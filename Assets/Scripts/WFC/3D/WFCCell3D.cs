using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WFCCell3D : MonoBehaviour
{
    public List<Module> _modules = new();

    private Module _collapsedModule = null;
    public Module CollapsedData
    {
        get
        {
            return _collapsedModule;
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
        return _modules.Count;
    }

    public void CollapseCell()
    {
        if (_modules.Count <= 0)
        {
            throw new UnityException($"{nameof(WFCCell3D)}, cell should get collapsed, but no tiles are possible!");
        }

        //Get random tile from the remaining ones
        int randomIndex = Random.Range(0, _modules.Count);
        var chosenTile = _modules[randomIndex];


        //Instantiate prefab
        var spawnedTile = Instantiate(chosenTile._prefab, this.transform);
        spawnedTile.transform.localPosition = Vector3.zero;

        //Add local rotation
        Quaternion newRot = Quaternion.Euler(0, chosenTile._tile._negY._rotationIndex* 90, 0);
        spawnedTile.transform.localRotation = newRot;

        _collapsedModule = chosenTile;
        _isCollapsed = true;

        _modules.Clear();
    }
}
