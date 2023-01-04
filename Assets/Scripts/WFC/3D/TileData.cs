using System;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

[Serializable]
public class TileData : MonoBehaviour
{
    public class FaceData
    {
        public int _socketID = 0;
    }
    [Serializable]
    public class HorizontalFaceData : FaceData
    {
        public bool _isFlipped = false;
        public bool _isSymmetric = false;

        public override string ToString()
        {
            return this._socketID.ToString() + (this._isFlipped ? "f" : (this._isSymmetric ? "s" : ""));
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
    }

    public TileData Clone()
    {
        MemoryStream ms = new MemoryStream();
        BinaryFormatter bf = new BinaryFormatter();

        bf.Serialize(ms, this);

        ms.Position = 0;
        object obj = bf.Deserialize(ms);
        ms.Close();

        return obj as TileData;
    }

    public HorizontalFaceData _posX;
    public HorizontalFaceData _negX;
    public HorizontalFaceData _posZ;
    public HorizontalFaceData _negZ;
    public VerticalFaceData _posY;
    public VerticalFaceData _negY;

    public float _tileSize = 2.0f;

    public Mesh GetMesh()
    {
        var _tileMesh = GetComponent<MeshFilter>();
        if (_tileMesh)
            return _tileMesh.sharedMesh;
        else
        {
            Debug.Log("No mesh available");
            return new Mesh();
        }
    }

    public Material[] GetMaterials()
    {
        var renderer = GetComponent<MeshRenderer>();
        if (renderer)
            return renderer.sharedMaterials;
        else
        {
            Debug.LogWarning("No materials found on renderer.");
            return null;
        }

    }


#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.gray;
        var pos = transform.position;
        pos.y += _tileSize / 2.0f;
        Gizmos.DrawWireCube(pos, new Vector3(_tileSize, _tileSize, _tileSize));
        DrawSocketIDs();
    }

    private void DrawSocketIDs()
    {
        GUIStyle style = new GUIStyle();
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
