using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;

/// <summary>
/// This objects holds all modules for a WFC 3D algorithm. It generates the list from a prefab of tiles and generates the rotated variants.
/// Note: Generating the modules and rotation variants, changes the prefab directly!
/// </summary>
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

    /// <summary>
    /// Generates the list of modules. Uses all tiles in the _tilesPrefab and generates rotational variants of necessary.
    /// </summary>
    public void CreateModules()
    {
#if(UNITY_EDITOR)
        //Reset modules to assure there are no duplicates form previous generating
        ResetModules();
#endif
        if (_tilesPrefab == null)
            return;

        List<TileData3D> tileList = new();
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

            tileList.Add(tileData);
        }

        //Loop over each  child object
        foreach (var tileData in tileList)
        {
            string name;

            // Generate rotational variants if
            //  -> a vertical tile face is invariant
            //  OR
            //  -> Generate Rotated Variant is true
            if ((tileData._negY._isInvariant && tileData._posY._isInvariant) && !tileData._generateRotatedVariants)
            {
                //Generate a name and an i to indicate it's invariant
                name = tileData.name + "_i";

                //Calculate the spawn probability

                //Create a new module for this invariant tile
                _modules.Add(new Module(name, tileData, tileData.gameObject));
            }
            else
            {
                //Generate a name and include rotation
                name = tileData.name + "_";

                string rotationIdx = "0";

                //Calculate the spawn probability

                _modules.Add(new Module(name + rotationIdx, tileData, tileData.gameObject));

                //Generate a module for each rotation
                GenerateRotationVariants(name, tileData, tileData.gameObject);
            }
        }

    }

    /// <summary>
    /// Generates 3 rotational variants. 
    /// </summary>
    /// <param name="name">name of the original variant</param>
    /// <param name="originalTileData3D">The tile data of the original variant.</param>
    /// <param name="prefab">The prefab of the 3D mesh of this modules</param>
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

#if UNITY_EDITOR
    /// <summary>
    /// Removes all extra tile data objects from a tile prefab. Assures that there is only one when generating starts.
    /// </summary>
    private void RemoveGeneratedTileDate()
    {
        EditorUtility.SetDirty(_tilesPrefab);
        var versionObject = AssetDatabase.LoadAssetAtPath("Assets/Prefabs/TileCollections/TileCollectionSource/Tiles_Night.prefab", typeof(GameObject)) as GameObject;

        if (versionObject == null)
        {
            Debug.Log("Not found");
            return;
        }

        foreach (Transform child in versionObject.transform)
        {
            var tileDataArray = child.GetComponents<TileData3D>().ToList();
            if (tileDataArray.Count > 1)
            {
                for (var index = 1; index < tileDataArray.Count; index++)
                {
                    var tileComp = tileDataArray[index];
                    DestroyImmediate(tileComp, true);
                }
            }
        }
    }

    public void ResetModules()
    {
        RemoveGeneratedTileDate();
        _modules.Clear();
    }
#endif
}
