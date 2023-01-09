using System.Collections.Generic;
using UnityEngine;

public class WFCCell2D : MonoBehaviour
{
    public List<Sprite> PossibleTiles = new();

    private Sprite _collapsedTile = null;
    public Sprite CollapsedTile
    {
        get => _collapsedTile;
    }

    private bool _isCollapsed = false;
    public bool IsCollapsed
    {
        get => _isCollapsed;
    }

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
    public void CollapseCell()
    {
        //Get random tile from the remaining ones
        var randomIndex = Random.Range(0, PossibleTiles.Count);

        //Save the collapsed info
        _collapsedTile = PossibleTiles[randomIndex];
        _isCollapsed = true;

        //Render the sprite
        var spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = _collapsedTile;

        //Clear the possible tiles list as the cell has been collapsed
        PossibleTiles.Clear();
    }

}
