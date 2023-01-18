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
    public WfcReturn CollapseCell(string moduleName)
    {
        WfcReturn result = new();

        var desiredModule = Modules.Find(x => moduleName == x.name);

        if (desiredModule == null)
        {
            result.returnState = WfcReturn.WfcReturnState.Error;
            result.returnContext = $"WFCCell3D, CollapseCell(string moduleName): Module with name = {moduleName} was not found.";
            return result;
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

        result.returnState = WfcReturn.WfcReturnState.Succes;
        return result;
    }

    /// <summary>
    /// Collapses the cell into its final state (tile).
    /// </summary>
    /// <exception cref="UnityException"> No modules available.</exception>
    public WfcReturn CollapseCell()
    {
        WfcReturn result = new();

        if (IsCollapsed)
        {
            result.returnState = WfcReturn.WfcReturnState.Warning;
            result.returnContext =$"{nameof(WFCCell3D)}, CollapseCell(): Trying to collapse an already collapsed cell!!";
            return result;
        }

        if (Modules.Count <= 0)
        {
            result.returnState = WfcReturn.WfcReturnState.Error;
            result.returnContext =$"{nameof(WFCCell3D)}, cell should get collapsed, but no tiles are possible!";
            return result;
        }

        if (UseTileWeights)
        {
            //Get the total weight of the remaining tiles
            var totalWeight = 0;
            Modules.ForEach(i => totalWeight += i._tile.Weight);

            //Get random number in the range [1,totalWeight]
            var targetWeight = Random.Range(1, totalWeight);

            //Loop over all the tiles
            foreach (var module in Modules)
            {
                if (targetWeight <= module._tile.Weight)
                {
                    result = CollapseCell(module.name);
                    return result;
                }

                targetWeight -= module._tile.Weight;
            }
        }
        else
        {
            //Get random tile from the remaining ones
            var randomIndex = Random.Range(0, Modules.Count);
            var chosenTile = Modules[randomIndex];

            result = CollapseCell(chosenTile.name);
            return result;
        }

        result.returnState = WfcReturn.WfcReturnState.Error;
        result.returnContext = "Failed to assign module to cell";
        return result;
    }
}
