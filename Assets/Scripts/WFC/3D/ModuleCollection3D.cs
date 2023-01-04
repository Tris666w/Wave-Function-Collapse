using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Unity.VisualScripting;
using UnityEngine;
using static TileData;

[CreateAssetMenu(fileName = "new collection", menuName = "WFC/ 3D Module collection")]
public class ModuleCollection3D : ScriptableObject
{
    [Tooltip("This prefab should contain all preferred tiles and eacht tile should have filled in TileData!")]
    [SerializeField] private GameObject _tilesPrefab;
    public List<Module> _modules;

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

            string name;

            if (tileData._negY._isInvariant && tileData._posY._isInvariant)
            {
                //Generate a name and an i to indicate it's invariant
                name = tileData.name + "_i";
            }
            else
            {
                //Generate a name and include rotation
                name = tileData.name + "_";

                GenerateRotationVariants(name, tileData, child.gameObject);

                name += tileData._negY._rotationIndex;
            }



            var newModule = new Module(name, tileData, child.gameObject);
            _modules.Add(newModule);
        }

    }

    private void GenerateRotationVariants(string name, TileData originalTileData, GameObject prefab)
    {
        //Generate rotation one
        string rot1name = name + "1";
        TileData rot1TileData = originalTileData.Clone();
        TileData temp = originalTileData.Clone();

        rot1TileData._posX = temp._posZ;
        rot1TileData._negZ = temp._posX;
        rot1TileData._negX = temp._negZ;
        rot1TileData._posZ = temp._negX;
        rot1TileData._negY._rotationIndex = 1;
        rot1TileData._posY._rotationIndex = 1;

        _modules.Add(new Module(rot1name, rot1TileData, prefab));

        //Generate rotation two
        string rot2name = name + "2";
        TileData rot2TileData = originalTileData.Clone();
        temp = rot1TileData.Clone();

        rot2TileData._posX = temp._posZ;
        rot2TileData._negZ = temp._posX;
        rot2TileData._negX = temp._negZ;
        rot2TileData._posZ = temp._negX;
        rot2TileData._negY._rotationIndex = 2;
        rot2TileData._posY._rotationIndex = 2;

        _modules.Add(new Module(rot2name, rot2TileData, prefab));

        //Generate rotation three
        string rot3name = name + "3";
        TileData rot3TileData = originalTileData.Clone();
        temp = rot2TileData.Clone();

        rot3TileData._posX = temp._posZ;
        rot3TileData._negZ = temp._posX;
        rot3TileData._negX = temp._negZ;
        rot3TileData._posZ = temp._negX;
        rot3TileData._negY._rotationIndex = 3;
        rot3TileData._posY._rotationIndex = 3;

        _modules.Add(new Module(rot3name, rot3TileData, prefab));
    }

    public void ResetModules()
    {
        _modules.Clear();
    }
}
