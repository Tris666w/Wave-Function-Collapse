using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenshotInput : MonoBehaviour
{
    [Header("Input parameters")]
    [SerializeField] private Camera _camera;
    [Space(10)]
    [Header("Output parameters")]
    [SerializeField] private string _targetPath = "ScreenShots/";
    [SerializeField][Tooltip("Always appends to Application.dataPath!")] private string _fileName = "ScreenShot";
    [SerializeField] private bool _addTimeToFileName = true;

    private void Update()
    {
        if (Input.GetAxis("SaveScreenshot") != 0)
        {
            SaveCameraView();
        }
    }

    private void SaveCameraView()
    {
        //Set up the texture to render camera view to
        var screenTexture = new RenderTexture(Screen.width, Screen.height, 16);
        _camera.targetTexture = screenTexture;
        RenderTexture.active = screenTexture;

        //Render the camera view
        _camera.Render();

        //Transfer the texture to a texture2D object
        var renderedTexture = new Texture2D(Screen.width, Screen.height);
        renderedTexture.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
        RenderTexture.active = null;

        //Encode it to png and write it to
        var byteArray = renderedTexture.EncodeToPNG();
        if (_addTimeToFileName)
        {
            var time = "_" + System.DateTime.Now.Hour + "_" +
                          System.DateTime.Now.Minute + "_" +
                          System.DateTime.Now.Second + "_";

            var targetPath = Application.dataPath;
            targetPath = targetPath.Remove(targetPath.Length - 6,6);
            targetPath += _targetPath;
            System.IO.File.WriteAllBytes(targetPath + _fileName + time + ".png", byteArray);

        }
        else
            System.IO.File.WriteAllBytes(Application.dataPath + "/" + _targetPath + _fileName + ".png", byteArray);
    }
}
