using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class WFCCell3D : MonoBehaviour
{
    public List<Module> Modules = new();

    public Module CollapsedData { get; private set; } = null;

    public bool IsCollapsed { get; private set; } = false;

    public double Entropy { get; private set; }

    public bool UseTileWeights = false;


    /// <summary>
    /// Recalculates the entropy. Uses Summation of the logarithm (base 2) of the weights if tile weights are enables otherwise uses the count of modules left.
    /// </summary>
    public void RecalculateEntropy()
    {
        if (UseTileWeights)
        {
            Entropy = 0;
            Modules.ForEach(x => Entropy += -Math.Log(x._tile.Weight, 2));
        }
        else
        {
            Entropy = Modules.Count;
        }
    }

    /// <summary>
    /// Collapses the cell into the module where module.name == moduleName.
    /// Note: return without assigning if module was not found.
    /// </summary>
    public void CollapseCell(string moduleName)
    {
        var desiredModule = Modules.Find(x => moduleName == x.name);

        if (desiredModule == null)
        {
            Debug.LogWarning($"WFCCell3D, CollapseCell(string moduleName): Module with name = {moduleName} was not found. \nExiting function.");
            return;
        }

        //Instantiate prefab
        var spawnedTile = Instantiate(desiredModule._prefab, this.transform);
        spawnedTile.transform.localPosition = Vector3.zero;

        //Add local rotation
        var newRot = Quaternion.Euler(0, desiredModule._tile._negY._rotationIndex * 90, 0);
        spawnedTile.transform.localRotation = newRot;

        CollapsedData = desiredModule;
        IsCollapsed = true;

        Modules.Clear();

    }

    /// <summary>
    /// Collapses the cell into its final state (tile).
    /// </summary>
    /// <param name="useTileWeights"> Do you want to use weighted choice to collapse the tile?</param>
    /// <exception cref="UnityException"> No modules available.</exception>
    public void CollapseCell()
    {
        if (IsCollapsed)
        {
            Debug.LogWarning($"{nameof(WFCCell3D)}, CollapseCell(): Trying to collapse an already collapsed cell!!");
            return;
        }

        if (Modules.Count <= 0)
        {
            throw new UnityException($"{nameof(WFCCell3D)}, cell should get collapsed, but no tiles are possible!");
        }

        
        //Get random tile from the remaining ones
        var randomIndex = Random.Range(0, Modules.Count);
        var chosenTile = Modules[randomIndex];

        CollapseCell(chosenTile.name);
        
    }
}
