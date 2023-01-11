using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WFCCell3D : MonoBehaviour
{
    public List<Module> Modules = new();

    public Module CollapsedData { get; private set; } = null;

    public bool IsCollapsed { get; private set; } = false;

    /// <summary>
    /// Check how many possible states this cell can be in and return the entropy.
    /// </summary>
    /// <returns>An integer value representing the entropy of this cell.</returns>
    public int GetEntropy()
    {
        return Modules.Count;
    }

    /// <summary>
    /// Collapses the cell into an empty module.
    /// </summary>
    public void CollapseCell(string moduleName)
    {
        var desiredModule = Modules.Find(x => moduleName == x.name);

        if (desiredModule == null)
            return;

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
    public void CollapseCell(bool useTileWeights)
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

        if (useTileWeights)
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
                    CollapseCell(module.name);
                    return;
                }

                targetWeight -= module._tile.Weight;
            }
        }
        else
        {
            //Get random tile from the remaining ones
            var randomIndex = Random.Range(0, Modules.Count);
            var chosenTile = Modules[randomIndex];

            CollapseCell(chosenTile.name);
        }
    }
}
