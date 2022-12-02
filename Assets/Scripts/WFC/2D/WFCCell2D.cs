using System.Collections.Generic;
using UnityEngine;

public class WFCCell2D : MonoBehaviour
{
    private List<Sprite> _possibleNeighbours = new();


    public WFCCell2D(List<Sprite> possibleNeighbours)
    {
        _possibleNeighbours = possibleNeighbours;
    }

    public int GetEntropy()
    {
        return _possibleNeighbours.Count;
    }

    public void CollapseCell()
    {

    }

}
