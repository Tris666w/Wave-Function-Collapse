using UnityEngine;
using System.Linq;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "new collection", menuName = "WFC/ 3D Module collection")]
public class ModuleCollection3D : ScriptableObject
{
    [Tooltip("This prefab should contain all preferred tiles and each tile should have filled in TileData3D!")]
    [SerializeField] private GameObject _tilesPrefab;

    [SerializeField] private List<Module> _modules;
    public List<Module> Modules
    {
        get => _modules;
    }

    public void CreateModules()
    {
        //Reset modules to assure there are no duplicates form previous generating
        ResetModules();

        if (_tilesPrefab == null)
            return;


        //Loop over each  child object
        foreach (Transform child in _tilesPrefab.transform)
        {
            //If no tile data component is found or the tile is disabled, disregard this object
            var tileData = child.gameObject.GetComponent<TileData3D>();
            if (tileData == null || tileData.gameObject.activeSelf == false)
                continue;

            var tileDataArray = child.GetComponents<TileData3D>().ToList();
            if (tileDataArray.Count > 1)
            {
                for (var index = 1; index < tileDataArray.Count; index++)
                {
                    DestroyImmediate(tileDataArray[index], true);
                }

            }

            string name;

            // Generate rotational variants if
            //  -> a vertical tile face is invariant
            //  OR
            //  -> Generate Rotated Variant is true
            if ((tileData._negY._isInvariant && tileData._posY._isInvariant) && !tileData._generateRotatedVariants)
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

    private void GenerateRotationVariants(string name, TileData3D originalTileData3D, GameObject prefab)
    {
        //1 rotation index added == 90 degrees rotation around Y-axis

        //Generate rotation one
        string rot1name = name + "1";
        TileData3D rot1TileData3D = originalTileData3D.Clone();

        rot1TileData3D._posX = originalTileData3D._posZ;
        rot1TileData3D._negZ = originalTileData3D._posX;
        rot1TileData3D._negX = originalTileData3D._negZ;
        rot1TileData3D._posZ = originalTileData3D._negX;
        rot1TileData3D._negY._rotationIndex = 1;
        rot1TileData3D._posY._rotationIndex = 1;

        _modules.Add(new Module(rot1name, rot1TileData3D, prefab));

        //Generate rotation two
        string rot2name = name + "2";
        TileData3D rot2TileData3D = originalTileData3D.Clone();

        rot2TileData3D._posX = rot1TileData3D._posZ;
        rot2TileData3D._negZ = rot1TileData3D._posX;
        rot2TileData3D._negX = rot1TileData3D._negZ;
        rot2TileData3D._posZ = rot1TileData3D._negX;
        rot2TileData3D._negY._rotationIndex = 2;
        rot2TileData3D._posY._rotationIndex = 2;

        _modules.Add(new Module(rot2name, rot2TileData3D, prefab));

        //Generate rotation three
        string rot3name = name + "3";
        TileData3D rot3TileData3D = originalTileData3D.Clone();

        rot3TileData3D._posX = rot2TileData3D._posZ;
        rot3TileData3D._negZ = rot2TileData3D._posX;
        rot3TileData3D._negX = rot2TileData3D._negZ;
        rot3TileData3D._posZ = rot2TileData3D._negX;
        rot3TileData3D._negY._rotationIndex = 3;
        rot3TileData3D._posY._rotationIndex = 3;

        _modules.Add(new Module(rot3name, rot3TileData3D, prefab));
    }


    private void RemoveGeneratedTileDate()
    {
        foreach (Transform child in _tilesPrefab.transform)
        {
            var tileDataArray = child.GetComponents<TileData3D>().ToList();
            if (tileDataArray.Count > 1)
            {
                for (var index = 1; index < tileDataArray.Count; index++)
                {
                    DestroyImmediate(tileDataArray[index] .gameObject, true);
                }

            }
        }
    }

    public void ResetModules()
    {
        RemoveGeneratedTileDate();
        _modules.Clear();
    }
}
