using System.Collections.Generic;
using UnityEngine;

public class WFCCell2D : MonoBehaviour
{
    public List<TileData2D> PossibleTiles = new();

    public TileData2D CollapsedTile { get; private set; }

    public bool IsCollapsed { get; private set; } = false;

    /// <summary>
    /// Check how many possible states this cell can be in and return the entropy.
    /// </summary>
    /// <returns>An integer value representing the entropy of this cell.</returns>
    public int GetEntropy()
    {
        return PossibleTiles.Count;
    }

    /// <summary>
    /// Collapses the cell into its final state (tile).
    /// </summary>
    /// <param name="useTileWeights"> Do you want to use weighted choice to collapse the tile?</param>
    public void CollapseCell(bool useTileWeights)
    {

        if (useTileWeights)
        {
            //Get the total weight of the remaining tiles
            var totalWeight = 0;
            PossibleTiles.ForEach(i => totalWeight += i.Weight);

            //Get random number in the range [1,totalWeight]
            var targetWeight = Random.Range(1, totalWeight);

            //Loop over all the tiles
            foreach (var tile in PossibleTiles)
            {
                if (targetWeight <= tile.Weight)
                {
                    //Save the collapsed info
                    CollapsedTile = tile;
                    break;
                }

                targetWeight -= tile.Weight;
            }
        }
        else
        {
            //Get random tile from the remaining ones
            var randomIndex = Random.Range(0, PossibleTiles.Count);

            //Save the collapsed info
            CollapsedTile = PossibleTiles[randomIndex];
        }

        IsCollapsed = true;

        //Render the sprite
        var spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = CollapsedTile._tileSprite;

        //Clear the possible tiles list as the cell has been collapsed
        PossibleTiles.Clear();
    }

}
