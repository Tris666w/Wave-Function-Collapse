using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[Serializable]
public class TileData3D : MonoBehaviour
{
    public enum TileMaterial
    {
        Grass, Sand, Rock
    }

    [Header("Tile material info")]
    public TileMaterial OwnMaterial;
    public List<TileMaterial> CompatibleMaterials = new();

    public class FaceData
    {
        [Header("Socket Info")]
        public int _socketID = 0;

    }
    [Serializable]
    public class HorizontalFaceData : FaceData
    {
        public bool _isFlipped = false;
        public bool _isSymmetric = false;

        public List<GameObject> ExcludedNeighbors = new();

        public override string ToString()
        {
            return this._socketID.ToString() + (this._isFlipped ? "f" : (this._isSymmetric ? "s" : ""));
        }

        public HorizontalFaceData Clone()
        {
            HorizontalFaceData clone = new();
            clone._isFlipped = this._isFlipped;
            clone._isSymmetric = this._isSymmetric;
            clone._socketID = this._socketID;
            clone.ExcludedNeighbors = this.ExcludedNeighbors;
            return clone;
        }

    }

    [Serializable]
    public class VerticalFaceData : FaceData
    {
        public bool _isInvariant = false;

        [Range(0, 3)]
        public int _rotationIndex = 0;

        public override string ToString()
        {
            return this._socketID.ToString() + (this._isInvariant ? "i" : "abcd".ElementAt(this._rotationIndex).ToString());
        }

        public VerticalFaceData Clone()
        {
            VerticalFaceData clone = new();
            clone._isInvariant = this._isInvariant;
            clone._rotationIndex = this._rotationIndex;
            clone._socketID = this._socketID;
            return clone;
        }
    }

    public TileData3D Clone()
    {
        var clone = gameObject.AddComponent<TileData3D>();
        clone._posX = this._posX.Clone();
        clone._negX = this._negX.Clone();
        clone._posZ = this._posZ.Clone();
        clone._negZ = this._negZ.Clone();
        clone._posY = this._posY.Clone();
        clone._negY = this._negY.Clone();
        clone._tileSize = this._tileSize;
        clone.OwnMaterial = this.OwnMaterial;
        clone.CompatibleMaterials = this.CompatibleMaterials;
        clone.Weight = this.Weight;
        return clone;
    }

    [Header("Tile data face parameters")]
    public HorizontalFaceData _posX;
    public HorizontalFaceData _negX;
    public HorizontalFaceData _posZ;
    public HorizontalFaceData _negZ;
    public VerticalFaceData _posY;
    public VerticalFaceData _negY;

    [Header("Tile data general parameters")]
    public float _tileSize = 2.0f;

    [Tooltip("This assures that rotated variants of this tile are generated, even if both vertical faces are invariant.")]
    public bool _generateRotatedVariants = false;

    public int Weight = 1;

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.gray;
        var pos = transform.position;
        pos.y += _tileSize / 2.0f;
        Gizmos.DrawWireCube(pos, new Vector3(_tileSize, _tileSize, _tileSize));
        DrawSocketIDs();
        DrawWeight();
    }

    private void DrawWeight()
    {
        var style = new GUIStyle
        {
            normal =
            {
                textColor = Color.red
            },
            fontStyle = FontStyle.Bold
        };

        var currentDrawPos = transform.position;
        currentDrawPos.y += _tileSize / 2.0f;
        Handles.Label(currentDrawPos, Weight.ToString(), style);
    }

    private void DrawSocketIDs()
    {
        var style = new GUIStyle();
        style.normal.textColor = Color.black;

        var currentDrawPos = transform.position;
        Handles.Label(currentDrawPos, _negY.ToString(), style);

        currentDrawPos.y += _tileSize;
        Handles.Label(currentDrawPos, _posY.ToString(), style);

        currentDrawPos.y -= _tileSize / 2f;
        currentDrawPos.x -= _tileSize / 2f;
        Handles.Label(currentDrawPos, _negX.ToString(), style);

        currentDrawPos.x += _tileSize;
        Handles.Label(currentDrawPos, _posX.ToString(), style);

        currentDrawPos.x -= _tileSize / 2f;
        currentDrawPos.z += _tileSize / 2f;
        Handles.Label(currentDrawPos, _posZ.ToString(), style);

        currentDrawPos.z -= _tileSize;
        Handles.Label(currentDrawPos, _negZ.ToString(), style);
    }
#endif
}
