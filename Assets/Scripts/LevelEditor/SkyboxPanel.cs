using UnityEngine;
using System.Collections;
using System.IO;
using System.Runtime.InteropServices;

/*
 * Created by Geordie Powers on Feb 14 2015
 * 
 * Manages the Level Editor skybox GUI, and creates/updates the skybox material held in WorldInfo
 * 
 */

public class SkyboxPanel : MonoBehaviour
{
    private Material newSkyboxMaterial;
    private bool isActive = false;
    private string tempSide = "";

    void Awake()
    {
        newSkyboxMaterial = new Material(WorldInfo.info.worldSkybox);
    }

    public void ClearSkybox()
    {
        newSkyboxMaterial.SetTexture("_FrontTex", null);
        newSkyboxMaterial.SetTexture("_BackTex", null);
        newSkyboxMaterial.SetTexture("_LeftTex", null);
        newSkyboxMaterial.SetTexture("_RightTex", null);
        newSkyboxMaterial.SetTexture("_UpTex", null);
        newSkyboxMaterial.SetTexture("_DownTex", null);
        SaveSkybox();
        SetVisible(false);
    }

    public void SaveSkybox()
    {
        WorldInfo.info.worldSkybox = newSkyboxMaterial;
        WorldInfo.info.UpdateCameraSkyboxes();
        SetVisible(false);
    }

    public void OpenImageDialog(string side)
    {
        tempSide = side;
        FileSelection fs = EditorInfo.info.GetFileSelection();
        fs.gameObject.SetActive(true);
        fs.OnFileSelected += fs_OnFileSelected;
    }

    void fs_OnFileSelected(string value)
    {
        SetImage(tempSide, value);
    }

    private void SetImage(string side, string path)
    {
        // for now texture images must be 1024x1024
        // TODO: allow for any size -- but how can a texture be created if the size is unknown, and can't get the size until the image is loaded? Possibly use System.Drawing.Image?
        Texture2D imageFile = new Texture2D(1024, 1024);
        imageFile.LoadImage(OpenFile(path));

        // using numbers is a bit worrysome - maybe use strings to set material nameid
        switch (side)
        {
            case "Front":
                newSkyboxMaterial.SetTexture("_FrontTex", imageFile);
                break;
            case "Back":
                newSkyboxMaterial.SetTexture("_BackTex", imageFile);
                break;
            case "Left":
                newSkyboxMaterial.SetTexture("_LeftTex", imageFile);
                break;
            case "Right":
                newSkyboxMaterial.SetTexture("_RightTex", imageFile);
                break;
            case "Up":
                newSkyboxMaterial.SetTexture("_UpTex", imageFile);
                break;
            case "Down":
                newSkyboxMaterial.SetTexture("_DownTex", imageFile);
                break;
        }
    }

    private byte[] OpenFile(string path)
    {
        FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read);
        // TODO: if canread
        byte[] buffer;
        try
        {
            int streamLength = (int)stream.Length;
            buffer = new byte[streamLength];
            int count, sum = 0;
            while ((count = stream.Read(buffer, sum, streamLength - sum)) > 0) sum += count;
        }
        finally
        {
            stream.Close();
        }
        return buffer;
    }

    public void ShowHide()
    {
        isActive = !isActive;
        SetVisible(isActive);
    }

    public void SetVisible(bool visible)
    {
        isActive = visible;
        gameObject.SetActive(isActive);
    }
}