using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[CreateAssetMenu(fileName = "new collection", menuName = "WFC/ 3D Module collection")]
public class ModuleCollection3D : ScriptableObject
{
    [Tooltip("This prefab should contain all preferred tiles and eacht tile should have filled in TileData!")]
    [SerializeField] private GameObject _tilesPrefab;

    [SerializeField] private List<Module> _modules;
    public List<Module> Modules
    {
        get => _modules;
    }

    public void CreateModules()
    {
        if (_tilesPrefab == null)
            return;

        //Loop over each  child object
        foreach (Transform child in _tilesPrefab.transform)
        {
            //If no tiledata component is found or the tile is disabled, disregard this object
            var tileData = child.gameObject.GetComponent<TileData>();
            if (tileData == null || tileData.gameObject.activeSelf == false)
                continue;

            var tileDataArray = child.GetComponents<TileData>().ToList();
            if(tileDataArray.Count > 1)
            {
                tileDataArray.RemoveRange(1, tileDataArray.Count - 1);
            }

            string name;

            if (tileData._negY._isInvariant && tileData._posY._isInvariant)
            {
                //Generate a name and an i to indicate it's invariant
                name = tileData.name + "_i";
                //Create a new module for this invariant tile
                _modules.Add(new Module(name, tileData, child.gameObject));
            }
            else
            {
                //Generate a name and include rotation
                name = tileData.name + "_";

                string rotationIdx = "0";
                _modules.Add(new Module(name + rotationIdx, tileData, child.gameObject));

                //Generate a module for each rotation
                GenerateRotationVariants(name, tileData, child.gameObject);
            }
        }

    }


    private void GenerateRotationVariants(string name, TileData originalTileData, GameObject prefab)
    {
        //1 rotation index added == 90 degrees rotation around Y-axis

        //Generate rotation one
        string rot1name = name + "1";
        TileData rot1TileData = originalTileData.Clone();

        rot1TileData._posX = originalTileData._posZ;
        rot1TileData._negZ = originalTileData._posX;
        rot1TileData._negX = originalTileData._negZ;
        rot1TileData._posZ = originalTileData._negX;
        rot1TileData._negY._rotationIndex = 1;
        rot1TileData._posY._rotationIndex = 1;

        _modules.Add(new Module(rot1name, rot1TileData, prefab));

        //Generate rotation two
        string rot2name = name + "2";
        TileData rot2TileData = originalTileData.Clone();

        rot2TileData._posX = rot1TileData._posZ;
        rot2TileData._negZ = rot1TileData._posX;
        rot2TileData._negX = rot1TileData._negZ;
        rot2TileData._posZ = rot1TileData._negX;
        rot2TileData._negY._rotationIndex = 2;
        rot2TileData._posY._rotationIndex = 2;

        _modules.Add(new Module(rot2name, rot2TileData, prefab));

        //Generate rotation three
        string rot3name = name + "3";
        TileData rot3TileData = originalTileData.Clone();

        rot3TileData._posX = rot2TileData._posZ;
        rot3TileData._negZ = rot2TileData._posX;
        rot3TileData._negX = rot2TileData._negZ;
        rot3TileData._posZ = rot2TileData._negX;
        rot3TileData._negY._rotationIndex = 3;
        rot3TileData._posY._rotationIndex = 3;

        _modules.Add(new Module(rot3name, rot3TileData, prefab));
    }

    public void ResetModules()
    {
        _modules.Clear();
    }
}
