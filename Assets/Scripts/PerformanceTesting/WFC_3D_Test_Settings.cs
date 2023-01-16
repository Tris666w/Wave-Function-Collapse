using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct WFC_3D_Test_Settings
{
    public string CaseName;
    public Vector3Int TestMapSize;
    public bool GenerateSolidFloor;
    public bool UseTileWeights;
    public bool UseMaterialAdjacency;
    public bool UseExcludedNeighborsAdjacency;
}
