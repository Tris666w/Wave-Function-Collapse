using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class represents one possible state for a cell in WFC 3D.
/// </summary>
[Serializable]
public class Module
{
    public string name = "Module";
    public TileData3D _tile;
    public GameObject _prefab;

    public Module(string name,
           TileData3D tileData3D,
           GameObject prefab)
    {
        this.name = name;
        this._tile = tileData3D;
        this._prefab = prefab;
    }

    public object Clone() => new Module(this.name, this._tile, this._prefab);
}
