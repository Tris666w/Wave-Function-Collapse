using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[Serializable]
public class Module
{
    public string name = "Module";
    public TileData _tile;
    public GameObject _prefab;

    public Module(string name,
           TileData tileData,
           GameObject prefab)
    {
        this.name = name;
        this._tile = tileData;
        this._prefab = prefab;
    }

    public object Clone() => new Module(this.name, this._tile, this._prefab);
}
